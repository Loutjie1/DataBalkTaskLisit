using DataBalkTaskLisit.Entities;
using DataBalkTaskLisitAPI.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace DataBalkTaskLisitAPI.Services
{
    public interface IDataBalkService
    {
        #region user Interface
        Task<UserDto> GetUserByIdAsync(int request);
        Task <List<UserDto>> GetAllUserAsync();
        Task<IActionResult> UpdateUserAsync(int id, [FromBody] UserDto userDto);
        Task<IActionResult> DeleteAllUsersFastAsync();
        Task<IActionResult> DeleteUserAsync(int id);
        #endregion

        #region Task Interface
        Task<List<TaskListDto>> GetAllTasksAsync();
        Task<TaskListDto> GetTaskByIdAsync(int request);
        Task<IActionResult> CreateTaskAsync([FromBody] TaskListDto taskDto);
        Task<IActionResult> UpdateTaskAsync(int id, [FromBody] TaskListDto taskDto);
        Task<IActionResult> DeleteAllTaskFastAsync();
        Task<IActionResult> DeleteTaskAsync(int id);

        #endregion

    }
}
