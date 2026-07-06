using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DevFlowAI.API.Models
{
    public class ActivityLog
    {
        // O MongoDB usa um ID de texto aleatório gerado por ele mesmo
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        public string Action { get; set; } = string.Empty; // Ex: "Moved Task"
        public string Details { get; set; } = string.Empty; // Ex: "Tarefa 1 movida para Doing"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}