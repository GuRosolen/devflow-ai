# DevFlow AI 🚀

Um sistema de gerenciamento de tarefas (Kanban) Full Stack com arquitetura de persistência poliglota e integração com Inteligência Artificial. Desenvolvido como projeto prático para demonstrar o domínio de tecnologias modernas exigidas pelo mercado corporativo.

## 🎯 O Projeto

O DevFlow AI vai além de um simples "To-Do List". Ele é um painel Kanban interativo que utiliza IA para quebrar tarefas complexas em subtarefas acionáveis. Além disso, o sistema implementa **Persistência Poliglota**, utilizando o banco de dados adequado para cada tipo de informação: dados estruturados no PostgreSQL e logs de movimentação em alta velocidade no MongoDB.

## 🛠️ Tecnologias Utilizadas

**Front-end:**
*   React (com Vite para build ultrarrápido)
*   TypeScript / JavaScript
*   Axios (Integração HTTP)

**Back-end:**
*   .NET 8 (C#)
*   Minimal APIs
*   Entity Framework Core (ORM)

**Bancos de Dados & Infraestrutura:**
*   PostgreSQL (Relacional - Gerenciamento das Tarefas e Colunas)
*   MongoDB (NoSQL - Histórico de logs de movimentação)
*   Docker & Docker Compose (Containerização dos bancos)

**Inteligência Artificial:**
*   Integração direta via HTTP com a API oficial do Google Gemini (Modelo `gemini-2.5-flash`).

## ✨ Principais Funcionalidades

*   **Kanban Interativo:** Criação e movimentação de cards entre as colunas "To Do", "Doing" e "Done".
*   **Assistente de IA Integrado:** Geração automática de subtarefas e planos de ação baseados no título da tarefa utilizando o Google Gemini.
*   **Histórico de Atividades (Logs):** Registro automático de toda movimentação de cards utilizando um banco de dados não-relacional (MongoDB).

## 🚀 Como Executar o Projeto Localmente

### Pré-requisitos
*   [Docker Desktop](https://www.docker.com/products/docker-desktop) instalado e rodando.
*   [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado.
*   [Node.js](https://nodejs.org/) instalado.
*   Uma chave de API gratuita do [Google AI Studio](https://aistudio.google.com/).

### 1. Subindo os Bancos de Dados (Docker)
Na raiz do projeto (onde está o arquivo `docker-compose.yml`), abra o terminal e rode:
```bash
docker compose up -d
Isso iniciará o PostgreSQL (porta 5433) e o MongoDB (porta 27018).

2. Configurando e Rodando a API (.NET)
Entre na pasta do backend:

Bash
cd backend/devflowai.api
Abra o arquivo Program.cs e insira sua chave da API do Google Gemini na variável apiKey:

C#
string apiKey = "SUA_CHAVE_AQUI"; 
Rode o projeto para aplicar as migrations e iniciar o servidor:

Bash
dotnet run
A API estará disponível em http://localhost:5009.

3. Rodando o Front-end (React)
Em um novo terminal, entre na pasta do frontend:

Bash
cd frontend/devflow-ui
Instale as dependências e inicie o servidor de desenvolvimento:

Bash
npm install
npm run dev
Acesse a aplicação no seu navegador em http://localhost:5173.

🛣️ Rotas da API (Endpoints)
GET /columns/{id}/tasks: Retorna as tarefas de uma coluna específica (PostgreSQL).

POST /tasks: Cria uma nova tarefa (PostgreSQL).

PUT /tasks/{id}/move: Move a tarefa de coluna (PostgreSQL) e gera um log de auditoria (MongoDB).

GET /logs: Retorna o histórico de movimentação dos cards (MongoDB).

POST /ai/suggest: Envia o título da tarefa para o Gemini e retorna sugestões estruturadas.