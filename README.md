# 💸 Caixa Eletrônico (ATM Simples)

Este projeto é uma simulação de um **Caixa Eletrônico (ATM)** feito em C#, como parte do curso da [Estartando Devs](https://estartandodevs.com.br/). Ele permite ao usuário realizar operações bancárias básicas via terminal.

---

## 🛠️ Funcionalidades

- [x] Cadastro de conta bancária
- [ ] Autenticação por número da conta e senha
- [x] Ver saldo
- [x] Realizar depósitos
- [x] Realizar saques
- [x] Transferências entre contas
- [x] Histórico de transações

---

## ⚙️ Tecnologias e Padrões Utilizados

- ✅ C# .NET (Console Application)
- ✅ SQLite (banco de dados leve local)
- ✅ Interface-based Repositories (boas práticas)
- ✅ Separação de responsabilidades (SOLID)
- ✅ Injeção via Factory (RepositoryFactory.cs)

---

## 🚀 Como Executar

### 1. Clone o repositório:

```bash
git clone git@github.com:estartandodevs-course/CaixaEletronico.git
cd CaixaEletronico
dotnet build
dotnet run
```

# Exemplos

![Tela com opções iniciais](docs/01.png)
*Figura 1: Tela com opções iniciais*

![Tela de exemplo 2](docs/02.png)
*Figura 2:  Tela com opções de transferências*

![Tela de exemplo 2](docs/03.png)
*Figura 3:  Tela de transfererir*

![Tela de exemplo 2](docs/04.png)
*Figura 4:  Exemplo de mensagem de errro ao tentar transfererir sem saldo*

![Tela de exemplo 2](docs/05.png)
*Figura 5:  Tela com todas as transações, feitas e recebidas*

## 🙋‍♂️ Autor

Desenvolvido por Jader, aluno do curso Estartando Devs.

