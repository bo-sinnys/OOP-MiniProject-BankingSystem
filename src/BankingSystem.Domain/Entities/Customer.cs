namespace BankingSystem.Domain.Entities;

public class Customer
{
    public Guid Id { get; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt { get; }

    public Customer(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Ім'я клієнта не може бути порожнім.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Некоректна електронна адреса.", nameof(email));

        Id = Guid.NewGuid();
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains('@'))
            throw new ArgumentException("Некоректна електронна адреса.", nameof(newEmail));
        Email = newEmail.Trim().ToLowerInvariant();
    }

    public override string ToString() => $"{FullName} ({Email})";
}
