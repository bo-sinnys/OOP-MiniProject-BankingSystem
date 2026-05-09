# Iteration 1 — Lab 34: звіт і передача в Lab 35

## Що вже працює

- **UC1 — Переказ між рахунками**: повний вертикальний зріз Console → Service → Domain → Repository
- Реєстрація клієнта, відкриття рахунку (Checking/Savings), поповнення, зняття, виписка
- `CheckingAccount`: овердрафт до ліміту + комісія при від'ємному балансі
- `SavingsAccount`: захист мінімального залишку, заготовка `ApplyInterest`
- Кастомні винятки: `InsufficientFundsException`, `AccountNotFoundException`, `CustomerNotFoundException`, `InvalidAmountException`
- 20+ юніт-тести (домен + сервіс), всі зелені
- GitHub Actions CI: restore → build → test

## Артефакти в репозиторії

| Файл | Призначення |
|------|-------------|
| `docs/vision.md` | Постановка задачі, UC, NFR, обмеження |
| `docs/backlog.md` | Пріоритети для ітерацій 1–4 |
| `docs/class-diagram.md` | UML діаграма класів (Mermaid) |
| `docs/sequence-diagram.md` | Sequence diagram для UC1 |
| `docs/iteration-1.md` | Цей файл |
| `.github/workflows/dotnet.yml` | CI конфігурація |
| `README.md` | Опис, запуск, структура |

## Сценарії для розширення в Lab 35

1. **UC2 — Нарахування відсотків**: `InterestService` + два калькулятори (Strategy); виклик `SavingsAccount.ApplyInterest` вже підготовлено.
2. **UC3 — Кредити**: нові класи `Loan`, `LoanPayment`, `LoanService`; скоринг (LINQ по балансах).
3. **JSON-персистентність**: замінити `InMemoryAccountRepository` на `JsonAccountRepository`, реалізувати той самий `IAccountRepository`; код сервісів не зміниться завдяки DIP.

## Ризики та невизначеності

| Ризик | Мітигація |
|-------|-----------|
| Серіалізація поліморфного `Account` (Checking vs Savings) | Discriminated union через `AccountType` enum + custom JSON converter |
| Транзакційність між двома Save() | Для Lab 34 прийнятно; Lab 36 — unit of work або snapshot |
| Зростання консольного меню | Рефакторинг у команди (Command pattern) — Lab 35/36 |

## Класи, свідомо підготовлені під розширення

| Клас / інтерфейс | Як буде розширений |
|------------------|--------------------|
| `abstract Account` | Новий підклас (наприклад `DepositAccount`) — без зміни сервісів |
| `IInterestCalculator` | `SimpleInterestCalculator`, `CompoundInterestCalculator` — Lab 35 |
| `IAccountRepository` | `JsonAccountRepository` — Lab 35 |
| `AccountService.OpenAccount` | Switch-вираз готовий прийняти новий `AccountType` |
