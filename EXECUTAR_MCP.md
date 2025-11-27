# ⚡ Execução Rápida do Servidor MCP

## 🚀 Comandos Rápidos

### 1. Restaurar dependências (primeira vez ou após mudanças)
```powershell
dotnet restore
```

### 2. Executar o servidor MCP
```powershell
dotnet run --project src/McpServer/McpServer.csproj
```

## ⚙️ Configuração Necessária

Antes de executar, certifique-se de:

1. ✅ Ter o arquivo `google-credentials.json` em `src/McpServer/`
2. ✅ Configurar o `CalendarId` no arquivo `src/McpServer/appsettings.json`

### Exemplo de appsettings.json:
```json
{
  "GoogleCalendar": {
    "CredentialsFile": "google-credentials.json",
    "CalendarId": "seu-email@gmail.com"
  }
}
```

## 📋 Checklist de Execução

- [ ] .NET 8.0 SDK instalado (`dotnet --version`)
- [ ] Dependências restauradas (`dotnet restore`)
- [ ] Arquivo `google-credentials.json` configurado
- [ ] `appsettings.json` configurado com `CalendarId`
- [ ] Servidor executando (`dotnet run --project src/McpServer/McpServer.csproj`)

## 📚 Documentação Completa

Para instruções detalhadas, consulte: [GUIA_EXECUCAO_MCP.md](GUIA_EXECUCAO_MCP.md)

