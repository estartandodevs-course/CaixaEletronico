using CaixaEletronico.Entities;

namespace CaixaEletronico.Repositories
{
    internal interface ITransactionRepository
    {
        long Save(Transaction transaction);
        Transaction? Get(long transactionId);
        List<Transaction> GetAll(Account account);
    }
}
