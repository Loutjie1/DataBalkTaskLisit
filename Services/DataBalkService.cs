using DataBalkTaskLisit.Entities;
using DataBalkTaskLisit.UserTaskDbContext;
using DataBalkTaskLisitAPI.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;



namespace DataBalkTaskLisitAPI.Services
{
    public class DataBalkService (UserTaskDbContext context, IConfiguration configuration) : IDataBalkService
    {
        private readonly UserTaskDbContext _context = context;

        #region Users
        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is null)
                return null;

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Password = user.Password,
                Tasks = user.Tasks.Select(t => new TaskListDto
                {
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate
                }).ToList()
            };
            return userDto;
        }

        public async Task<List<UserDto>> GetAllUserAsync()
        {
            if (_context.Users == null)
            {
                return null;
            }
           var user = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Password = u.Password,
                    Tasks = u.Tasks.Select(t => new TaskListDto
                    {
                        Title = t.Title,
                        Description = t.Description,
                        DueDate = t.DueDate
                    }).ToList()
                })
                .ToListAsync();
            return user;
        }

        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UserDto userDto)
        {
            if (userDto == null)
                return null;

           var user = await _context.Users.FindAsync(id);

              if (userDto == null)
                return null;
            // Update user properties
            user.UserName = userDto.UserName;
            user.Email = userDto.Email;
            user.Password = userDto.Password;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new OkObjectResult(user);
        }

        public async Task<IActionResult> DeleteAllUsersFastAsync()
        {
            var user = await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
            return new OkObjectResult(user);
        }
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            // You could also consider a "soft delete" approach here
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return new OkObjectResult(user);
        }
        #endregion

        #region Tasks
        public async Task<List<TaskListDto>> GetAllTasksAsync()
        {
            if (_context.Tasks == null)
            {
                return null;
            }
            var tasks = await _context.Tasks
                     .Select(t => new TaskListDto
                     {
                         Title = t.Title,
                         Description = t.Description,
                         UserId = t.UserId,
                         DueDate = t.DueDate
                     })
                     .ToListAsync();
            return tasks;
        }

        public async Task<TaskListDto> GetTaskByIdAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task is null)
                return null;

            var taskDto = new TaskListDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                UserId = task.UserId,   
                DueDate = task.DueDate
                
            };
            return taskDto;
        }
        public async Task<IActionResult> CreateTaskAsync([FromBody] TaskListDto taskDto)
        {

            if (taskDto is null)
                return null;

            var task = new TasksList
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                UserId = taskDto.UserId,
                DueDate = taskDto.DueDate
            };

            // Add the new task to the context and save to DB
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return new OkObjectResult(task);
        }

        public async Task<IActionResult> UpdateTaskAsync(int id, [FromBody] TaskListDto taskDto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return null; 

            // Update task properties
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.UserId = taskDto.UserId;
            task.DueDate = taskDto.DueDate;

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return new OkObjectResult(taskDto);
        }

        public async Task<IActionResult> DeleteAllTaskFastAsync()
        {
            var task = await _context.Database.ExecuteSqlRawAsync("DELETE FROM TASKS");
            return new OkObjectResult(task);
        }

        public async Task<IActionResult> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            // You could also consider a "soft delete" approach here
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return new OkObjectResult(task);
        }
            #endregion

    }
}
