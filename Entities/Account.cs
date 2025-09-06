using CaixaEletronico.Repositories;
using System.Transactions;

namespace CaixaEletronico.Entities
{
    internal class Account
    {
        public long? Number { get; }
        public string HolderName { get; }
        public decimal Balance { get; private set; }
        private readonly List<Transaction> _transactions = [];

        public Account(string holderName) 
        { 
            this.HolderName = holderName;
            this.Balance = 0;
        }

        public Account(long number, string holderName, decimal balance, List<Transaction> transactions)
        {
            Number = number;
            HolderName = holderName;
            Balance = balance;
            _transactions = transactions;
        }

        public Transaction Withdraw(decimal amount) 
        {

            if (Balance < amount) 
            {
                throw new InvalidOperationException("Saldo insuficiente");
            }

            Transaction transaction = new (TransactionType.Withdraw, amount, this);
            _transactions.Add(transaction);
            Balance -= amount;
            return transaction;
        }

        public Transaction Deposit(decimal amount) 
        {
            Transaction transaction = new(TransactionType.Deposit, amount, this);
            _transactions.Add(transaction);
            Balance += amount;
            return transaction;
        }
        
        public Transaction Transfer(decimal amount, Account destinationAccount) 
        {

            if (Balance < amount) 
            {
                throw new InvalidOperationException("Saldo insuficiente");
            }

            Transaction transaction = new Transaction(TransactionType.Transfer, amount, this, destinationAccount);
            destinationAccount.ReceiveTransfer(transaction);
            Balance -= amount;
            _transactions.Add(transaction);
            return transaction;
        }

        public void ReceiveTransfer(Transaction transaction) 
        {
            _transactions.Add(transaction);
            Balance += transaction.Amount;
        }
    }
}
