# GamePlatform.Jogos

## 📋 Sobre o Projeto
**GamePlatform.Jogos** é uma API RESTful desenvolvida em **.NET 8.0**, responsável pelo gerenciamento e catálogo de jogos da plataforma GamePlatform.  
Essa API faz parte da nova arquitetura baseada em **microsserviços**, sendo responsável pelas operações de CRUD de jogos, compra de jogos e integração com o **Elasticsearch** para buscas e métricas.

---

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture** e está estruturado em camadas:

- **GamePlatform.Jogos.Api**: Camada de apresentação que expõe os endpoints REST.
- **GamePlatform.Jogos.Application**: Contém a lógica de aplicação e casos de uso (cadastro, atualização, compra e sincronização com Elasticsearch).
- **GamePlatform.Jogos.Domain**: Define as entidades de domínio e regras de negócio.
- **GamePlatform.Jogos.Infrastructure**: Implementa o acesso a dados (PostgreSQL), mensageria (Azure Service Bus) e integração com Elasticsearch.
- **GamePlatform.Jogos.Tests**: Projeto de testes unitários.

---

## 🚀 Como Executar

### Pré-requisitos
- .NET SDK 8.0 ou superior  
- Banco de dados PostgreSQL  
- Azure Service Bus configurado  
- Instância do Elasticsearch acessível  
- IDE compatível (Visual Studio, JetBrains Rider ou VS Code)

### Passos para Execução

1. Clone o repositório:
```bash
git clone https://github.com/gameplatform-team/GamePlatform.Jogos.git
```

2. Navegue até a pasta do projeto:
```bash
cd GamePlatform.Jogos
``` 

3. Restaure as dependências:
```bash
dotnet restore
``` 

4. Execute a aplicação:
```bash
cd GamePlatform.Jogos.Api
``` 
```bash
dotnet run
``` 

A API estará disponível em `http://localhost:8081`.

Você pode executar as requisições através do Swagger: `http://localhost:8081/swagger/index.html`.

## 🧩 Principais Funcionalidades
- CRUD de jogos (restrito a administradores nas operações de escrita)
- Compra de jogos com publicação de eventos em filas do Azure Service Bus
- Sincronização de dados com o Elasticsearch
- Incremento de popularidade de jogos após compras confirmadas
- Consulta de catálogo, recomendações e jogos populares via Elasticsearch

## 🧪 Executando os Testes

Para executar os testes unitários:
```bash
dotnet test
```

## 🛠️ Tecnologias Utilizadas

- ASP.NET Core 8.0
- C# 12.0
- PostgreSQL
- Azure Service Bus
- Elasticsearch
- Clean Architecture
- Testes Unitários (xUnit)

## 📦 Estrutura da Solução

```plaintext
GamePlatform.Jogos/
├── GamePlatform.Jogos.Api/            # API endpoints e configurações
├── GamePlatform.Jogos.Application/    # Casos de uso e lógica de aplicação
├── GamePlatform.Jogos.Domain/         # Entidades e regras de negócio
├── GamePlatform.Jogos.Infrastructure/ # Repositórios, mensageria e ES
└── GamePlatform.Jogos.Tests/          # Testes unitários
```

## 🔄 CI/CD

O projeto utiliza GitHub Actions para automação de CI/CD, incluindo:
- Build e testes automatizados
- Build e push de imagem Docker
- Deploy automatizado no Azure Container Apps
