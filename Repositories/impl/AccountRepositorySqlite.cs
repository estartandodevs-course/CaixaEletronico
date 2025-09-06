using CaixaEletronico.Entities;
using System.Data.SQLite;

namespace CaixaEletronico.Repositories.impl
{
    internal class AccountRepositorySqlite : IAccountRepository
    {
        private string ConnectionSource { get; }

        public AccountRepositorySqlite(string connectionSource) 
        { 
            this.ConnectionSource = connectionSource;
            string createTableCmd = @"
                CREATE TABLE IF NOT EXISTS accounts (
                    number INTEGER PRIMARY KEY AUTOINCREMENT,
                    holder_name VARCHAR(50) NOT NULL,
                    balance DECIMAL NOT NULL
                );";

            try
            {
                // fecha os recursos antes de sair do escopo
                using var connection = new SQLiteConnection(ConnectionSource);
                connection.Open();

                // fecha os recursos antes de sair do escopo
                using var command = new SQLiteCommand(createTableCmd, connection);
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex) 
            {
                Console.WriteLine("Error on accounts table initialization. Error message: " + ex.Message);
            }
        }

        public long Save(Account account)
        {
            string sql = @"INSERT INTO accounts (holder_name, balance) 
                            VALUES (@name, @balance)
                            RETURNING number";

            
            try
            {
                using var connection = new SQLiteConnection(ConnectionSource);
                connection.Open();

                using var command = new SQLiteCommand(sql, connection);

                command.Parameters.AddWithValue("@name", account.HolderName);
                command.Parameters.AddWithValue("@balance", account.Balance);

                // pega o number gerado da conta inserida 
                long number = (long)command.ExecuteScalar();

                return number;
            }
            catch (SQLiteException)
            {
                throw new ArgumentException("Was not possible to save the new account");
            }
            
        }

        public Account? Get(long accountNumber)
        {
            using var connection = new SQLiteConnection(ConnectionSource);
            connection.Open();

            using var command = new SQLiteCommand(connection);
            command.CommandText = @"
                SELECT number, holder_name, balance 
                FROM accounts 
                WHERE number = @number
            ";

            command.Parameters.AddWithValue("@number", accountNumber);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            long number = reader.GetInt64(0);
            string holderName = reader.GetString(1);
            decimal balance = reader.GetDecimal(2);

            return new Account(number, holderName, balance, []);
        }

        public Account GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
