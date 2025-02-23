using DataBalkTaskLisit.Entities;

namespace DataBalkTaskLisitAPI.Dtos
{
    public class TaskListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        // Foreign Key
        public int UserId { get; set; }
        public DateTime DueDate { get; set; }
        //public string Assignee { get; set; }

        // Navigation Property
       //public UserDto user { get; set; }
    }
}
