# 🚀 Guia de Execução do Servidor MCP

Este guia fornece instruções passo a passo para instalar as dependências e executar o servidor MCP no projeto ApiAgendamentos.

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

1. **.NET 8.0 SDK** ou superior
   - Verifique a instalação: `dotnet --version`
   - Se não tiver, baixe em: https://dotnet.microsoft.com/download/dotnet/8.0

2. **Visual Studio 2022** ou **Visual Studio Code** (opcional, mas recomendado)

## 🔧 Passo 1: Verificar a Instalação do .NET

Abra o PowerShell ou Terminal e execute:

```powershell
dotnet --version
```

Você deve ver algo como: `8.0.xxx` ou superior.

## 📦 Passo 2: Restaurar as Dependências

Navegue até a raiz do projeto e restaure todas as dependências NuGet:

```powershell
cd C:\2025\Projetos\ApiAgendamentos
dotnet restore
```

Este comando irá:
- Baixar todos os pacotes NuGet necessários
- Restaurar as referências entre projetos
- Preparar o ambiente de build

**Tempo estimado:** 1-3 minutos (dependendo da conexão)

## ⚙️ Passo 3: Configurar o Google Calendar

### 3.1. Obter Credenciais do Google Calendar

1. Acesse [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um projeto ou selecione um existente
3. Habilite a **Google Calendar API**:
   - Vá em "APIs & Services" > "Library"
   - Procure por "Google Calendar API"
   - Clique em "Enable"
4. Crie uma **Service Account**:
   - Vá em "APIs & Services" > "Credentials"
   - Clique em "Create Credentials" > "Service Account"
   - Preencha os dados e crie
   - Após criar, clique na conta e vá em "Keys"
   - Clique em "Add Key" > "Create new key"
   - Selecione "JSON" e baixe o arquivo
5. **Compartilhe o calendário** com o Service Account:
   - Abra o Google Calendar
   - Vá em "Configurações" > "Configurações de compartilhamento"
   - Adicione o email do Service Account (formato: `nome@projeto.iam.gserviceaccount.com`)
   - Dê permissão de "Fazer alterações em eventos"

### 3.2. Configurar o Arquivo de Credenciais

1. Renomeie o arquivo JSON baixado para `google-credentials.json`
2. Coloque o arquivo em uma das seguintes localizações:
   - `src/McpServer/google-credentials.json` (recomendado)
   - Ou ajuste o caminho no `appsettings.json`

### 3.3. Configurar o appsettings.json

Edite o arquivo `src/McpServer/appsettings.json` e configure:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "GoogleCalendar": {
    "CredentialsFile": "google-credentials.json",
    "CalendarId": "seu-email@gmail.com"
  }
}
```

**Importante:**
- `CredentialsFile`: Caminho relativo ao executável ou caminho absoluto
- `CalendarId`: Pode ser:
  - O email do calendário principal: `seu-email@gmail.com`
  - O ID de um calendário secundário (encontrado nas configurações do calendário)

## 🏗️ Passo 4: Compilar o Projeto

Compile o projeto para verificar se há erros:

```powershell
dotnet build
```

Se houver erros, corrija-os antes de prosseguir.

## ▶️ Passo 5: Executar o Servidor MCP

### Opção 1: Executar diretamente (Recomendado para desenvolvimento)

```powershell
dotnet run --project src/McpServer/McpServer.csproj
```

### Opção 2: Executar o executável compilado

Primeiro, compile em modo Release:

```powershell
dotnet build --configuration Release --project src/McpServer/McpServer.csproj
```

Depois, execute o executável:

```powershell
.\src\McpServer\bin\Release\net8.0\McpServer.exe
```

### Opção 3: Executar via Visual Studio

1. Abra o arquivo `ApiAgendamentos.sln` no Visual Studio
2. Configure o projeto `McpServer` como projeto de inicialização (botão direito > Set as Startup Project)
3. Pressione F5 ou clique em "Run"

## ✅ Verificação

Quando o servidor iniciar corretamente, você verá:

```
Servidor MCP iniciado. Aguardando requisições via STDIO...
```

O servidor está pronto para receber requisições via STDIO (stdin/stdout) seguindo o protocolo MCP.

## 🔌 Como o Servidor MCP Funciona

O servidor MCP:
- Comunica-se via **STDIO** (entrada/saída padrão)
- Espera requisições JSON no formato do protocolo MCP
- Responde com JSON no formato do protocolo MCP
- Expõe as seguintes ferramentas:
  - `agendar_criar` - Cria um novo agendamento
  - `agendar_consultar` - Consulta agendamentos existentes
  - `agendar_atualizar` - Atualiza um agendamento existente
  - `agendar_deletar` - Deleta um agendamento
  - `agendar_disponibilidade` - Verifica disponibilidade de um horário

## 🧪 Testar o Servidor

Para testar o servidor, você pode usar um cliente MCP ou enviar requisições JSON manualmente via stdin.

### Exemplo de requisição de inicialização:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {}
}
```

### Exemplo de listagem de ferramentas:

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/list",
  "params": {}
}
```

## 🐛 Solução de Problemas

### Erro: "GoogleCalendar:CredentialsFile não configurado"
- Verifique se o arquivo `appsettings.json` existe em `src/McpServer/`
- Verifique se o caminho do `CredentialsFile` está correto

### Erro: "GoogleCalendar:CalendarId não configurado"
- Verifique se o `CalendarId` está configurado no `appsettings.json`
- Certifique-se de que o calendário foi compartilhado com o Service Account

### Erro: "File not found: google-credentials.json"
- Verifique se o arquivo `google-credentials.json` existe
- Se estiver em outro local, ajuste o caminho no `appsettings.json` (use caminho absoluto se necessário)

### Erro: "dotnet: command not found"
- Instale o .NET 8.0 SDK
- Verifique se o PATH está configurado corretamente

### Erro ao restaurar pacotes NuGet
- Verifique sua conexão com a internet
- Tente limpar o cache: `dotnet nuget locals all --clear`
- Execute `dotnet restore` novamente

## 📝 Logs

Os logs são salvos em:
- Console: Saída padrão
- Arquivo: `logs/log-YYYYMMDD.txt` (criado automaticamente)

## 🔒 Segurança

⚠️ **IMPORTANTE:**
- **NUNCA** commite o arquivo `google-credentials.json` no repositório
- Adicione `google-credentials.json` ao `.gitignore`
- Use variáveis de ambiente para produção
- Mantenha suas credenciais seguras

## 📚 Próximos Passos

Após executar o servidor com sucesso, você pode:
1. Integrar com um cliente MCP (ChatGPT, agentes OpenAI, etc.)
2. Testar as ferramentas disponíveis
3. Consultar a documentação completa em `src/Tools/README.md`

## 🆘 Suporte

Se encontrar problemas:
1. Verifique os logs em `logs/log-YYYYMMDD.txt`
2. Verifique se todas as dependências foram instaladas corretamente
3. Verifique se o Google Calendar está configurado corretamente
4. Consulte a documentação do projeto em `README.md`

