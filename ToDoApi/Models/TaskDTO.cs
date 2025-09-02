namespace ToDoApi.Models
{
    // Frontend'e güvenli ve temiz bir görev nesnesi göndermek için bu DTO'yu kullanacağız.
    public class TaskDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Category { get; set; }
        public State State { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}