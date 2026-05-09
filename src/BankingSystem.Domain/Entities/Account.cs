using BankingSystem.Domain.Exceptions;

namespace BankingSystem.Domain.Entities;

public enum AccountType { Checking, Savings }

public abstract class Account
{
    private readonly List<Transaction> _transactions = new();

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public decimal Balance { get; protected set; }
    public AccountType Type { get; }
    public DateTime OpenedAt { get; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    protected Account(Guid customerId, AccountType type, decimal initialBalance = 0)
    {
        if (initialBalance < 0)
            throw new ArgumentException("Початковий баланс не може бути від'ємним.", nameof(initialBalance));

        Id = Guid.NewGuid();
        CustomerId = customerId;
        Type = type;
        Balance = initialBalance;
        OpenedAt = DateTime.UtcNow;
    }

    public virtual void Deposit(decimal amount)
    {
        ValidateAmount(amount);
        Balance += amount;
        RecordTransaction(amount, TransactionType.Deposit, $"Поповнення рахунку на {amount:C}");
    }

    public abstract void Withdraw(decimal amount);

    public void Close()
    {
        if (!IsActive)
            throw new DomainException("Рахунок вже закрито.");
        if (Balance != 0)
            throw new DomainException($"Неможливо закрити рахунок з ненульовим балансом ({Balance:C}).");
        IsActive = false;
    }

    public IReadOnlyList<Transaction> GetTransactionHistory() => _transactions.AsReadOnly();

    protected void RecordTransaction(decimal amount, TransactionType type, string description)
    {
        _transactions.Add(new Transaction(Id, amount, type, description));
    }

    protected static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidAmountException(amount);
    }

    public override string ToString() =>
        $"[{Type}] {Id.ToString()[..8]}... | Баланс: {Balance:C} | Відкрито: {OpenedAt:yyyy-MM-dd}";
}

// ──────────────────────────────────────────────────────────────────────────────

public class CheckingAccount : Account
{
    public decimal OverdraftLimit { get; }
    public decimal OverdraftFee { get; }

    public CheckingAccount(Guid customerId, decimal overdraftLimit = 500m, decimal overdraftFee = 50m)
        : base(customerId, AccountType.Checking)
    {
        if (overdraftLimit < 0)
            throw new ArgumentException("Ліміт овердрафту не може бути від'ємним.", nameof(overdraftLimit));
        OverdraftLimit = overdraftLimit;
        OverdraftFee = overdraftFee;
    }

    public override void Withdraw(decimal amount)
    {
        ValidateAmount(amount);

        decimal effectiveBalance = Balance + OverdraftLimit;
        if (amount > effectiveBalance)
            throw new InsufficientFundsException(amount, effectiveBalance);

        Balance -= amount;
        RecordTransaction(amount, TransactionType.Withdrawal, $"Зняття {amount:C}");

        // Нараховуємо штраф за овердрафт якщо баланс пішов у мінус
        if (Balance < 0 && OverdraftFee > 0)
        {
            Balance -= OverdraftFee;
            RecordTransaction(OverdraftFee, TransactionType.Withdrawal, $"Комісія за овердрафт {OverdraftFee:C}");
        }
    }
}

// ──────────────────────────────────────────────────────────────────────────────

public class SavingsAccount : Account
{
    public decimal InterestRate { get; }
    public decimal MinimumBalance { get; }

    public SavingsAccount(Guid customerId, decimal interestRate = 0.05m, decimal minimumBalance = 1000m)
        : base(customerId, AccountType.Savings)
    {
        if (interestRate < 0 || interestRate > 1)
            throw new ArgumentException("Відсоткова ставка повинна бути від 0 до 1.", nameof(interestRate));
        if (minimumBalance < 0)
            throw new ArgumentException("Мінімальний баланс не може бути від'ємним.", nameof(minimumBalance));

        InterestRate = interestRate;
        MinimumBalance = minimumBalance;
    }

    public override void Withdraw(decimal amount)
    {
        ValidateAmount(amount);

        if (amount > Balance)
            throw new InsufficientFundsException(amount, Balance);

        decimal resultingBalance = Balance - amount;
        if (resultingBalance < MinimumBalance)
            throw new DomainException(
                $"Знімання призведе до балансу {resultingBalance:C}, що нижче мінімального ({MinimumBalance:C}).");

        Balance -= amount;
        RecordTransaction(amount, TransactionType.Withdrawal, $"Зняття {amount:C}");
    }

    // Заготовка для Lab 35 — InterestService викличе цей метод
    public void ApplyInterest(decimal interestAmount)
    {
        ValidateAmount(interestAmount);
        Balance += interestAmount;
        RecordTransaction(interestAmount, TransactionType.Interest,
            $"Нарахування відсотків ({InterestRate:P0} річних)");
    }
}
