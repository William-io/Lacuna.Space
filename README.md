
## Arquitetura do Sistema

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, organizando o código em camadas bem definidas:

```
Source/
├── Lacuna.Space.Api/          # Camada de Apresentação (API Web)
├── Lacuna.Space.Application/  # Camada de Aplicação (Casos de Uso)
├── Lacuna.Space.Domain/       # Camada de Domínio (Regras de Negócio)
└── Lacuna.Space.Infrastructure/ # Camada de Infraestrutura (Serviços Externos)
```

---

## 📁 Estrutura Detalhada do Projeto

### **Lacuna.Space.Api** - Camada de Apresentação

Esta camada expõe os endpoints HTTP e gerencia as requisições externas.

#### Arquivos Principais:

- **`Program.cs`**
  - Ponto de entrada da aplicação
  - Configuração de serviços, middleware e pipeline de requisições
  - Configuração do Swagger para documentação da API

- **`appsettings.json` / `appsettings.Development.json`**
  - Configurações da aplicação (URLs, logging, etc.)
  - Configurações específicas do ambiente de desenvolvimento

- **`Lacuna.Space.Api.http`**
  - Arquivo de teste HTTP para testar endpoints da API
  - Contém exemplos de requisições para desenvolvimento

#### 📂 **Configuration/**
- **`ConfigureSwaggerOptions.cs`**
  - Configuração personalizada do Swagger/OpenAPI
  - Define versioning e documentação da API

#### 📂 **Extensions/**
- **`ServiceCollectionExtensions.cs`**
  - Métodos de extensão para configuração de dependências
  - Registro de serviços no container de IoC

- **`WebApplicationExtensions.cs`**
  - Configuração de endpoints da API
  - Mapeamento de rotas e configuração de middleware

#### 📂 **Models/**
- **`SyncRequest.cs`**
  - DTOs (Data Transfer Objects) para requisições
  - Modelos de entrada e saída da API
  - Contém: `SyncRequest`, `SyncResponse`, `ProbeInfo`

#### 📂 **Properties/**
- **`launchSettings.json`**
  - Configurações de inicialização e profiles de execução
  - URLs, variáveis de ambiente, etc.

---

### **Lacuna.Space.Application** - Camada de Aplicação

Esta camada implementa os casos de uso e orquestra as operações de negócio.

#### 📂 **Orchestrations/**
- **`LumaOrchestrationService.cs`**
  - Serviço principal que coordena todo o processo de sincronização
  - Orquestra: autenticação → busca de sondas → sincronização → processamento de jobs
  - Gerencia o fluxo completo do protocolo Luma

#### 📂 **Syncs/**
- **`ClockSynchronizationService.cs`**
  - Implementa o algoritmo de sincronização de relógio
  - Calcula offset temporal e round-trip delay
  - Algoritmo baseado no Network Time Protocol (NTP)
  - Fórmulas: `offset = ((t1-t0) + (t2-t3)) / 2`

#### 📂 **Jobs/ProcessJob/**
- **`JobProcessingService.cs`**
  - Gerencia o processamento de jobs de verificação
  - Obtém jobs da API Luma e executa verificações de sincronização
  - Implementa o ciclo: take job → check job → repeat até "Done"

---

### **Lacuna.Space.Domain** - Camada de Domínio

Esta camada contém as regras de negócio puras, entidades e interfaces.

#### 📂 **Abstractions/**
- **`Entity.cs`**
  - Classe base para entidades do domínio
  - Fornece identidade única e comportamentos comuns

- **`ILumaApiService.cs`**
  - Interface que define o contrato para comunicação com a API Luma
  - Métodos: StartSession, GetProbes, SyncProbe, TakeJob, CheckJob

#### 📂 **Enums/**
- **`JobStatus.cs`**
  - Estados possíveis de um job de sincronização
  - Valores: Pending, Completed, Failed

- **`TimestampEncoding.cs`**
  - Tipos de codificação de timestamp suportados pelas sondas
  - Valores: Iso8601, Ticks, TicksBinary, TicksBinaryBigEndian

#### 📂 **Jobs/**
- **`Job.cs`**
  - Entidade que representa um job de verificação de relógio
  - Contém ID do job e nome da sonda a ser verificada

#### 📂 **Probes/**
- **`Probe.cs`**
  - Entidade principal do domínio - representa uma sonda espacial
  - Propriedades: Id, Name, Encoding, TimeOffset, RoundTrip, IsSynchronized
  - Métodos de sincronização e codificação/decodificação de timestamps
  - Implementa diferentes encodings de timestamp para cada sonda

#### 📂 **Examples/**
- Exemplos e casos de teste do domínio

#### 📂 **Utilities/**
- Utilitários e helpers do domínio

---

### **Lacuna.Space.Infrastructure** - Camada de Infraestrutura

Esta camada implementa as interfaces do domínio e gerencia comunicação externa.

#### 📂 **Authentication/**
- **`TokenManager.cs`**
  - Gerencia tokens de acesso da API Luma
  - Controla expiração (2 minutos) e renovação automática
  - Implementa cache de token em memória

#### 📂 **Clients/**
- **`IApiClient.cs`**
  - Interface para cliente HTTP genérico
  - Define métodos para requisições GET/POST

- **`ApiClient.cs`**
  - Implementação do cliente HTTP para comunicação com APIs externas
  - Gerencia headers, serialização JSON e tratamento de erros
  - Configuração de timeout e retry policies

#### 📂 **Services/**
- **`LumaApiService.cs`**
  - Implementação concreta da interface `ILumaApiService`
  - Comunica-se com a API Luma (https://luma.lacuna.cc/)
  - Implementa todos os endpoints: start, probe, sync, job/take, job/check
  - Gerencia autenticação e tratamento de erros

#### 📂 **Setting/**
- Configurações específicas da infraestrutura

#### 📂 **Validation/**
- **`IApiResponse.cs`**
  - Interfaces e records para respostas da API Luma
  - Define: `StartResponse`, `ProbesResponse`, `SyncResponse`, `JobResponse`, `CheckJobResponse`
  - Implementa validação de respostas da API externa

- **`IResponseValidator.cs`**
  - Interface para validação de respostas
  - Implementa validação de códigos de resposta ('Success', 'Error', 'Unauthorized')

---

## Tecnologias Utilizadas

- **.NET 8.0** - Framework principal
- **ASP.NET Core** - API Web
- **Swagger/OpenAPI** - Documentação da API
- **HttpClient** - Comunicação HTTP
- **Dependency Injection** - Inversão de controle
- **Logging** - Microsoft.Extensions.Logging

---

## Como Executar

### Pré-requisitos:
- .NET 8.0 SDK
- Visual Studio 2022 ou VS Code

### Execução:
```bash
cd Source/Lacuna.Space.Api
dotnet run
```

### Endpoints:
- **API**: http://localhost:5008
- **Swagger**: http://localhost:5008/swagger

### Teste:
```bash
POST http://localhost:5008/sync
Content-Type: application/json

{
  "username": "seu_usuario",
  "email": "seu_email@exemplo.com"
}
```

---

## Estrutura de Arquivos

```
Lacuna.Space/
├── README.md                           # Este arquivo
├── Lacuna.Space.sln                    # Solution do projeto
├── Lacuna.Space.sln.DotSettings.user   # Configurações do usuário
│
└── Source/
    ├── Lacuna.Space.Api/               # Camada de Apresentação
    │   ├── Program.cs                  # Entry point
    │   ├── appsettings.json            # Configurações
    │   ├── Configuration/              # Configuração Swagger
    │   ├── Extensions/                 # Extensions DI/Routing
    │   ├── Models/                     # DTOs da API
    │   └── Properties/                 # Launch settings

    ├── Lacuna.Space.Application/       # Camada de Aplicação
    │   ├── Orchestrations/             # Orquestração principal
    │   ├── Syncs/                      # incronização de relógio
    │   └── Jobs/ProcessJob/            # Processamento de jobs

    ├── Lacuna.Space.Domain/            # Camada de Domínio
    │   ├── Abstractions/               # Interfaces e entidades base
    │   ├── Enums/                      # Enumerações
    │   ├── Jobs/                       # Entidade Job
    │   ├── Probes/                     # Entidade Probe (principal)
    │   ├── Examples/                   # Exemplos
    │   └── Utilities/                  # Utilitários

    └── Lacuna.Space.Infrastructure/    # Camada de Infraestrutura
        ├── Authentication/             # Gerenciamento de tokens
        ├── Clients/                    # Cliente HTTP
        ├── Services/                   # Implementação APIs
        ├── Setting/                    # Configurações
        └── Validation/                 # Validação de respostas
```