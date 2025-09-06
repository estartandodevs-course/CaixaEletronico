using CaixaEletronico.Entities;

namespace CaixaEletronico.Repositories
{
    internal interface IAccountRepository
    {
        long Save(Account account);
        Account? Get(long accountNumber);
        Account GetAll();
    }
}
