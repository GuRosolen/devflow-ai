using Microsoft.EntityFrameworkCore;
using DevFlowAI.API.Models;

namespace DevFlowAI.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Mapeia a classe Board para uma tabela chamada "Boards" no Postgres
        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
    }
}