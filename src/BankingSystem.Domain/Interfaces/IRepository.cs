using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.Interfaces;

// Узагальнений репозиторій — контракт між Application і Infrastructure
public interface IRepository<T>
{
    T? GetById(Guid id);
    IReadOnlyList<T> GetAll();
    void Save(T entity);
    void Delete(Guid id);
}

// Спеціалізований контракт для рахунків з додатковими запитами
public interface IAccountRepository : IRepository<Account>
{
    IReadOnlyList<Account> GetByCustomerId(Guid customerId);
    IReadOnlyList<Account> GetAllSavingsAccounts();
}

// Заготовка для Strategy pattern (Lab 35)
public interface IInterestCalculator
{
    decimal Calculate(decimal balance, decimal annualRate, int days);
}
