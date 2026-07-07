using Microsoft.EntityFrameworkCore;
using DevFlowAI.API.Data;
using DevFlowAI.API.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// 1. Configura o Banco de Dados (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Adiciona os serviços do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Libera o CORS para o React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:5173") // A porta do seu Vite
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// 3. Configura o Banco de Dados (MongoDB)
var mongoClient = new MongoClient(builder.Configuration["MongoDb"]);
var mongoDatabase = mongoClient.GetDatabase("DevFlowDB");
builder.Services.AddSingleton(mongoDatabase);

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("PermitirTudo");

// 3. Ativa a interface visual do Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

// Desativado para limpar o aviso amarelo do terminal
// app.UseHttpsRedirection();

// --- ROTAS DO KANBAN ---

app.MapGet("/boards", async (AppDbContext db) =>
{
    var boards = await db.Boards.ToListAsync();
    return Results.Ok(boards);
});

app.MapPost("/boards", async (Board novoBoard, AppDbContext db) =>
{
    db.Boards.Add(novoBoard);
    await db.SaveChangesAsync();
    return Results.Created($"/boards/{novoBoard.Id}", novoBoard);
});

// --- ROTAS DE COLUNAS ---

// Lista colunas de um board específico
app.MapGet("/boards/{boardId}/columns", async (int boardId, AppDbContext db) =>
{
    var columns = await db.Columns.Where(c => c.BoardId == boardId).ToListAsync();
    return Results.Ok(columns);
});

// Cria uma nova coluna
app.MapPost("/columns", async (Column novaColuna, AppDbContext db) =>
{
    db.Columns.Add(novaColuna);
    await db.SaveChangesAsync();
    return Results.Created($"/columns/{novaColuna.Id}", novaColuna);
});

// --- ROTAS DE TAREFAS ---

// Lista tarefas de uma coluna específica
app.MapGet("/columns/{columnId}/tasks", async (int columnId, AppDbContext db) =>
{
    var tasks = await db.TaskItems.Where(t => t.ColumnId == columnId).ToListAsync();
    return Results.Ok(tasks);
});

// Cria uma nova tarefa
app.MapPost("/tasks", async (TaskItem novaTarefa, AppDbContext db) =>
{
    db.TaskItems.Add(novaTarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{novaTarefa.Id}", novaTarefa);
});

// Mover uma tarefa para outra coluna
app.MapPut("/tasks/{id}/move", async (int id, int newColumnId, AppDbContext db, IMongoDatabase mongo) =>
{
    // 1. Busca a tarefa no Postgres
    var task = await db.TaskItems.FindAsync(id);
    if (task is null) return Results.NotFound();

    int oldColumnId = task.ColumnId;
    
    // 2. Atualiza a coluna no Postgres
    task.ColumnId = newColumnId;
    await db.SaveChangesAsync();
    
    // 3. Salva o histórico no MongoDB (NoSQL)
    var logCollection = mongo.GetCollection<ActivityLog>("ActivityLogs");
    var log = new ActivityLog
    {
        Action = "Task Moved",
        Details = $"A tarefa '{task.Title}' foi movida da coluna {oldColumnId} para a coluna {newColumnId}."
    };
    await logCollection.InsertOneAsync(log);
    
    return Results.NoContent(); 
});

// --- ROTA DE LOGS (MONGODB) ---

// Listar todo o histórico de atividades
app.MapGet("/logs", async (IMongoDatabase mongo) =>
{
    var logCollection = mongo.GetCollection<ActivityLog>("ActivityLogs");
    // Busca todos os logs e ordena do mais recente para o mais antigo
    var logs = await logCollection.Find(_ => true).SortByDescending(l => l.Timestamp).ToListAsync();
    return Results.Ok(logs);
});

// --- ROTA DE INTELIGÊNCIA ARTIFICIAL (GEMINI API) ---
app.MapPost("/ai/suggest", async (TaskItem task, IConfiguration config) =>
{
    string apiKey = config["GeminiApiKey"]; 
    
    if (string.IsNullOrEmpty(apiKey))
        return Results.Problem("A chave da API não foi encontrada nas configurações.");

    string prompt = $"Você é um assistente de produtividade. O usuário tem uma tarefa chamada '{task.Title}'. Sugira 3 passos curtos e práticos para concluir isso. Retorne apenas os passos em formato de tópicos (- passo 1...), sem introdução.";
    
    using var client = new HttpClient();
    var requestBody = new
    {
        contents = new[]
        {
            new { parts = new[] { new { text = prompt } } }
        }
    };

    var response = await client.PostAsJsonAsync($"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}", requestBody);
    
    // 1. Verifica se o Google recusou a chamada (ex: chave errada)
    if (!response.IsSuccessStatusCode)
    {
        var erroGoogle = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"\n❌ ERRO DO GOOGLE: {erroGoogle}\n");
        return Results.Problem("Erro ao conectar com a IA.");
    }

    // 2. Tenta ler o texto gerado
    try 
    {
        var jsonResult = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var iaText = jsonResult
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return Results.Ok(new { suggestion = iaText });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ ERRO AO LER O JSON: {ex.Message}\n");
        return Results.Problem("Erro ao processar a resposta da IA.");
    }
});
app.Run();