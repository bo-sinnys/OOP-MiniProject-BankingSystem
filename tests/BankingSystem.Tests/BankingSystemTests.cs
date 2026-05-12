using BankingSystem.Application.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Exceptions;
using BankingSystem.Infrastructure.Repositories;
using Xunit;

namespace BankingSystem.Tests;

public class AccountTests
{
    // ── CheckingAccount ───────────────────────────────────────────────────────

    [Fact]
    public void CheckingAccount_Deposit_IncreasesBalance()
    {
        var acc = new CheckingAccount(Guid.NewGuid());
        acc.Deposit(1000m);
        Assert.Equal(1000m, acc.Balance);
    }

    [Fact]
    public void CheckingAccount_Withdraw_DecreasesBalance()
    {
        var acc = new CheckingAccount(Guid.NewGuid());
        acc.Deposit(500m);
        acc.Withdraw(200m);
        Assert.Equal(300m, acc.Balance);
    }

    [Fact]
    public void CheckingAccount_Withdraw_WithinOverdraft_Succeeds_AndChargesFee()
    {
        // Ліміт овердрафту 500, комісія 50
        var acc = new CheckingAccount(Guid.NewGuid(), overdraftLimit: 500m, overdraftFee: 50m);
        acc.Deposit(100m);
        acc.Withdraw(400m); // баланс стане -300, потім -350 (комісія)

        Assert.Equal(-350m, acc.Balance);
        Assert.Equal(2, acc.Transactions.Count(t => t.Type == TransactionType.Withdrawal));
    }

    [Fact]
    public void CheckingAccount_Withdraw_BeyondOverdraft_ThrowsInsufficientFunds()
    {
        var acc = new CheckingAccount(Guid.NewGuid(), overdraftLimit: 200m);
        acc.Deposit(100m);

        var ex = Assert.Throws<InsufficientFundsException>(() => acc.Withdraw(400m));
        Assert.Equal(400m, ex.Requested);
    }

    [Fact]
    public void CheckingAccount_Deposit_ZeroAmount_ThrowsInvalidAmount()
    {
        var acc = new CheckingAccount(Guid.NewGuid());
        Assert.Throws<InvalidAmountException>(() => acc.Deposit(0m));
    }

    [Fact]
    public void CheckingAccount_Deposit_NegativeAmount_ThrowsInvalidAmount()
    {
        var acc = new CheckingAccount(Guid.NewGuid());
        Assert.Throws<InvalidAmountException>(() => acc.Deposit(-100m));
    }

    // ── SavingsAccount ────────────────────────────────────────────────────────

    [Fact]
    public void SavingsAccount_Withdraw_BelowMinimum_ThrowsDomainException()
    {
        var acc = new SavingsAccount(Guid.NewGuid(), minimumBalance: 1000m);
        acc.Deposit(1500m);

        Assert.Throws<DomainException>(() => acc.Withdraw(600m)); // залишилось б 900 < 1000
    }

    [Fact]
    public void SavingsAccount_ApplyInterest_IncreasesBalance()
    {
        var acc = new SavingsAccount(Guid.NewGuid(), interestRate: 0.05m);
        acc.Deposit(2000m);
        acc.ApplyInterest(100m);

        Assert.Equal(2100m, acc.Balance);
        Assert.Contains(acc.Transactions, t => t.Type == TransactionType.Interest);
    }

    [Fact]
    public void SavingsAccount_Withdraw_ExactBalance_ThrowsIfBelowMinimum()
    {
        var acc = new SavingsAccount(Guid.NewGuid(), minimumBalance: 500m);
        acc.Deposit(500m);

        // Спроба зняти все — залишилось б 0 < 500
        Assert.Throws<DomainException>(() => acc.Withdraw(500m));
    }

    // ── Customer ──────────────────────────────────────────────────────────────

    [Fact]
    public void Customer_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Customer("", "test@bank.ua"));
    }

    [Fact]
    public void Customer_InvalidEmail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Customer("Аліса", "not-an-email"));
    }

    [Fact]
    public void Customer_ValidData_CreatesSuccessfully()
    {
        var c = new Customer("Іван Франко", "ivan@bank.ua");
        Assert.Equal("іван франко", c.FullName.ToLower()); // перевіряємо trim
        Assert.NotEqual(Guid.Empty, c.Id);
    }

    // ── Transaction ───────────────────────────────────────────────────────────

    [Fact]
    public void Transaction_ZeroAmount_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Transaction(Guid.NewGuid(), 0m, TransactionType.Deposit, "test"));
    }

    [Fact]
    public void Transaction_RecordsCorrectly()
    {
        var accountId = Guid.NewGuid();
        var t = new Transaction(accountId, 500m, TransactionType.Transfer, "Переказ");

        Assert.Equal(accountId, t.AccountId);
        Assert.Equal(500m, t.Amount);
        Assert.Equal(TransactionType.Transfer, t.Type);
    }
}

// ── AccountService integration-like tests ─────────────────────────────────────

public class AccountServiceTests
{
    private AccountService CreateService(out InMemoryAccountRepository accountRepo,
                                         out InMemoryCustomerRepository customerRepo)
    {
        accountRepo  = new InMemoryAccountRepository();
        customerRepo = new InMemoryCustomerRepository();
        return new AccountService(accountRepo, customerRepo);
    }

    [Fact]
    public void Transfer_HappyPath_UpdatesBothBalances()
    {
        var service = CreateService(out _, out _);

        var alice = service.RegisterCustomer("Аліса", "alice@b.ua");
        var bob   = service.RegisterCustomer("Боб",   "bob@b.ua");

        var fromAcc = service.OpenAccount(alice.Id, AccountType.Checking);
        var toAcc   = service.OpenAccount(bob.Id,   AccountType.Checking);

        service.Deposit(fromAcc.Id, 1000m);
        service.Transfer(fromAcc.Id, toAcc.Id, 400m);

        Assert.Equal(600m, service.GetAccount(fromAcc.Id).Balance);
        Assert.Equal(400m, service.GetAccount(toAcc.Id).Balance);
    }

    [Fact]
    public void Transfer_SameAccount_ThrowsDomainException()
    {
        var service = CreateService(out _, out _);
        var customer = service.RegisterCustomer("Аліса", "alice@b.ua");
        var acc = service.OpenAccount(customer.Id, AccountType.Checking);
        service.Deposit(acc.Id, 500m);

        Assert.Throws<DomainException>(() => service.Transfer(acc.Id, acc.Id, 100m));
    }

    [Fact]
    public void Transfer_InsufficientFunds_ThrowsAndDoesNotChangeBalances()
    {
        var service = CreateService(out _, out _);

        var alice = service.RegisterCustomer("Аліса", "alice@b.ua");
        var bob   = service.RegisterCustomer("Боб",   "bob@b.ua");

        // SavingsAccount не має овердрафту
        var from = service.OpenAccount(alice.Id, AccountType.Savings);
        var to   = service.OpenAccount(bob.Id,   AccountType.Savings);

        service.Deposit(from.Id, 200m);

        Assert.Throws<InsufficientFundsException>(() => service.Transfer(from.Id, to.Id, 500m));

        // Баланси не змінились (крім мінімального балансу — перевірка бізнес-правила)
        // from має 200, намагались зняти 500 — виняток кинуто, баланс не змінений
        Assert.Equal(200m, service.GetAccount(from.Id).Balance);
        Assert.Equal(0m, service.GetAccount(to.Id).Balance);
    }

    [Fact]
    public void OpenAccount_UnknownCustomer_ThrowsCustomerNotFoundException()
    {
        var service = CreateService(out _, out _);
        Assert.Throws<CustomerNotFoundException>(() =>
            service.OpenAccount(Guid.NewGuid(), AccountType.Checking));
    }

    [Fact]
    public void GetStatement_FiltersCorrectly()
    {
        var service = CreateService(out _, out _);
        var customer = service.RegisterCustomer("Аліса", "alice@b.ua");
        var acc = service.OpenAccount(customer.Id, AccountType.Checking);

        service.Deposit(acc.Id, 1000m);
        service.Withdraw(acc.Id, 200m);

        var deposits = service.GetStatement(acc.Id, TransactionType.Deposit);
        var withdrawals = service.GetStatement(acc.Id, TransactionType.Withdrawal);

        Assert.Single(deposits);
        Assert.Single(withdrawals);
    }
}
