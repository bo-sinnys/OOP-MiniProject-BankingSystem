using BankingSystem.Application.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Exceptions;
using BankingSystem.Infrastructure.Repositories;

// Composition root — єдине місце де збираємо залежності
var customerRepo = new InMemoryCustomerRepository();
var accountRepo  = new InMemoryAccountRepository();
var service      = new AccountService(accountRepo, customerRepo);

// Seed demo-даних щоб одразу можна було тестувати
var alice = service.RegisterCustomer("Аліса Коваленко", "alice@bank.ua");
var bob   = service.RegisterCustomer("Боб Шевченко",   "bob@bank.ua");

var aliceChecking = service.OpenAccount(alice.Id, AccountType.Checking);
var bobSavings    = service.OpenAccount(bob.Id,   AccountType.Savings);

service.Deposit(aliceChecking.Id, 5000m);
service.Deposit(bobSavings.Id, 2000m);

Console.OutputEncoding = System.Text.Encoding.UTF8;
RunMenu();

void RunMenu()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("════════════════════════════════");
        Console.WriteLine("  BankingSystem — головне меню  ");
        Console.WriteLine("════════════════════════════════");
        Console.WriteLine("  1. Показати всі рахунки");
        Console.WriteLine("  2. Поповнити рахунок");
        Console.WriteLine("  3. Зняти кошти");
        Console.WriteLine("  4. Переказ між рахунками");
        Console.WriteLine("  5. Виписка по рахунку");
        Console.WriteLine("  6. Зареєструвати клієнта");
        Console.WriteLine("  7. Відкрити рахунок");
        Console.WriteLine("  0. Вихід");
        Console.WriteLine("────────────────────────────────");
        Console.Write("  Оберіть пункт: ");

        var choice = Console.ReadLine()?.Trim();
        Console.WriteLine();

        try
        {
            switch (choice)
            {
                case "1": ShowAllAccounts();   break;
                case "2": DoDeposit();         break;
                case "3": DoWithdraw();        break;
                case "4": DoTransfer();        break;
                case "5": ShowStatement();     break;
                case "6": RegisterCustomer();  break;
                case "7": OpenAccount();       break;
                case "0":
                    Console.WriteLine("До побачення!");
                    return;
                default:
                    PrintWarning("Невідомий пункт меню. Спробуйте ще раз.");
                    break;
            }
        }
        catch (DomainException ex)
        {
            PrintError($"Помилка: {ex.Message}");
        }
        catch (Exception ex)
        {
            PrintError($"Системна помилка: {ex.Message}");
        }
    }
}

// ── Handlers ──────────────────────────────────────────────────────────────────

void ShowAllAccounts()
{
    var accounts = accountRepo.GetAll();
    if (!accounts.Any())
    {
        Console.WriteLine("Рахунків немає.");
        return;
    }

    Console.WriteLine($"{"ID (перші 8)",-12} {"Тип",-10} {"Клієнт",-22} {"Баланс",12}");
    Console.WriteLine(new string('─', 60));

    foreach (var acc in accounts)
    {
        var customer = customerRepo.GetById(acc.CustomerId);
        var shortId  = acc.Id.ToString()[..8];
        Console.WriteLine($"{shortId,-12} {acc.Type,-10} {customer?.FullName ?? "?",-22} {acc.Balance,12:C}");
    }
}

void DoDeposit()
{
    var id     = ReadAccountId("ID рахунку для поповнення");
    var amount = ReadDecimal("Сума поповнення");
    service.Deposit(id, amount);
    PrintSuccess($"Рахунок поповнено. Новий баланс: {service.GetAccount(id).Balance:C}");
}

void DoWithdraw()
{
    var id     = ReadAccountId("ID рахунку для зняття");
    var amount = ReadDecimal("Сума зняття");
    service.Withdraw(id, amount);
    PrintSuccess($"Кошти знято. Новий баланс: {service.GetAccount(id).Balance:C}");
}

void DoTransfer()
{
    Console.WriteLine("── Переказ між рахунками ──");
    var fromId = ReadAccountId("ID рахунку-відправника");
    var toId   = ReadAccountId("ID рахунку-отримувача");
    var amount = ReadDecimal("Сума переказу");

    service.Transfer(fromId, toId, amount);

    var from = service.GetAccount(fromId);
    var to   = service.GetAccount(toId);
    PrintSuccess($"Переказ виконано успішно.");
    Console.WriteLine($"  Відправник: {from.Balance:C}");
    Console.WriteLine($"  Отримувач:  {to.Balance:C}");
}

void ShowStatement()
{
    var id = ReadAccountId("ID рахунку");
    var transactions = service.GetStatement(id);

    if (!transactions.Any())
    {
        Console.WriteLine("Транзакцій немає.");
        return;
    }

    Console.WriteLine($"{"Дата",-20} {"Тип",-14} {"Сума",12}  Опис");
    Console.WriteLine(new string('─', 70));

    foreach (var t in transactions)
    {
        Console.WriteLine($"{t.OccurredAt:yyyy-MM-dd HH:mm,-20} {t.Type,-14} {t.Amount,12:C}  {t.Description}");
    }

    var total = transactions.Sum(t =>
        t.Type is TransactionType.Deposit or TransactionType.Interest ? t.Amount : -t.Amount);
    Console.WriteLine(new string('─', 70));
    Console.WriteLine($"{"Підсумок:",50} {total,12:C}");
}

void RegisterCustomer()
{
    Console.Write("  Ім'я та прізвище: ");
    var name = Console.ReadLine()?.Trim() ?? string.Empty;
    Console.Write("  Email: ");
    var email = Console.ReadLine()?.Trim() ?? string.Empty;

    var customer = service.RegisterCustomer(name, email);
    PrintSuccess($"Клієнта зареєстровано. ID: {customer.Id}");
}

void OpenAccount()
{
    var customerId = ReadGuid("ID клієнта");
    Console.Write("  Тип рахунку (1 = Поточний, 2 = Ощадний): ");
    var typeInput = Console.ReadLine()?.Trim();

    var type = typeInput switch
    {
        "1" => AccountType.Checking,
        "2" => AccountType.Savings,
        _   => throw new ArgumentException("Оберіть 1 або 2.")
    };

    var account = service.OpenAccount(customerId, type);
    PrintSuccess($"Рахунок відкрито. ID: {account.Id}");
}

// ── Helpers ───────────────────────────────────────────────────────────────────

Guid ReadAccountId(string prompt)
{
    Console.Write($"  {prompt}: ");
    var input = Console.ReadLine()?.Trim() ?? string.Empty;

    // Дозволяємо вводити перші 8 символів (короткий ID)
    var accounts = accountRepo.GetAll();
    var match = accounts.FirstOrDefault(a => a.Id.ToString().StartsWith(input, StringComparison.OrdinalIgnoreCase));
    if (match != null) return match.Id;

    return ReadGuidDirect(input);
}

Guid ReadGuid(string prompt)
{
    Console.Write($"  {prompt}: ");
    return ReadGuidDirect(Console.ReadLine()?.Trim() ?? string.Empty);
}

Guid ReadGuidDirect(string input)
{
    if (!Guid.TryParse(input, out var id))
        throw new ArgumentException($"Некоректний ID: '{input}'");
    return id;
}

decimal ReadDecimal(string prompt)
{
    Console.Write($"  {prompt}: ");
    var input = Console.ReadLine()?.Trim() ?? "0";
    if (!decimal.TryParse(input, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var value))
        throw new ArgumentException($"Некоректне число: '{input}'");
    return value;
}

void PrintSuccess(string msg)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  ✓ {msg}");
    Console.ResetColor();
}

void PrintError(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  ✗ {msg}");
    Console.ResetColor();
}

void PrintWarning(string msg)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  ! {msg}");
    Console.ResetColor();
}
