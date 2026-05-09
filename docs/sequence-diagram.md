# Sequence Diagram — UC1: Переказ між рахунками

```mermaid
sequenceDiagram
    actor Operator as Оператор
    participant Menu as ConsoleMenu
    participant Service as AccountService
    participant FromAcc as Account (відправник)
    participant ToAcc as Account (отримувач)
    participant Repo as IRepository~Account~

    Operator->>Menu: Обирає "Переказ"
    Menu->>Operator: Запитує ID відправника
    Operator->>Menu: Вводить fromAccountId
    Menu->>Operator: Запитує ID отримувача
    Operator->>Menu: Вводить toAccountId
    Menu->>Operator: Запитує суму
    Operator->>Menu: Вводить amount

    Menu->>Service: Transfer(fromId, toId, amount)

    Service->>Repo: GetById(fromId)
    Repo-->>Service: fromAccount (або null)

    alt fromAccount == null
        Service-->>Menu: throw AccountNotFoundException
        Menu-->>Operator: "Рахунок відправника не знайдено"
    end

    Service->>Repo: GetById(toId)
    Repo-->>Service: toAccount (або null)

    alt toAccount == null
        Service-->>Menu: throw AccountNotFoundException
        Menu-->>Operator: "Рахунок отримувача не знайдено"
    end

    Service->>FromAcc: Withdraw(amount)

    alt Недостатньо коштів (і не CheckingAccount з овердрафтом)
        FromAcc-->>Service: throw InsufficientFundsException
        Service-->>Menu: propagate exception
        Menu-->>Operator: "Недостатньо коштів: є X, потрібно Y"
    end

    FromAcc->>FromAcc: Додає Transaction(Withdrawal)
    FromAcc-->>Service: OK

    Service->>ToAcc: Deposit(amount)
    ToAcc->>ToAcc: Додає Transaction(Deposit)
    ToAcc-->>Service: OK

    Service->>Repo: Save(fromAccount)
    Service->>Repo: Save(toAccount)

    Service-->>Menu: void (успіх)
    Menu-->>Operator: "Переказ виконано. Баланс відправника: X грн"
```

## Ключові рішення

| Рішення | Обґрунтування |
|---------|---------------|
| `AccountService` не знає про конкретний репозиторій | DIP — залежить від `IRepository<Account>`, не від `InMemoryAccountRepository` |
| Виняток кидає `Account.Withdraw`, не сервіс | Інваріант рахунку захищається самим доменним об'єктом (SRP) |
| Дві окремі транзакції (Withdrawal + Deposit) | Аудит-трейл повний; кожна операція атомарно записана на свій рахунок |
| Операція не атомарна між Save(from) і Save(to) | Свідоме обмеження Lab 34; транзакційність — Lab 36 |
