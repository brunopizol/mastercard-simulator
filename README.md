# Mastercard Purchase Simulator API

Uma aplicação ASP.NET Core 9 que simula transações de compra Mastercard e as encaminha para sistemas externos. Implementada com **DDD (Domain-Driven Design)** e **Clean Architecture**.

---

## 📋 Visão Geral

A aplicação fornece dois endpoints principais:
1. **Simular Compras** - Gera payloads de transações fictícias
2. **Encaminhar Compras** - Envia transações geradas para uma URL especificada

## 🏗️ Arquitetura e Estrutura de Pastas

A solução segue o padrão **DDD + Clean Architecture**, dividindo responsabilidades em 4 projetos:

---

## 🔄 Fluxo de Responsabilidades

### **1. Mastercard.Core (Domínio)**
Contém as **entidades e regras de negócio puras**, sem dependências externas.

**Responsabilidades:**
- ✅ Definir estrutura de dados de compra
- ✅ Exceções específicas do domínio

---

### **2. Mastercard.Application (Casos de Uso)**
Define **interfaces de contrato** e **orquestra casos de uso**, sem implementações concretas.

#### **IPurchaseGenerator** (Interface)
Contrato: "Algo gera compras de forma realista".

#### **IForwardService** (Interface)
Contrato: "Algo envia compras via HTTP para uma URL".

#### **ForwardPurchasesHandler** (Caso de Uso)
Orquestra o fluxo completo:

**Responsabilidades:**
- ✅ Orquestrar fluxos
- ✅ Validações de entrada
- ✅ Mapear entre DTOs e entidades

---

### **3. Mastercard.Infrastructure (Implementações)**
Implementa as **interfaces da Application** com tecnologias específicas.

#### **PurchaseGenerator** (Implementação)
Gera compras fictícias com dados realistas.

**Detalhes técnicos:**
- 🔐 **PAN (Primary Account Number)**: 16 dígitos com checksum Luhn válido
- 🔐 **PAN Hash**: SHA256 para proteção
- 🎲 **Dados aleatórios**: Moedas, comerciantes, valores
- 🔄 **Contadores thread-safe**: Usando `Interlocked.Increment` para sincronização

#### **HttpForwardService** (Implementação)

**Responsabilidades:**
- ✅ Comunicação HTTP real
- ✅ Tratamento de erros de rede
- ✅ Serialização JSON

---

### **4. Mastercard.Api (Apresentação)**
Expõe **endpoints HTTP** e configura a injeção de dependências.

#### **Program.cs** (Configuração)

#### **SimulationEndpoints.cs** (Rotas de Simulação)

#### **ForwardEndpoints.cs** (Rotas de Encaminhamento)

---

## 📡 Endpoints da API

### **1. GET /api/simulate**
Gera uma única transação de compra.

**Request:**

**Response (200 OK):**
````````

# Response
````````

### **3. POST /api/forward**
Encaminha compras geradas para um endpoint externo.

**Request:**

````````

**Response (200 OK):**
````````


# Response
````````markdown

````````

**Error (400 Bad Request):**
````````


# Response
````````markdown

````````

## 🔧 Injeção de Dependências

As dependências são registradas em `ServiceCollectionExtensions.cs`:

````````markdown

- **Ciclos de Vida:**
  - `Singleton` - Uma instância para toda a aplicação (gerador)
  - `Scoped` - Uma instância por requisição HTTP (handler)
  - `HttpClient` - Gerenciado automaticamente (com pool)

---

## 🚀 Executando a Aplicação

### **Pré-requisitos**
- .NET 9 SDK
- Visual Studio 2022 (opcional)

### **Comando de Build**

````````

### **Comando de Run**
````````


# Response
````````markdown

````````

A API estará disponível em `https://localhost:7000` (ou conforme configurado).

### **Acessar Swagger/OpenAPI**
````````


# Response
````````markdown

````````

---

## 🧪 Testando a API com cURL

````````

# '''Gerar uma compra
curl -X GET https://localhost:7000/api/simulate

# Gerar 5 compras
curl -X GET https://localhost:7000/api/simulate/5
# Response
````````markdown

````````

---

## 🏛️ Princípios de Design

### **DDD (Domain-Driven Design)**
- **Core** contém apenas regras de negócio puras
- **Entidades** representam conceitos do domínio (PurchasePayload)
- **Exceções de domínio** são específicas e significativas

### **Clean Architecture**
- **Inversão de Dependência**: Camadas altas não dependem de camadas baixas
- **Separação de Responsabilidades**: Cada projeto tem um propósito único
- **Testabilidade**: Interfaces permitem fácil mocking

### **Padrões Utilizados**
- **Dependency Injection**: Acoplamento mínimo via interfaces
- **Handler Pattern**: Orquestração clara de casos de uso
- **Data Transfer Objects (DTOs)**: Contratos entre camadas
- **Factory Pattern**: Geração de dados (PurchaseGenerator)

---

## 📊 Fluxo Visual

````````

````````

---

## 📝 Notas Técnicas

### **Geração de PAN (Cartão)**
- Formato: 16 dígitos iniciando com 5 (Mastercard)
- Checksum Luhn válido para cada PAN gerado
- Hash SHA256 para segurança adicional

### **Sincronização Thread-Safe**
- Contadores `_correlationCounter` e `_nsuCounter` usam `Interlocked.Increment`
- Garante unicidade mesmo em requisições paralelas

### **Configuração Swagger**
- Todos os endpoints têm tags e descrições
- OpenAPI schema disponível em `/swagger/v1/swagger.json`

---

## 🤝 Contribuindo

Para adicionar novos recursos:
1. Defina interfaces em `Application/Interfaces/`
2. Implemente em `Infrastructure/`
3. Crie handlers em `Application/UseCases/`
4. Exponha endpoints em `Api/Endpoints/`

---

## 📄 Licença

[Adicione sua licença aqui]

---

**Última atualização:** dezembro 2025
