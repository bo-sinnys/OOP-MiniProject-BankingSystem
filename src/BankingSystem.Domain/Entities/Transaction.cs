namespace BankingSystem.Domain.Entities;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    Interest,
    LoanDisbursement,
    LoanPayment
}

public class Transaction
{
    public Guid Id { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public TransactionType Type { get; }
    public string Description { get; }
    public DateTime OccurredAt { get; }

    public Transaction(Guid accountId, decimal amount, TransactionType type, string description)
    {
        if (amount <= 0)
            throw new ArgumentException("Сума транзакції повинна бути більше нуля.", nameof(amount));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Опис транзакції не може бути порожнім.", nameof(description));

        Id = Guid.NewGuid();
        AccountId = accountId;
        Amount = amount;
        Type = type;
        Description = description;
        OccurredAt = DateTime.UtcNow;
    }

    public override string ToString() =>
        $"[{OccurredAt:yyyy-MM-dd HH:mm}] {Type,-12} {Amount,10:C}  {Description}";
}
