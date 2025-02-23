using System.ComponentModel.DataAnnotations;

namespace DataBalkTaskLisit.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]  // Ensures UserName is required
        public string UserName { get; set; }
        [Required]  // Ensures Email is required
        [EmailAddress]  // Ensures correct email format
        public string Email { get; set; }
        [Required]  // Ensures Password is required
        public string Password { get; set; }


        // Navigation Property
        public ICollection<TasksList> Tasks { get; set; } = new List<TasksList>();
    }
}
