namespace ToDoApi.Models
{
    public class CreateTaskDto
    {
        public required string Title { get; set; }
        public required string Category { get; set; }
        public State State { get; set; } = 0;
        
        // Frontend'den gelen tarihi alacak olan yeni alan
        public DateTime CreatedAt { get; set; }
    }
}