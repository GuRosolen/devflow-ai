namespace DevFlowAI.API.Models
{
    public class Column
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // Ex: "To Do", "Doing"
        
        // Relacionamento: Toda coluna pertence a um Board
        public int BoardId { get; set; }
    }
}