namespace CaixaEletronico.Entities
{
    internal enum TransactionType
    {
        Deposit,
        Withdraw,
        Transfer,
        Other
    }

    internal class Transaction
    {
        public long? id { get; }
        public TransactionType Type { get; }
        public DateTime DateTime { get; }
        public decimal Amount {  get; }
        public Account SourceAccount { get; }
        public Account? DestinationAccount { get; }
        public string DestinationAccountHolderName => DestinationAccount?.HolderName ?? "";

        // construtor, com o destination e o id sendo opcionais
        public Transaction(TransactionType type, decimal amount, Account sourceAccount, Account? destinationAccount = null, long? id = null, DateTime? dateTime = null)
        {
            if (type.Equals(TransactionType.Transfer) && destinationAccount == null)
            {
                throw new ArgumentException("To transfer something, a destination is necessary");
            }

            // Lança uma execeção se o valor da transferencia for negativo ou zero
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

            this.Type = type;
            this.Amount = amount;
            this.SourceAccount = sourceAccount;
            this.DestinationAccount = destinationAccount;

            this.DateTime = DateTime.UtcNow;
            if (dateTime != null)
                this.DateTime = (DateTime)dateTime;

            this.id = id;
        }


        public bool IsDestination(Account account)
        {
            return account.Equals(this.DestinationAccount);
        }

        public override string ToString()
        {
            return $"{DateTime:dd/MM/yyyy HH:mm} - {Type}: {Amount:C}";
        }
    }
}
