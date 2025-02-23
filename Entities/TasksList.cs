using System.Security.Principal;

namespace DataBalkTaskLisit.Entities
{
    public class TasksList
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        // Foreign Key
        public int UserId { get; set; }
        public DateTime DueDate { get; set; }

        // Navigation Property
        public User user { get; set; }
    }
}
