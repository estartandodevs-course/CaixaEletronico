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
        CreateTransactionUI();
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

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Conta criada com sucesso. número da conta = " + account.Number);
                Console.ResetColor();

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

        List<string> menuOptions = Enum.GetNames(typeof(TransactionType)).ToList();

        Console.WriteLine("Informe o tipo de transação");

        var selectedOption = ShowMenu(menuOptions);

        bool isValidOption = Enum.TryParse<TransactionType>(menuOptions[selectedOption], out var selectedType);

        if (!isValidOption)
        {
            _errorMessage = "O tipo de transação selecionado não é suportado";
            return;
        }

        if (selectedType == TransactionType.Transfer)
        {
            MakeTransferUI();
            return;
        }

        if (selectedType == TransactionType.Withdraw)
        {
            MakeWithdrawUI();
            return;
        }

        if (selectedType == TransactionType.Deposit)
        {
            MakeDepositUI();
            return;
        }

        _errorMessage = $"Transações do tipo {selectedType} ainda não são suportadas";
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
            _sucessMessage = $"Transferência de {transaction.Amount} para {destinationAccount.HolderName} em {transaction.DateTime}";
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
            _sucessMessage = $"Depósito de {transaction.Amount} feito em {transaction.DateTime}";
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

        Console.Write($"Informe o valor a ser depositado: ");

        decimal amount = Convert.ToDecimal(Console.ReadLine());

        try
        {
            var transaction = _selectedAccount.Withdraw(amount);
            SaveTransaction(transaction);
            _errorMessage = null;
            _sucessMessage = $"Saque de {transaction.Amount} feito em {transaction.DateTime}";
        }
        catch (Exception ex)
        {
            _errorMessage = $"Falha em Saque. Motivo: {ex.Message}"; ;
            _sucessMessage = null;
        }
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
        Console.WriteLine("Mensagens: \n");
        Console.ResetColor();

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
