README.md
# CloudGames.Users

Microserviço responsável pelo **gerenciamento de usuários** da plataforma **CloudGames**.

Este serviço faz parte de uma arquitetura de **microsserviços orientada a eventos**, utilizando **RabbitMQ**, **MassTransit** e **Clean Architecture**.

---

# Arquitetura

O projeto segue os princípios de **Clean Architecture** e **DDD (Domain Driven Design)**.

Estrutura das camadas:


CloudGames.Users
│
├── CloudGames.Users.API
│ Camada de entrada da aplicação (Controllers, Auth, Middlewares)

├── CloudGames.Users.Application
│ Regras de aplicação (UseCases, Interfaces, DTOs)

├── CloudGames.Users.Domain
│ Entidades e regras de negócio

├── CloudGames.Users.Infrastructure
│ Persistência, mensageria e integrações externas

└── CloudGames.Users.Tests
Testes unitários


---

# Tecnologias Utilizadas

- .NET
- ASP.NET Core
- Entity Framework Core
- SQLite (desenvolvimento)
- MassTransit
- RabbitMQ
- JWT Authentication
- NLog
- xUnit
- Moq

---

# Funcionalidades

Este microserviço é responsável por:

- Criar usuários
- Listar usuários
- Atualizar usuários
- Remover usuários
- Buscar usuário por ID
- Autenticação com JWT
- Publicação de eventos para outros microserviços

---

# Mensageria

O projeto utiliza **RabbitMQ** com **MassTransit** para comunicação entre microsserviços.

Quando eventos importantes acontecem, o serviço publica mensagens no **broker**.

Exemplo:


UserCreatedIntegrationEvent


Esse evento pode ser consumido por outros serviços como:

- Notifications
- Payments
- Analytics

---

# Integration Events

Um **Integration Event** representa algo importante que aconteceu no sistema e precisa ser comunicado a outros microsserviços.

Exemplo:


UserCreatedIntegrationEvent


Fluxo:


Users API
│
▼
UseCase
│
▼
Publica evento
│
▼
RabbitMQ
│
▼
Outros Microserviços


---

# Retry Policy

O sistema utiliza **Retry Policy** para tratar falhas temporárias no processamento de mensagens.

Exemplo de configuração no MassTransit:


UseMessageRetry


Se ocorrer uma falha no processamento da mensagem, o sistema tentará processá-la novamente.

---

# Dead Letter Queue (DLQ)

Caso uma mensagem falhe várias vezes no processamento, ela será enviada para uma fila especial chamada:


Dead Letter Queue


Essa fila armazena mensagens que precisam de análise manual.

---

# Segurança

O projeto utiliza **JWT Authentication**.

Exemplo de autenticação:


Authorization: Bearer {token}


Políticas de autorização podem ser aplicadas como:

- Admin
- Usuário ativo

---

# Logging

O sistema utiliza **NLog** para geração de logs.

Logs incluem:

- Inicialização da aplicação
- Processamento de eventos
- Erros de aplicação
- Informações de execução

Exemplo de log:


Starting CloudGames.Users...
User created successfully
Processing message from queue


---

# Seed Inicial de Usuário (Admin)

Quando a aplicação inicia pela **primeira vez**, o sistema verifica se existem usuários cadastrados no banco de dados.

Caso **nenhum usuário exista**, o sistema cria automaticamente um **usuário administrador padrão**.

Isso garante que o sistema tenha acesso inicial para gerenciamento.

### Usuário criado automaticamente


Email: admin@cloudgames.com

Password: Admin@123
Role: Admin
Status: Active


### Fluxo de inicialização


Application Start
│
▼
Verifica usuários existentes
│
├── Existe usuário → continua execução
│
└── Não existe usuário
│
▼
Cria usuário Admin padrão


### Observação de segurança

Em ambiente de **produção**, recomenda-se:

- Alterar a senha do administrador após o primeiro login
- Ou remover a criação automática

---

# Executar o projeto

### Clonar o repositório


git clone <repositorio>


---

### Restaurar pacotes


dotnet restore


---

### Rodar a aplicação


dotnet run --project CloudGames.Users.API


---

# Executar RabbitMQ com Docker

Caso esteja utilizando RabbitMQ localmente:


docker run -d
--hostname rabbit
--name rabbitmq
-p 5672:5672
-p 15672:15672
rabbitmq:3-management


Painel de administração:


http://localhost:15672


Login padrão:


user: guest
password: guest


---

# Executar Testes


dotnet test


Os testes utilizam:

- xUnit
- Moq

Eles validam:

- UseCases
- Consumers
- Regras de negócio

---

# Fluxo geral do sistema


Client
│
▼
Users API
│
▼
Application Layer (UseCases)
│
▼
Domain
│
▼
Infrastructure
│
▼
Database
│
▼
Publish Integration Event
│
▼
RabbitMQ
│
▼
Outros Microserviços


---

# Boas práticas aplicadas

- Clean Architecture
- SOLID
- Domain Driven Design (DDD)
- Microsserviços
- Mensageria orientada a eventos
- Retry Pattern
- Dead Letter Queue
- Logging estruturado
- Testes automatizados

---

# Autor

**Leandro Gomes Oliveira**

Projeto desenvolvido para estudo de **Arquitetura de Microsserviços com .NET**.


FIAP - Arquitetura de Software
CloudGames Microservices