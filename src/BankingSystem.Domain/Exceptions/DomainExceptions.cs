namespace BankingSystem.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientFundsException : DomainException
{
    public decimal Requested { get; }
    public decimal Available { get; }

    public InsufficientFundsException(decimal requested, decimal available)
        : base($"Недостатньо коштів: запитано {requested:C}, доступно {available:C}")
    {
        Requested = requested;
        Available = available;
    }
}

public class AccountNotFoundException : DomainException
{
    public Guid AccountId { get; }

    public AccountNotFoundException(Guid accountId)
        : base($"Рахунок {accountId} не знайдено")
    {
        AccountId = accountId;
    }
}

public class CustomerNotFoundException : DomainException
{
    public Guid CustomerId { get; }

    public CustomerNotFoundException(Guid customerId)
        : base($"Клієнта {customerId} не знайдено")
    {
        CustomerId = customerId;
    }
}

public class InvalidAmountException : DomainException
{
    public InvalidAmountException(decimal amount)
        : base($"Некоректна сума: {amount}. Сума повинна бути більше нуля.") { }
}
