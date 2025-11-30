# Ferramentas MCP - Agendamentos

Este documento descreve todas as ferramentas MCP disponíveis para gerenciamento de agendamentos.

## 📋 Ferramentas Disponíveis

### 1. `agendar_criar`

Cria um novo agendamento no Google Calendar.

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "clienteNome": {
      "type": "string",
      "description": "Nome completo do cliente (obrigatório, máximo 200 caracteres)"
    },
    "clienteTelefone": {
      "type": "string",
      "description": "Telefone do cliente (obrigatório, máximo 20 caracteres)"
    },
    "dataHoraInicio": {
      "type": "string",
      "format": "date-time",
      "description": "Data e hora de início do agendamento (formato ISO 8601, deve ser futura)"
    },
    "dataHoraFim": {
      "type": "string",
      "format": "date-time",
      "description": "Data e hora de fim do agendamento (formato ISO 8601, deve ser futura e posterior à data de início)"
    },
    "observacoes": {
      "type": "string",
      "description": "Observações adicionais sobre o agendamento (opcional, máximo 1000 caracteres)"
    }
  },
  "required": ["clienteNome", "clienteTelefone", "dataHoraInicio", "dataHoraFim"]
}
```

**Exemplo de Input:**
```json
{
  "clienteNome": "João Silva",
  "clienteTelefone": "11999999999",
  "dataHoraInicio": "2025-02-01T18:00:00Z",
  "dataHoraFim": "2025-02-01T19:00:00Z",
  "observacoes": "Quadra de areia"
}
```

**Exemplo de Output (Sucesso):**
```json
{
  "success": true,
  "errors": [],
  "content": {
    "agendamento": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "clienteNome": "João Silva",
      "clienteTelefone": "11999999999",
      "dataHoraInicio": "2025-02-01T18:00:00.000Z",
      "dataHoraFim": "2025-02-01T19:00:00.000Z",
      "observacoes": "Quadra de areia",
      "dataCriacao": "2025-01-24T10:00:00.000Z",
      "googleCalendarEventId": "event123abc"
    }
  }
}
```

**Exemplo de Output (Erro):**
```json
{
  "success": false,
  "errors": [
    "O nome do cliente é obrigatório.",
    "A data/hora de início deve ser futura."
  ],
  "content": null
}
```

---

### 2. `agendar_consultar`

Consulta agendamentos existentes. Pode filtrar por data ou retornar todos.

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "data": {
      "type": "string",
      "format": "date",
      "description": "Data para consultar agendamentos (formato YYYY-MM-DD, opcional)"
    },
    "dataInicio": {
      "type": "string",
      "format": "date",
      "description": "Data inicial do intervalo de consulta (opcional)"
    },
    "dataFim": {
      "type": "string",
      "format": "date",
      "description": "Data final do intervalo de consulta (opcional)"
    }
  }
}
```

**Exemplo de Input (por data específica):**
```json
{
  "data": "2025-02-01"
}
```

**Exemplo de Input (por intervalo):**
```json
{
  "dataInicio": "2025-02-01",
  "dataFim": "2025-02-07"
}
```

**Exemplo de Input (todos):**
```json
{}
```

**Exemplo de Output:**
```json
{
  "success": true,
  "errors": [],
  "content": {
    "agendamentos": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "clienteNome": "João Silva",
        "clienteTelefone": "11999999999",
        "dataHoraInicio": "2025-02-01T18:00:00.000Z",
        "dataHoraFim": "2025-02-01T19:00:00.000Z",
        "observacoes": "Quadra de areia",
        "dataCriacao": "2025-01-24T10:00:00.000Z",
        "googleCalendarEventId": "event123abc"
      }
    ]
  }
}
```

---

### 3. `agendar_atualizar`

Atualiza um agendamento existente.

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid",
      "description": "ID do agendamento (UUID)"
    },
    "eventId": {
      "type": "string",
      "description": "ID do evento no Google Calendar (alternativa ao id)"
    },
    "clienteNome": {
      "type": "string",
      "description": "Novo nome do cliente (obrigatório)"
    },
    "clienteTelefone": {
      "type": "string",
      "description": "Novo telefone do cliente (obrigatório)"
    },
    "dataHoraInicio": {
      "type": "string",
      "format": "date-time",
      "description": "Nova data e hora de início (obrigatório)"
    },
    "dataHoraFim": {
      "type": "string",
      "format": "date-time",
      "description": "Nova data e hora de fim (obrigatório)"
    },
    "observacoes": {
      "type": "string",
      "description": "Novas observações (opcional)"
    }
  },
  "required": ["clienteNome", "clienteTelefone", "dataHoraInicio", "dataHoraFim"]
}
```

**Exemplo de Input:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "clienteNome": "João Silva Santos",
  "clienteTelefone": "11999999999",
  "dataHoraInicio": "2025-02-01T19:00:00Z",
  "dataHoraFim": "2025-02-01T20:00:00Z",
  "observacoes": "Quadra coberta"
}
```

**Exemplo de Output:**
```json
{
  "success": true,
  "errors": [],
  "content": {
    "agendamento": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "clienteNome": "João Silva Santos",
      "clienteTelefone": "11999999999",
      "dataHoraInicio": "2025-02-01T19:00:00.000Z",
      "dataHoraFim": "2025-02-01T20:00:00.000Z",
      "observacoes": "Quadra coberta",
      "dataCriacao": "2025-01-24T10:00:00.000Z",
      "dataAtualizacao": "2025-01-24T11:00:00.000Z",
      "googleCalendarEventId": "event123abc"
    }
  }
}
```

---

### 4. `agendar_deletar`

Deleta um agendamento existente.

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid",
      "description": "ID do agendamento (UUID)"
    },
    "eventId": {
      "type": "string",
      "description": "ID do evento no Google Calendar (alternativa ao id)"
    }
  },
  "required": ["id"]
}
```

**Exemplo de Input:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Exemplo de Output:**
```json
{
  "success": true,
  "errors": [],
  "content": {
    "deletado": true,
    "mensagem": "Agendamento deletado com sucesso"
  }
}
```

---

### 5. `agendar_disponibilidade`

Verifica a disponibilidade de um horário específico.

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "dataHoraInicio": {
      "type": "string",
      "format": "date-time",
      "description": "Data e hora de início para verificar disponibilidade (obrigatório)"
    },
    "dataHoraFim": {
      "type": "string",
      "format": "date-time",
      "description": "Data e hora de fim para verificar disponibilidade (obrigatório)"
    }
  },
  "required": ["dataHoraInicio", "dataHoraFim"]
}
```

**Exemplo de Input:**
```json
{
  "dataHoraInicio": "2025-02-01T18:00:00Z",
  "dataHoraFim": "2025-02-01T19:00:00Z"
}
```

**Exemplo de Output (Disponível):**
```json
{
  "success": true,
  "errors": [],
  "content": {
    "disponivel": true,
    "mensagem": "Horário disponível"
  }
}
```

**Exemplo de Output (Ocupado):**
```json
{
  "success": true,
  "errors": [],
  "content": {
    "disponivel": false,
    "mensagem": "Horário ocupado",
    "agendamentoConflitante": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "clienteNome": "João Silva",
      "dataHoraInicio": "2025-02-01T17:30:00.000Z",
      "dataHoraFim": "2025-02-01T18:30:00.000Z"
    }
  }
}
```

## 🔄 Formato de Resposta Padrão

Todas as ferramentas retornam respostas no seguinte formato:

```json
{
  "success": boolean,
  "errors": string[],
  "content": object | null
}
```

- `success`: Indica se a operação foi bem-sucedida
- `errors`: Array de mensagens de erro (vazio se `success` for `true`)
- `content`: Objeto com os dados da resposta (null se houver erros)

## ✅ Validações

Todas as ferramentas utilizam FluentValidation para validar os dados de entrada:

- **Nome do cliente**: obrigatório, máximo 200 caracteres
- **Telefone**: obrigatório, máximo 20 caracteres
- **Data/hora de início**: obrigatória, deve ser futura
- **Data/hora de fim**: obrigatória, deve ser futura e posterior à data de início
- **Observações**: opcional, máximo 1000 caracteres

## 🚨 Tratamento de Erros

Erros são retornados no campo `errors` do resultado. Tipos de erro comuns:

- **Validação**: Erros de validação do FluentValidation
- **Conflito de horário**: Quando já existe um agendamento no horário solicitado
- **Não encontrado**: Quando o agendamento não existe
- **Erro de domínio**: Outros erros de regra de negócio

