# ClinicScheduler

Sistema web de agendamento para clínicas, com verificação de disponibilidade, bloqueio de finais de semana/feriados e criação de agendamentos via API REST.

## Visão Geral

O **ClinicScheduler** permite selecionar uma data, listar horários disponíveis e confirmar um agendamento.  
As regras de negócio são validadas no backend e incluem:

- Horários de atendimento (08:00–18:00)
- Duração fixa de consulta (1h)
- Bloqueio de finais de semana
- Bloqueio de feriados nacionais (Brasil) via **Nager.Date**

## Funcionalidades

- Verificação de disponibilidade (**GET `/available?date=YYYY-MM-DD`**)
- Criação de agendamento (**POST `/appointments`**)
- Listagem de agendamentos (**GET `/appointments`**)
- Validação de conflitos (impede agendamentos no mesmo horário de início)
- Interface web responsiva (arquivos estáticos em `wwwroot/`)

## Regras de Negócio

- Dias úteis: **segunda a sexta-feira**
- Horário de atendimento: **08:00 às 18:00** (**último início às 17:00**)
- Duração das consultas: **1 hora** (início/fim devem diferir exatamente 1h)
- Não permite agendar em finais de semana, feriados nacionais ou slots ocupados
- Período permitido para agendamento: **2026–2035**

## Stack

- Backend: **ASP.NET Core** + **Entity Framework Core**
- Banco: **SQL Server** (LocalDB/Express/Full)
- Documentação: **Swagger/OpenAPI**
- Feriados: **Nager.Date** (Public Holiday API)

> **Nota:** Este projeto utiliza **.NET 10.0**. Para desenvolvimento, recomenda-se Visual Studio 2026 (v18+) ou VS Code + dotnet CLI.

## Estrutura do Projeto

```text
ClinicScheduler/
└── Backend/
    ├── Controllers/
    ├── Domain/
    ├── DTOs/
    ├── Infrastructure/
    ├── Servicos/
    ├── wwwroot/
    └── Program.cs
```

## Como Rodar (Local)

### Pré-requisitos

- .NET SDK (conforme `TargetFramework` do projeto)
- SQL Server (LocalDB, Express ou completo)

### Configuração

Edite `Backend/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "ConexaoPadrao": "Server=SuaConexão;Database=ClinicaAgendamentosDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Migrações e Execução

```bash
cd Backend
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef database update
dotnet run
```

### URLs (por padrão)

- Web: `https://localhost:7032` / `http://localhost:5000`
- Swagger: `https://localhost:7032/swagger` / `http://localhost:5090/swagger`

> As portas podem variar dependendo do seu ambiente/perfil de execução.

## API

### Verificar Disponibilidade

```http
GET /available?date=2026-01-15
```

**Resposta de sucesso:**
```json
{
  "slots": [
    {
      "inicio": "2026-01-15T08:00:00",
      "fim": "2026-01-15T09:00:00"
    },
    {
      "inicio": "2026-01-15T09:00:00",
      "fim": "2026-01-15T10:00:00"
    }
  ],
  "motivo": null,
  "feriadoNome": null
}
```

**Resposta para feriado:**
```json
{
  "slots": [],
  "motivo": "FERIADO",
  "feriadoNome": "Confraternização Universal"
}
```

### Criar Agendamento

```http
POST /appointments
Content-Type: application/json

{
  "nomePaciente": "Maria Silva",
  "emailPaciente": "maria@email.com",
  "inicio": "2026-01-15T08:00:00",
  "fim": "2026-01-15T09:00:00"
}
```

**Erros comuns (exemplos):**
- `400 Bad Request`: horário inválido, fora do período permitido, final de semana/feriado ou conflito de agenda.

### Listar Agendamentos

```http
GET /appointments
```

## Decisões Técnicas

- **Duração fixa (1h):** simplifica validações e geração de slots
- **Validação no backend:** garante integridade independente do cliente
- **Detecção de conflitos:** valida antes de persistir para evitar sobreposição
- **Reutilização de HttpClient:** melhora performance/estabilidade nas chamadas à API de feriados (cache por ano fica como melhoria futura)

## Conformidade com o Desafio

-  Consome API pública de feriados (Brasil) no backend
-  Bloqueia finais de semana e feriados
-  Implementa endpoints mínimos (**GET `/available`**, **POST `/appointments`**, **GET `/appointments`**)
-  Frontend web consome o backend para exibir slots e confirmar agendamento

## Roadmap

- Testes automatizados (unit/integration)
- Cache de feriados por ano
- Autenticação e painel administrativo
