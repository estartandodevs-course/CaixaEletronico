using CaixaEletronico.Repositories.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaixaEletronico.Repositories
{
    internal static class RepositoryFactory
    {
        // caminho do arquivo de banco de dados sqlite
        private static readonly string FilePath = Path.Combine(
            AppContext.BaseDirectory,
            @"..\..\..\Repositories\db.sqlite3"
        );

        private static readonly string SqliteSource = $"Data Source={FilePath};Version=3;";

        public static IAccountRepository CreateAccountRepository()
        {
            return new AccountRepositorySqlite(SqliteSource);
        }

        public static ITransactionRepository CreateTransactionRepository(IAccountRepository accountRepository)
        {
            return new TransactionRepositorySqlite(SqliteSource, accountRepository);
        }
    }
}
