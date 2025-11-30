# API de Agendamentos

API REST para gerenciamento de agendamentos usando Clean Architecture e integração com Google Calendar.

## 🏗️ Arquitetura

O projeto segue os princípios da Clean Architecture com as seguintes camadas:

- **Domain**: Entidades e interfaces de domínio
- **Application**: Casos de uso, DTOs, validações e serviços de aplicação
- **Infrastructure**: Implementações de serviços externos (Google Calendar)
- **WebApi**: Controllers, middlewares e configuração da aplicação

## 🔧 Configuração

### Pré-requisitos

1. .NET 8.0 SDK
2. Conta do Google Cloud Platform
3. Credenciais de Service Account do Google Calendar API

### Configuração do Google Calendar

1. **Criar um projeto no Google Cloud Platform**
   - Acesse [Google Cloud Console](https://console.cloud.google.com/)
   - Crie um novo projeto ou selecione um existente

2. **Habilitar Google Calendar API**
   - No menu, vá em "APIs & Services" > "Library"
   - Procure por "Google Calendar API"
   - Clique em "Enable"

3. **Criar Service Account**
   - Vá em "APIs & Services" > "Credentials"
   - Clique em "Create Credentials" > "Service Account"
   - Preencha os dados e crie a conta
   - Após criar, clique na conta e vá em "Keys"
   - Clique em "Add Key" > "Create new key"
   - Selecione "JSON" e baixe o arquivo

4. **Compartilhar o calendário com o Service Account**
   - Abra o Google Calendar
   - Vá em "Configurações" > "Configurações de compartilhamento"
   - Adicione o email do Service Account (formato: `nome@projeto.iam.gserviceaccount.com`)
   - Dê permissão de "Fazer alterações em eventos"

5. **Configurar o arquivo de credenciais**
   - Renomeie o arquivo JSON baixado para `google-credentials.json`
   - Coloque o arquivo na raiz do projeto `src/WebApi/`
   - Ou ajuste o caminho no `appsettings.json`

6. **Configurar appsettings.json**
   ```json
   {
     "GoogleCalendar": {
       "CredentialsFile": "google-credentials.json",
       "CalendarId": "seu-email@gmail.com"
     }
   }
   ```
   
   **Nota**: O `CalendarId` pode ser:
   - O email do calendário principal: `seu-email@gmail.com`
   - O ID de um calendário secundário (encontrado nas configurações do calendário)

## 🚀 Executando a aplicação

1. Restaure as dependências:
   ```bash
   dotnet restore
   ```

2. Execute a aplicação:
   ```bash
   dotnet run --project src/WebApi/WebApi.csproj
   ```

3. Acesse o Swagger:
   - URL: `https://localhost:5001/swagger` (ou a porta configurada)

## 📝 Endpoints

- `POST /api/agendamentos` - Criar novo agendamento
- `GET /api/agendamentos` - Listar todos os agendamentos
- `GET /api/agendamentos/{id}` - Obter agendamento por ID
- `PUT /api/agendamentos/{id}` - Atualizar agendamento
- `DELETE /api/agendamentos/{id}` - Deletar agendamento

## ✅ Validações

O projeto utiliza FluentValidation para validar os dados de entrada:

- Nome do cliente: obrigatório, máximo 200 caracteres
- Telefone: obrigatório, máximo 20 caracteres
- Data/hora de início: obrigatória, deve ser futura
- Data/hora de fim: obrigatória, deve ser futura e posterior à data de início
- Observações: opcional, máximo 1000 caracteres

## 🧪 Testes

Execute os testes com:

```bash
dotnet test
```

## 📦 Tecnologias

- .NET 8.0
- ASP.NET Core
- Google Calendar API v3
- FluentValidation
- AutoMapper
- Serilog
- Swagger/OpenAPI

## 🔒 Segurança

- **Nunca commite o arquivo `google-credentials.json` no repositório**
- Adicione `google-credentials.json` ao `.gitignore`
- Use variáveis de ambiente para produção

## 📚 Estrutura do Projeto

```
ApiAgendamentos/
├── src/
│   ├── Domain/           # Entidades e interfaces de domínio
│   ├── Application/      # Casos de uso, DTOs, validações
│   ├── Infrastructure/   # Implementação do Google Calendar
│   └── WebApi/           # Controllers e configuração
└── tests/                # Testes unitários
```
