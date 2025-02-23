using Azure.Core;
using DataBalkTaskLisit.Entities;
using DataBalkTaskLisit.UserTaskDbContext;
using DataBalkTaskLisitAPI.Dtos;
using DataBalkTaskLisitAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Generators;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataBalkTaskLisitAPI.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class DataBalkController(UserTaskDbContext dbContext, IAuthService authService, ILogger<DataBalkController> logger, IDataBalkService dataBalkService) : ControllerBase
    {
        private readonly ILogger<DataBalkController> _logger = logger;
        private readonly UserTaskDbContext _context = dbContext;

        #region Authenticated
        [HttpPost("register-user")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
                return BadRequest("Username already exists.");

            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
        {
            var result = await authService.LoginAsync(request);
            if (result is null)
                return BadRequest("Invalid username or password.");

            return Ok(result);
        }


        [Authorize]
        [HttpGet("authenticated-user")]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            var userName = User.Identity?.Name ?? "Unknown User";
            return Ok(new { message = "You are authenticated!", user = userName });
        }
        #endregion

        #region Users
        [Authorize]
        [HttpGet("all-users")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {

             var result = await dataBalkService.GetAllUserAsync();
            return result is null ? NotFound("No users found.") : Ok(result);
            
        }

        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var result = await dataBalkService.GetUserByIdAsync(id);
            return result is null ? NotFound($"User with ID {id} not found.") : Ok(result);
            
        }

        [Authorize]
        [HttpPut("user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
          
            if (userDto == null)
                return BadRequest("Invalid user data.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var result = await dataBalkService.UpdateUserAsync(id, userDto);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("user-delete-all-fast")]
        public async Task<IActionResult> DeleteAllUsersFast()
        {
            var tASKresult = await dataBalkService.DeleteAllTaskFastAsync();
            var result = await dataBalkService.DeleteAllUsersFastAsync();
            return Ok("All Tasks have been deleted.");
        }

        [Authorize]  
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found.");
            
            var task = await _context.Tasks.FirstOrDefaultAsync(t=>t.UserId ==user.Id);
            var taskResult = await dataBalkService.DeleteTaskAsync(task.Id);
            var result = await dataBalkService.DeleteUserAsync(id);
            return Ok(new { message = $"User with ID {id} deleted successfully." });
        }

       #endregion

        #region Task
        [Authorize]
        [HttpGet("tasks")]
        public async Task<ActionResult<List<TaskListDto>>> GetAllTasks()
        {
            var results = await dataBalkService.GetAllTasksAsync();
            return  results is null ? NotFound("No tasks found.") : Ok(results);
        }


        [Authorize]
        [HttpGet("task/{id}")]
        public async Task<ActionResult<TaskListDto>> GetTaskById(int id)
        {
            var result = await dataBalkService.GetTaskByIdAsync(id);
            return result is null ? NotFound($"Task with ID {id} not found.") : Ok(result);
        }


        [Authorize]  // Restrict access to authenticated users
        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask([FromBody] TaskListDto taskDto)
        {
             
            if (taskDto == null)
            {
                return BadRequest("Task data is required.");
            }

            // Check if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == taskDto.UserId);
            if (!userExists)
            {
                return BadRequest($"User not found with ID {taskDto.UserId}");
            }
 
            var results = await dataBalkService.CreateTaskAsync(taskDto);

            var task = _context.Tasks.FirstOrDefault();
            var createdTaskDto = new TaskListDto
            {
                Id =  task.Id,
                Title = task.Title,
                Description = task.Description,
                UserId = task.UserId,
                DueDate = task.DueDate
            };

            return CreatedAtAction(nameof(CreateTask), new { id = task.Id }, createdTaskDto);
        }

        [Authorize]  // Ensure that only authenticated users can update tasks
        [HttpPut("Task/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskListDto taskDto)
        {
            if (taskDto == null)
                return BadRequest("Invalid task data.");

            // Validate that the user exists before assigning UserId
            var userExists = await _context.Users.AnyAsync(u => u.Id == taskDto.UserId);
            if (!userExists)
            {
                return BadRequest($"User with ID {taskDto.UserId} does not exist.");
            }

            var result = await dataBalkService.UpdateTaskAsync(id, taskDto);
            if (result == null)
                return NotFound($"Task with ID {id} not found.");


            var task = _context.Tasks.FirstOrDefault(t => t.Id == id);
            // Return updated task as DTO
            var updatedTaskDto = new TaskListDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                UserId = task.UserId,
                DueDate = task.DueDate
            };

            return  Ok(updatedTaskDto);
        }

        [Authorize]  // Ensure that only authenticated users can update tasks
        [HttpDelete("delete-all-fast-tasks")]
        public async Task<IActionResult> DeleteAllTaskFast()
        {
            var result = await dataBalkService.DeleteAllTaskFastAsync();
            return Ok("All Tasks have been deleted.");
        }

        [Authorize]  // Ensure only authenticated users can delete tasks
        [HttpDelete("delete-task/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound($"Task with ID {id} not found.");

            var result = await dataBalkService.DeleteTaskAsync(id);
            return Ok(new { message = $"Task with ID {id} deleted successfully." });
        }

        #endregion
    }
}
