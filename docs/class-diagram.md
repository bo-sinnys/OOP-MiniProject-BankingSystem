# Class Diagram — BankingSystem

```mermaid
classDiagram
    class Customer {
        +Guid Id
        +string FullName
        +string Email
        +DateTime CreatedAt
        +Customer(string fullName, string email)
    }

    class Account {
        <<abstract>>
        +Guid Id
        +Guid CustomerId
        +decimal Balance
        +List~Transaction~ Transactions
        +Deposit(decimal amount) void
        +Withdraw(decimal amount) void
        +GetTransactionHistory() IReadOnlyList~Transaction~
    }

    class CheckingAccount {
        +decimal OverdraftLimit
        +decimal OverdraftFee
        +Withdraw(decimal amount) void
    }

    class SavingsAccount {
        +decimal InterestRate
        +decimal MinimumBalance
        +ApplyInterest(decimal amount) void
    }

    class Transaction {
        +Guid Id
        +Guid AccountId
        +decimal Amount
        +TransactionType Type
        +string Description
        +DateTime OccurredAt
        +Transaction(Guid accountId, decimal amount, TransactionType type, string description)
    }

    class TransactionType {
        <<enumeration>>
        Deposit
        Withdrawal
        Transfer
        Interest
        LoanDisbursement
        LoanPayment
    }

    class IRepository~T~ {
        <<interface>>
        +GetById(Guid id) T?
        +GetAll() IReadOnlyList~T~
        +Save(T entity) void
        +Delete(Guid id) void
    }

    class IInterestCalculator {
        <<interface>>
        +Calculate(decimal balance, decimal rate, int days) decimal
    }

    class AccountService {
        -IRepository~Account~ _accountRepo
        -IRepository~Customer~ _customerRepo
        +AccountService(IRepository~Account~ accounts, IRepository~Customer~ customers)
        +Transfer(Guid fromId, Guid toId, decimal amount) void
        +Deposit(Guid accountId, decimal amount) void
        +Withdraw(Guid accountId, decimal amount) void
        +GetAccount(Guid id) Account
        +OpenAccount(Guid customerId, AccountType type) Account
    }

    class InMemoryAccountRepository {
        -Dictionary~Guid, Account~ _store
        +GetById(Guid id) Account?
        +GetAll() IReadOnlyList~Account~
        +Save(Account entity) void
        +Delete(Guid id) void
    }

    class InsufficientFundsException {
        +decimal Requested
        +decimal Available
    }

    class AccountNotFoundException {
        +Guid AccountId
    }

    Account <|-- CheckingAccount : extends
    Account <|-- SavingsAccount : extends
    Account "1" --> "*" Transaction : contains
    Account "*" --> "1" Customer : belongs to
    IRepository~T~ <|.. InMemoryAccountRepository : implements
    AccountService --> IRepository~Account~ : depends on
    AccountService --> IRepository~Customer~ : depends on
    SavingsAccount ..> IInterestCalculator : uses (Lab 35)
```

## Нотатки до діаграми

- `Account` — абстрактний клас; `Withdraw` перевизначається в підкласах (поліморфізм).
- `CheckingAccount.Withdraw` дозволяє від'ємний баланс до `OverdraftLimit`, але нараховує `OverdraftFee`.
- `IRepository<T>` — контракт між Application і Infrastructure; дозволяє підміняти in-memory на JSON без зміни сервісів.
- `IInterestCalculator` залишений як заготовка для Lab 35 (Strategy pattern).
- `AccountNotFoundException` і `InsufficientFundsException` наслідують `DomainException`.
