using CaixaEletronico.Entities;
using CaixaEletronico.Repositories;


class Program
{
    private static readonly IAccountRepository _accountRepository = RepositoryFactory.CreateAccountRepository();
    private static readonly ITransactionRepository _transactionRepository = RepositoryFactory.CreateTransactionRepository(_accountRepository);
    private static List<Account> _accounts = new List<Account>();
    private static Account? _selectedAccount;
    private static string? _errorMessage = null;
    private static string? _sucessMessage = null;

    static void Main(string[] args)
    {

        while (true)
        {
            
            if (_selectedAccount != null)
            {
                ShowLoggedMenu();
            } else
            {
                ShowLoggedOutMenu();
            }

        }
       
    }

    public static Account SaveAccount(Account account)
    {
        // persiste a conta, retornando o numero unico gerado
        long accountNumber = _accountRepository.Save(account);

        // instancia uma nova conta, passando agora o numero de conta
        Account newAccount = new(accountNumber, account.HolderName, account.Balance, []);

        // adiciona a conta na lista de conta salvas
        _accounts.Add(newAccount);

        // retorna a conta salva
        return newAccount;
    }

    public static Transaction SaveTransaction(Transaction transaction)
    {
        _transactionRepository.Save(transaction);
        return transaction;
    }

    public static int ShowMenu(List<String> options)
    {
        int activeOptionIndex = 0; // índice da opção selecionada, é o que será retornado
        bool confirmed = false;

        ConsoleKey key;

        do
        {
            Console.Clear();

            if (_selectedAccount != null)
            {
                ShowAccountInfoUI();
            }

            Console.WriteLine("Use Up e Down para navegar, Enter para selecionar\n");

            // Mostra opções
            for (int i = 0; i < options.Count; i++)
            {

                if (i == activeOptionIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"> {options[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {options[i]}");
                }
            }

            // Lê a tecla
            key = Console.ReadKey(true).Key;

            switch (key)
            {
                
                case ConsoleKey.UpArrow:
                    activeOptionIndex = (activeOptionIndex + options.Count - 1) % options.Count;
                    break;

                case ConsoleKey.DownArrow:
                    activeOptionIndex = (activeOptionIndex + 1) % options.Count;

                    break;

                case ConsoleKey.Enter:
                    return activeOptionIndex;
            }

        } while (!confirmed);

        return activeOptionIndex;
    }

    public static void ShowLoggedMenu()
    {
        List<String> menuOptions = ["Fazer transação", "Visualizar histórico de transações", "Sair"];

        var selectedOption = ShowMenu(menuOptions);

        switch (selectedOption)
        {

            case 0:
                CreateTransactionUI();
                break;
            case 1:
                ShowTransactionsHistory();
                break;
            case 2:
                _selectedAccount = null;
                break;
        }
    }

    public static void ShowLoggedOutMenu()
    {
        List<String> menuOptions = ["Criar conta", "Entrar em uma conta existente"];
        
        var selectedOption = ShowMenu(menuOptions);
        if (selectedOption == 0)
        {
            CreateAccountUI();
        }
        else
        {
            _selectedAccount = GetAccountUI();
            {

            }
        }
    }

    public static void CreateAccountUI()
    {
        Console.Clear();

        Console.WriteLine("Vamos criar sua conta, mas primeiro precisamo de algumas informações...");

        while (true)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Qual é o seu nome? ");

                Console.Write(": ");
                Console.ResetColor();

                string? holderName = Console.ReadLine();


                if (holderName == null || holderName.Trim() == "")
                    throw new ArgumentException("Nome inválido");
                

                Account account = new Account(holderName);

                account = SaveAccount(account);

                _selectedAccount = account;
                _sucessMessage = "Conta criada com sucesso.";

                break;
            }
            catch (ArgumentException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Valores inválidos, vamos tentar novamente");
                Console.ResetColor();
            } 
            catch (Exception ex) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Houve um erro {ex.GetType} ao criar sua conta, vamos tentar novamente :)");
                Console.ResetColor();
            }
        }
    }

    public static void CreateTransactionUI()
    {

        if (_selectedAccount == null)
        {
            throw new InvalidOperationException("Só é possivél fazer essa operação, estando logado em uma conta");
        }

        List<string> menuOptions = ["Transferir", "Sacar", "Depositar"];

        var selectedOption = ShowMenu(menuOptions);

        switch (selectedOption)
        {

            case 0:
                MakeTransferUI();
                break;
            case 1:
                MakeWithdrawUI();
                break;
            case 2:
                MakeDepositUI();
                break;
            default:
                _errorMessage = $"Transações do tipo {selectedOption + 1} ainda não são suportadas";
                break;
        }
    }

    public static void MakeTransferUI()
    {
        if (_selectedAccount == null)
        {
            throw new InvalidOperationException("Só é possivél fazer essa operação, estando logado em uma conta");
        }

        Account? destinationAccount = null;

        destinationAccount = GetAccountUI();

        if (destinationAccount == null) return;

        Console.Write($"Informe o valor a ser transferido para {destinationAccount.HolderName}\n: ");
        Console.ResetColor();

        decimal amount = Convert.ToDecimal(Console.ReadLine());

        try
        {
            var transaction = _selectedAccount.Transfer(amount, destinationAccount);
            SaveTransaction(transaction);
            _errorMessage = null;
            _sucessMessage = $"Transferência de {transaction.Amount:C} para {destinationAccount.HolderName} em {transaction.DateTime}";
        } 
        catch (Exception ex)
        {
            _errorMessage = $"Falha em Transferência para {destinationAccount.HolderName}. Motivo: {ex.Message}"; ;
            _sucessMessage = null;
        }
    }
    
    public static void MakeDepositUI()
    {
        if (_selectedAccount == null)
        {
            throw new InvalidOperationException("Só é possivél fazer essa operação, estando logado em uma conta");
        }

        Console.Write($"Informe o valor a ser depositado: ");

        decimal amount = Convert.ToDecimal(Console.ReadLine());

        try
        {
            var transaction = _selectedAccount.Deposit(amount);
            SaveTransaction(transaction);
            _errorMessage = null;
            _sucessMessage = $"Depósito de {transaction.Amount:C} feito em {transaction.DateTime}";
        }
        catch (Exception ex)
        {
            _errorMessage = $"Falha em Depósito. Motivo: {ex.Message}"; ;
            _sucessMessage = null;
        }
    }

    public static void MakeWithdrawUI()
    {
        if (_selectedAccount == null)
        {
            throw new InvalidOperationException("Só é possivél fazer essa operação, estando logado em uma conta");
        }

        Console.Write($"Informe o valor a ser sacado: ");

        decimal amount = Convert.ToDecimal(Console.ReadLine());

        try
        {
            var transaction = _selectedAccount.Withdraw(amount);
            SaveTransaction(transaction);
            _errorMessage = null;
            _sucessMessage = $"Saque de {transaction.Amount:C} feito em {transaction.DateTime}";
        }
        catch (Exception ex)
        {
            _errorMessage = $"Falha em Saque. Motivo: {ex.Message}"; ;
            _sucessMessage = null;
        }
    }

    public static void ShowTransactionsHistory()
    {
        if (_selectedAccount == null)
        {
            throw new InvalidOperationException("Só é possivél fazer essa operação, estando logado em uma conta");
        }

        Console.Clear();
        _errorMessage = null;
        _sucessMessage = null;

        ShowAccountInfoUI();

        // insere as transações da conta encontrada
        var transactions = _transactionRepository.GetAll(_selectedAccount);

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Transações ({transactions.Count}): \n");
        Console.ResetColor();


        foreach (var transaction in transactions)
        {
            if (transaction.Type == TransactionType.Transfer)
            {
                if (transaction.IsDestination(_selectedAccount)) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Trasferência recebida de {transaction.SourceAccount.HolderName} no valor de {transaction.Amount:C} em {transaction.DateTime}");
                } else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Trasferência enviada para {transaction.DestinationAccountHolderName} no valor de {transaction.Amount:C} em {transaction.DateTime}");
                }
            }

            if (transaction.Type == TransactionType.Deposit)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Depósito efetuado no valor de {transaction.Amount:C} em {transaction.DateTime}");
            }
                

            if (transaction.Type == TransactionType.Withdraw)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Saque efetuado no valor de {transaction.Amount:C} em {transaction.DateTime}");
            } 

            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\nPressione qualquer tecla para voltar...");
        Console.ReadKey(true);

        Console.ResetColor();

    }
    public static Account? GetAccountUI()
    {
        while (true)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Qual é o número da conta? ");

                Console.Write(": ");
                Console.ResetColor();

                string numberStr = Console.ReadLine() ?? throw new ArgumentException("Valor inválido");
                long number = long.Parse(numberStr); ;

                Account? foundedAccount = _accountRepository.Get(number);

                if (foundedAccount != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Conta encontrada, nome do dono é " + foundedAccount.HolderName);
                    Console.ResetColor();

                    return foundedAccount;
                } else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Não existe conta com esse número!");
                    Console.ResetColor();

                    Console.Write("Digite 1 para continuar a procura ou 0 para cancelar: ");
                    int continueSearch = int.Parse(Console.ReadLine());

                    if (continueSearch == 0)
                    {
                        return null;
                    } 
                }

            }
            catch (ArgumentException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Número de conta inválido, insira um válido !");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Houve um erro, vamos tentar novamente :)");
                Console.ResetColor();
            }
        }
    }

    public static void ShowAccountInfoUI()
    {
        if (_selectedAccount == null)
        {
            Console.WriteLine("Nenhuma conta selecionada");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Olá, {_selectedAccount.HolderName}");
        Console.WriteLine($"Número: {_selectedAccount.Number}");
        Console.WriteLine($"Saldo: {_selectedAccount.Balance:C}\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;


        if (_errorMessage != null || _sucessMessage != null)
            Console.WriteLine("Mensagens: \n");


        if (_errorMessage != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"* {_errorMessage} \n\n");
            Console.ResetColor();
        }

        if (_sucessMessage != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"* {_sucessMessage} \n\n");
            Console.ResetColor();
        }

        Console.ResetColor();
    }
}
