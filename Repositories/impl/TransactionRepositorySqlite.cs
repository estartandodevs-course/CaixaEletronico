using CaixaEletronico.Entities;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CaixaEletronico.Repositories.impl
{
    internal class TransactionRepositorySqlite : ITransactionRepository
    {
        private string ConnectionSource { get; }
        protected IAccountRepository AccountRepository { get; }

        public TransactionRepositorySqlite(string connectionSource, IAccountRepository accountRepository) 
        {
            ConnectionSource = connectionSource;
            AccountRepository = accountRepository;

            string createTableCmd = @"
                CREATE TABLE IF NOT EXISTS transactions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    date_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    type VARCHAR(50) NOT NULL,
                    source_account_fk INTEGER NOT NULL,
                    destination_account_fk INTEGER,
                    amount DECIMAL NOT NULL,
                    FOREIGN KEY (source_account_fk) REFERENCES accounts(number),
                    FOREIGN KEY (destination_account_fk) REFERENCES accounts(number)
                );";

            try
            {
                // fecha os recursos antes de sair do escopo
                using var connection = new SQLiteConnection(ConnectionSource);
                connection.Open();

                using (var command = new SQLiteCommand(createTableCmd, connection)) {
                    command.ExecuteNonQuery();
                }

            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error on transaction table initialization. Error message: " + ex.Message);
            }
        }

        public long Save(Transaction transaction)
        {
            if (transaction.Type.Equals(TransactionType.Transfer)) {
                return SaveTransfer(transaction);
            }

            string newTransactionSql = @"INSERT INTO transactions (type, source_account_fk, amount) 
                                        VALUES (@type, @source, @amount)
                                        RETURNING id";

            string incrementBalanceSql = "UPDATE Accounts SET balance = balance + @amount WHERE number = @account_number";
            string decrementBalanceSql = "UPDATE Accounts SET balance = balance - @amount WHERE number = @account_number";

            string updateBalance = incrementBalanceSql;

            if (transaction.Type.Equals(TransactionType.Withdraw))
            {
                updateBalance = decrementBalanceSql;
            }

            using var connection = new SQLiteConnection(ConnectionSource);
            connection.Open();

            using var dbTransaction = connection.BeginTransaction();

            try
            {
                long newTransactionId = -1;
                // Comando para salvar a transação
                using (var command = new SQLiteCommand(newTransactionSql, connection, dbTransaction))
                {

                    command.Parameters.AddWithValue("@type", transaction.Type.ToString());
                    command.Parameters.AddWithValue("@source", transaction.SourceAccount.Number);
                    command.Parameters.AddWithValue("@amount", transaction.Amount);

                    newTransactionId = (long)command.ExecuteScalar();
                }

                if (newTransactionId < 0)
                {
                    throw new SQLiteException("Error on saving transaction");
                }

                // Comando para atualizar o saldo de quem vai receber a transação
                using (var command = new SQLiteCommand(updateBalance, connection, dbTransaction))
                {
                    command.Parameters.AddWithValue("@account_number", transaction.SourceAccount.Number);
                    command.Parameters.AddWithValue("@amount", transaction.Amount);

                    command.ExecuteNonQuery();
                }

                // Efetua as alterações apenas se todos os comando anteriores não lançarem um erro
                dbTransaction.Commit();
                return newTransactionId;
            }
            catch (SQLiteException)
            {
                // desfaz todas as alterações
                dbTransaction.Rollback();
                throw new ArgumentException("Error on saving transaction");
            }
        }

        private long SaveTransfer(Transaction transaction)
        {

            if (transaction.DestinationAccount == null)
            {
                throw new ArgumentException("Destination account cannot be null");
            }

            string newTransactionSql = @"INSERT INTO transactions (type, source_account_fk, destination_account_fk, amount) 
                            VALUES (@type, @source, @destination, @amount)
                            RETURNING id";

            string incrementBalanceSql = "UPDATE Accounts SET balance = balance + @amount WHERE number = @account_number";
            string decrementBalanceSql = "UPDATE Accounts SET balance = balance - @amount WHERE number = @account_number";

            using var connection = new SQLiteConnection(ConnectionSource);
            connection.Open();

            using var dbTransaction = connection.BeginTransaction();

            try
            {
                long newTransactionId = -1;
                // Comando para salvar a transação
                using (var command = new SQLiteCommand(newTransactionSql, connection, dbTransaction))
                {

                    command.Parameters.AddWithValue("@type", transaction.Type.ToString());
                    command.Parameters.AddWithValue("@source", transaction.SourceAccount.Number);
                    command.Parameters.AddWithValue("@destination", transaction.DestinationAccount.Number);
                    command.Parameters.AddWithValue("@amount", transaction.Amount);

                    newTransactionId = (long)command.ExecuteScalar();
                }

                if (newTransactionId < 0)
                {
                    throw new SQLiteException("Error on saving transaction");
                }

                // Comando para aumentar o saldo de quem vai receber a transação
                using (var command = new SQLiteCommand(incrementBalanceSql, connection, dbTransaction))
                {
                    command.Parameters.AddWithValue("@amount", transaction.Amount);
                    command.Parameters.AddWithValue("@account_number", transaction.DestinationAccount.Number);
                    command.ExecuteNonQuery();
                }

                // Comando para diminuir o saldo de quem vai receber a transação
                using (var command = new SQLiteCommand(decrementBalanceSql, connection, dbTransaction))
                {
                    command.Parameters.AddWithValue("@amount", transaction.Amount);
                    command.Parameters.AddWithValue("@account_number", transaction.SourceAccount.Number);
                    command.ExecuteNonQuery();
                }

                // Efetua as alterações apenas se todos os comando anteriores não lançarem um erro
                dbTransaction.Commit();
                return newTransactionId;
            }
            catch (SQLiteException)
            {
                // desfaz todas as alterações
                dbTransaction.Rollback();
                throw new ArgumentException("Error on saving transaction");
            }

        }

        public Transaction? Get(long transactionId)
        {
            using var connection = new SQLiteConnection(ConnectionSource);
            connection.Open();

            using var command = new SQLiteCommand(connection);
            command.CommandText = "SELECT id, type, amount, source_account_fk, destination_account_fk FROM transactions WHERE id = @id";
            command.Parameters.AddWithValue("@id", transactionId);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;
            

            long id = reader.GetInt64(0);
            string typeStr = reader.GetString(1);
            decimal amount = reader.GetDecimal(2);
            long sourceAccountId = reader.GetInt32(3);
            long? destinationAccountId = reader.GetInt32(4);

            // inicializa o tipo da transação como Other
            TransactionType type = TransactionType.Other;
            // caso exista o tipo salvo no enum, pega o valor correspondente no enum, ignorando maiusculas e minusculas
            Enum.TryParse<TransactionType>(typeStr, true, out type);

            Account? sourceAccount = AccountRepository.Get(sourceAccountId);
            Account? destinationAccount = null;

            if (destinationAccountId != null)
                destinationAccount = AccountRepository.Get(sourceAccountId);

            return new Transaction(type, amount, sourceAccount, destinationAccount, id);
            
        }

        public List<Transaction> GetAll(Account account)
        {
            if (account.Number == null)
            {
                throw new Exception("A conta deve possui um número");
            }

            using var connection = new SQLiteConnection(ConnectionSource);
            connection.Open();

            using var command = new SQLiteCommand(connection);
            command.CommandText = @"SELECT id, type, amount, source_account_fk, destination_account_fk, date_time
                                    FROM transactions WHERE source_account_fk = @source_account_fk OR destination_account_fk = @destination_account_fk ";
            command.Parameters.AddWithValue("@source_account_fk", account.Number);
            command.Parameters.AddWithValue("@destination_account_fk", account.Number);

            using var reader = command.ExecuteReader();

            List<Transaction> result = new List<Transaction>();

            var accountsMap = new Dictionary<long, Account>
            {
                [(long)account.Number] = account
            };



            while (reader.Read())
            {
                long id = reader.GetInt64(0);
                string typeStr = reader.GetString(1);
                decimal amount = reader.GetDecimal(2);
                long sourceAccountId = reader.GetInt64(3);
                long? destinationAccountId = reader.IsDBNull(4) ? null : reader.GetInt64(4);
                DateTime dateTime = reader.GetDateTime(5);

                // inicializa o tipo da transação como Other
                TransactionType type = TransactionType.Other;
                // caso exista o tipo salvo no enum, pega o valor correspondente no enum, ignorando maiusculas e minusculas
                Enum.TryParse<TransactionType>(typeStr, true, out type);

                // tenta buscar a instancia de conta que já esta salva no dicionario, se não achar, busca no banco.
                if (!accountsMap.TryGetValue(sourceAccountId, out Account? sourceAccount))
                    sourceAccount = AccountRepository.Get(sourceAccountId);

                if (sourceAccount == null)
                {
                    throw new Exception("Não foi possivél carregar a instancia da conta de origem da transação");
                }

                // tenta buscar a instancia de conta que já esta salva no dicionario, se não achar, busca no banco.
                Account? destinationAccount = null;
                if (destinationAccountId != null && !accountsMap.TryGetValue((int)destinationAccountId, out destinationAccount))
                    destinationAccount = AccountRepository.Get((long)destinationAccountId);

                result.Add(
                    new Transaction(type, amount, sourceAccount, destinationAccount, id, dateTime)
                );
            }

            return result;
        }
    }
}
