using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Exceptions;
using BankingSystem.Domain.Interfaces;

namespace BankingSystem.Infrastructure.Repositories;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<Guid, Account> _store = new();

    public Account? GetById(Guid id) =>
        _store.TryGetValue(id, out var account) ? account : null;

    public IReadOnlyList<Account> GetAll() =>
        _store.Values.ToList().AsReadOnly();

    public void Save(Account entity) =>
        _store[entity.Id] = entity;

    public void Delete(Guid id)
    {
        if (!_store.Remove(id))
            throw new AccountNotFoundException(id);
    }

    public IReadOnlyList<Account> GetByCustomerId(Guid customerId) =>
        _store.Values.Where(a => a.CustomerId == customerId).ToList().AsReadOnly();

    public IReadOnlyList<Account> GetAllSavingsAccounts() =>
        _store.Values.OfType<SavingsAccount>().Cast<Account>().ToList().AsReadOnly();
}

public class InMemoryCustomerRepository : IRepository<Customer>
{
    private readonly Dictionary<Guid, Customer> _store = new();

    public Customer? GetById(Guid id) =>
        _store.TryGetValue(id, out var customer) ? customer : null;

    public IReadOnlyList<Customer> GetAll() =>
        _store.Values.ToList().AsReadOnly();

    public void Save(Customer entity) =>
        _store[entity.Id] = entity;

    public void Delete(Guid id)
    {
        if (!_store.Remove(id))
            throw new CustomerNotFoundException(id);
    }
}
