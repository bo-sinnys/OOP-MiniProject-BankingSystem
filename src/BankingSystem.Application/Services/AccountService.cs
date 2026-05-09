using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Exceptions;
using BankingSystem.Domain.Interfaces;

namespace BankingSystem.Application.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepo;
    private readonly IRepository<Customer> _customerRepo;

    // DIP: залежимо від абстракцій, не від конкретних класів
    public AccountService(IAccountRepository accountRepo, IRepository<Customer> customerRepo)
    {
        _accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
        _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
    }

    /// <summary>Відкрити новий рахунок для існуючого клієнта.</summary>
    public Account OpenAccount(Guid customerId, AccountType type)
    {
        var customer = _customerRepo.GetById(customerId)
            ?? throw new CustomerNotFoundException(customerId);

        Account account = type switch
        {
            AccountType.Checking => new CheckingAccount(customerId),
            AccountType.Savings  => new SavingsAccount(customerId),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Невідомий тип рахунку: {type}")
        };

        _accountRepo.Save(account);
        return account;
    }

    /// <summary>Поповнити рахунок.</summary>
    public void Deposit(Guid accountId, decimal amount)
    {
        var account = GetAccountOrThrow(accountId);
        account.Deposit(amount);
        _accountRepo.Save(account);
    }

    /// <summary>Зняти кошти з рахунку.</summary>
    public void Withdraw(Guid accountId, decimal amount)
    {
        var account = GetAccountOrThrow(accountId);
        account.Withdraw(amount);
        _accountRepo.Save(account);
    }

    /// <summary>Переказ між двома рахунками — головний UC1.</summary>
    public void Transfer(Guid fromAccountId, Guid toAccountId, decimal amount)
    {
        if (fromAccountId == toAccountId)
            throw new DomainException("Рахунок відправника і отримувача не можуть збігатися.");

        var from = GetAccountOrThrow(fromAccountId);
        var to   = GetAccountOrThrow(toAccountId);

        // Бізнес-правила перевіряються в домені (from.Withdraw кине виняток при нестачі)
        from.Withdraw(amount);
        to.Deposit(amount);

        _accountRepo.Save(from);
        _accountRepo.Save(to);
    }

    /// <summary>Отримати рахунок або кинути виняток.</summary>
    public Account GetAccount(Guid accountId) => GetAccountOrThrow(accountId);

    /// <summary>Отримати всі рахунки клієнта.</summary>
    public IReadOnlyList<Account> GetCustomerAccounts(Guid customerId) =>
        _accountRepo.GetByCustomerId(customerId);

    /// <summary>Отримати виписку — транзакції за рахунком, відфільтровані за типом.</summary>
    public IReadOnlyList<Transaction> GetStatement(Guid accountId, TransactionType? filterType = null)
    {
        var account = GetAccountOrThrow(accountId);
        var history = account.GetTransactionHistory();

        return filterType.HasValue
            ? history.Where(t => t.Type == filterType.Value).ToList().AsReadOnly()
            : history.ToList().AsReadOnly();
    }

    /// <summary>Зареєструвати нового клієнта.</summary>
    public Customer RegisterCustomer(string fullName, string email)
    {
        var customer = new Customer(fullName, email);
        _customerRepo.Save(customer);
        return customer;
    }

    private Account GetAccountOrThrow(Guid accountId) =>
        _accountRepo.GetById(accountId) ?? throw new AccountNotFoundException(accountId);
}
