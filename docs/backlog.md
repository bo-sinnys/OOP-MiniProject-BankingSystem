# Backlog — BankingSystem

## Ітерація 1 — Lab 34 (baseline)

| ID | Опис | Пріоритет | Статус |
|----|------|-----------|--------|
| B1-01 | Доменна модель: `Customer`, `Account` (abstract), `CheckingAccount`, `SavingsAccount` | Must | ✅ Done |
| B1-02 | `Transaction`, `TransactionType` (enum) | Must | ✅ Done |
| B1-03 | `IRepository<T>` контракт, `InMemoryAccountRepository`, `InMemoryCustomerRepository` | Must | ✅ Done |
| B1-04 | `AccountService` з методами `Transfer`, `Deposit`, `Withdraw` | Must | ✅ Done |
| B1-05 | Перший вертикальний зріз: переказ між рахунками через консольне меню | Must | ✅ Done |
| B1-06 | Кастомні винятки: `InsufficientFundsException`, `AccountNotFoundException` | Must | ✅ Done |
| B1-07 | Мінімум 5 юніт-тестів (домен + сервіс) | Must | ✅ Done |
| B1-08 | GitHub Actions CI (build + test) | Must | ✅ Done |
| B1-09 | `vision.md`, `backlog.md`, class-diagram, sequence-diagram, `iteration-1.md` | Must | ✅ Done |
| B1-10 | `IInterestCalculator` інтерфейс (заготовка для Lab 35) | Should | ✅ Done |

## Ітерація 2 — Lab 35 (бізнес-логіка, persistence, LINQ)

| ID | Опис | Пріоритет |
|----|------|-----------|
| B2-01 | JSON-репозиторії (`JsonAccountRepository`, `JsonCustomerRepository`) | Must |
| B2-02 | UC2: `InterestService` + `SimpleInterestCalculator`, `CompoundInterestCalculator` | Must |
| B2-03 | UC3: `Loan`, `LoanPayment`, `LoanService` з кредитним скорингом | Must |
| B2-04 | UC4: LINQ-запити для виписки (фільтр за датою/типом, агрегація) | Must |
| B2-05 | `AccountFactory` для створення рахунків | Should |
| B2-06 | Оновлені UML-діаграми та `iteration-2.md` | Must |
| B2-07 | Розширений набір тестів (persistence, LINQ, кредитна логіка) | Must |

## Ітерація 3 — Lab 36 (quality gate, тести, fault handling)

| ID | Опис | Пріоритет |
|----|------|-----------|
| B3-01 | `TESTING.md`, `test-strategy.md`, `test-matrix.md` | Must |
| B3-02 | Інтеграційні тести (сервіс + JSON-репозиторій) | Must |
| B3-03 | Негативні сценарії: овердрафт, прострочений кредит, некоректні суми | Must |
| B3-04 | Coverage >= 70% (Coverlet + ReportGenerator) | Must |
| B3-05 | Retry-логіка при читанні/запису файлу | Should |
| B3-06 | CI з quality gate (fail if coverage < threshold) | Must |
| B3-07 | `iteration-3.md` | Must |

## Ітерація 4 — Lab 37 (release, документація, демо)

| ID | Опис | Пріоритет |
|----|------|-----------|
| B4-01 | `USER_GUIDE.md` з покроковими сценаріями | Must |
| B4-02 | `DEVELOPER_GUIDE.md`: архітектура, як додати новий тип рахунку | Must |
| B4-03 | `CHANGELOG.md` по ітераціях | Must |
| B4-04 | `FINAL_REPORT.md` | Must |
| B4-05 | `DEMO.md` з командами для демонстрації | Must |
| B4-06 | `syllabus-coverage.md`: матриця тем курсу | Must |
| B4-07 | `release-plan.md` | Should |
