# BankingSystem

Консольна банківська система — підсумковий міні-проєкт з курсу ООП (варіант 5).

## Поточний стан

**Ітерація 1 (Lab 34)** — baseline: доменна модель, перший вертикальний зріз (переказ між рахунками), юніт-тести, CI.

## Запуск

```bash
git clone <repo-url>
cd BankingSystem
dotnet run --project src/BankingSystem.Console
```

## Запуск тестів

```bash
dotnet test
```

## Структура проєкту

```
BankingSystem/
├── src/
│   ├── BankingSystem.Domain/           # Сутності, інтерфейси, винятки
│   │   ├── Entities/                   # Account, Customer, Transaction
│   │   ├── Interfaces/                 # IRepository<T>, IInterestCalculator
│   │   └── Exceptions/                 # DomainException і нащадки
│   ├── BankingSystem.Application/      # Сервіси та бізнес-логіка
│   │   └── Services/AccountService.cs
│   ├── BankingSystem.Infrastructure/   # Репозиторії (in-memory → Lab35: JSON)
│   │   └── Repositories/
│   └── BankingSystem.Console/          # Консольний UI, меню
│       └── Program.cs
├── tests/
│   └── BankingSystem.Tests/            # xUnit юніт і інтеграційні тести
├── docs/
│   ├── vision.md                       # Постановка задачі
│   ├── backlog.md                      # Пріоритети по ітераціях
│   ├── class-diagram.md                # UML (Mermaid)
│   ├── sequence-diagram.md             # Sequence diagram UC1
│   └── iteration-1.md                  # Звіт по ітерації 1
├── .github/workflows/dotnet.yml        # GitHub Actions CI
└── BankingSystem.sln
```

## Архітектура

```
Console ──► Application (AccountService)
                  │
                  ├──► Domain (Account, Customer, Transaction)
                  │         бізнес-правила та інваріанти
                  │
                  └──► Infrastructure (IRepository<T>)
                              in-memory (Lab 34) → JSON (Lab 35)
```

**Ключові принципи:**
- DIP: `AccountService` залежить від `IAccountRepository`, не від конкретного класу
- OCP: нові типи рахунків — просто новий підклас `Account`, без зміни сервісу
- SRP: кожен сервіс має одну зону відповідальності

## Реалізовані сценарії (Ітерація 1)

- Реєстрація клієнта
- Відкриття рахунку (CheckingAccount, SavingsAccount)
- Поповнення / зняття
- **Переказ між рахунками** (головний vertical slice)
- Перегляд виписки

## Майбутні ітерації

| Lab | Що додається |
|-----|-------------|
| Lab 35 | JSON-персистентність, нарахування відсотків (Strategy), кредити |
| Lab 36 | Coverage, інтеграційні тести, fault handling, CI quality gate |
| Lab 37 | Release-документація, DEMO, фінальний звіт |
