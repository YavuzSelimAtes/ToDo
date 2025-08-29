namespace ToDoApi.Models
{
    public class DeleteTaskDto
    {
        public required string Password { get; set; }
        public required int UserId { get; set; }
    }
}