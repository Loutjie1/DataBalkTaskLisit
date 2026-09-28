using DataBalkTaskLisit.UserTaskDbContext;
using DataBalkTaskLisit.Entities;
using DataBalkTaskLisitAPI.Controllers;
using DataBalkTaskLisitAPI.Dtos;
using DataBalkTaskLisitAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataBalkUnitTest.Controllers
{
    [TestFixture]
    public class DataBalkControllerTests
    {
        private MockRepository mockRepository;
        private UserTaskDbContext userTaskDbContext;
        private Mock<IAuthService> mockAuthService;
        private Mock<ILogger<DataBalkController>> mockLogger;
        private Mock<IDataBalkService> mockDataBalkService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            var options = new DbContextOptionsBuilder<UserTaskDbContext>()
                .UseInMemoryDatabase(databaseName: $"DataBalkControllerTests_{TestContext.CurrentContext.Test.ID}")
                .Options;

            this.userTaskDbContext = new UserTaskDbContext(options);
            this.mockAuthService = this.mockRepository.Create<IAuthService>();
            this.mockLogger = this.mockRepository.Create<ILogger<DataBalkController>>();
            this.mockDataBalkService = this.mockRepository.Create<IDataBalkService>();
        }

        [TearDown]
        public void TearDown()
        {
            this.userTaskDbContext.Dispose();
        }

        private DataBalkController CreateDataBalkController()
        {
            return new DataBalkController(
                this.userTaskDbContext,
                this.mockAuthService.Object,
                this.mockLogger.Object,
                this.mockDataBalkService.Object);
        }

        [Test]
        public async Task Register_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            var request = new UserDto { UserName = "test", Email = "test@mail.com", Password = "pw" };
            var createdUser = new User { Id = 1, UserName = "test", Email = "test@mail.com", Password = "pw" };

            this.mockAuthService
                .Setup(s => s.RegisterAsync(request))
                .ReturnsAsync(createdUser);

            var result = await dataBalkController.Register(request);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result.Result;
            Assert.That(ok.Value, Is.SameAs(createdUser));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Login_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            var request = new UserDto { UserName = "test", Password = "pw", Email = "test@mail.com" };
            var token = new TokenResponseDto { AccessToken = "a", RefreshToken = "r" };

            this.mockAuthService
                .Setup(s => s.LoginAsync(request))
                .ReturnsAsync(token);

            var result = await dataBalkController.Login(request);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result.Result;
            Assert.That(ok.Value, Is.SameAs(token));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void AuthenticatedOnlyEndpoint_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            dataBalkController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, "unit-user")], "TestAuth"))
                }
            };

            var result = dataBalkController.AuthenticatedOnlyEndpoint();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllUsers_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, UserName = "u1", Email = "u1@mail.com", Password = "p" }
            };

            this.mockDataBalkService
                .Setup(s => s.GetAllUserAsync())
                .ReturnsAsync(users);

            var result = await dataBalkController.GetAllUsers();

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result.Result;
            Assert.That(ok.Value, Is.SameAs(users));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetUserById_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            int id = 1;
            var user = new UserDto { Id = id, UserName = "u1", Email = "u1@mail.com", Password = "p" };

            this.mockDataBalkService
                .Setup(s => s.GetUserByIdAsync(id))
                .ReturnsAsync(user);

            var result = await dataBalkController.GetUserById(id);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result.Result;
            Assert.That(ok.Value, Is.SameAs(user));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateUser_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            int id = 1;
            var userDto = new UserDto { Id = id, UserName = "updated", Email = "updated@mail.com", Password = "p" };
            var serviceResult = new OkObjectResult("updated");

            await this.userTaskDbContext.Users.AddAsync(new User
            {
                Id = id,
                UserName = "old",
                Email = "old@mail.com",
                Password = "p"
            });
            await this.userTaskDbContext.SaveChangesAsync();

            this.mockDataBalkService
                .Setup(s => s.UpdateUserAsync(id, userDto))
                .ReturnsAsync(serviceResult);

            var result = await dataBalkController.UpdateUser(id, userDto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteAllUsersFast_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            this.mockDataBalkService
                .Setup(s => s.DeleteAllTaskFastAsync())
                .ReturnsAsync(new OkObjectResult("tasks deleted"));
            this.mockDataBalkService
                .Setup(s => s.DeleteAllUsersFastAsync())
                .ReturnsAsync(new OkObjectResult("users deleted"));

            var result = await dataBalkController.DeleteAllUsersFast();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteUser_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            int id = 1;

            await this.userTaskDbContext.Users.AddAsync(new User
            {
                Id = id,
                UserName = "u1",
                Email = "u1@mail.com",
                Password = "p"
            });
            await this.userTaskDbContext.Tasks.AddAsync(new TasksList
            {
                Id = 10,
                Title = "t1",
                Description = "d1",
                UserId = id,
                DueDate = System.DateTime.UtcNow
            });
            await this.userTaskDbContext.SaveChangesAsync();

            this.mockDataBalkService
                .Setup(s => s.DeleteTaskAsync(10))
                .ReturnsAsync(new OkObjectResult("task deleted"));
            this.mockDataBalkService
                .Setup(s => s.DeleteUserAsync(id))
                .ReturnsAsync(new OkObjectResult("user deleted"));

            var result = await dataBalkController.DeleteUser(id);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllTasks_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            var tasks = new List<TaskListDto>
            {
                new TaskListDto { Id = 1, Title = "t1", Description = "d1", UserId = 1, DueDate = System.DateTime.UtcNow }
            };

            this.mockDataBalkService
                .Setup(s => s.GetAllTasksAsync())
                .ReturnsAsync(tasks);

            var result = await dataBalkController.GetAllTasks();

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result.Result;
            Assert.That(ok.Value, Is.SameAs(tasks));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetTaskById_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            int id = 1;
            var task = new TaskListDto { Id = id, Title = "t1", Description = "d1", UserId = 1, DueDate = System.DateTime.UtcNow };

            this.mockDataBalkService
                .Setup(s => s.GetTaskByIdAsync(id))
                .ReturnsAsync(task);

            var result = await dataBalkController.GetTaskById(id);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result.Result;
            Assert.That(ok.Value, Is.SameAs(task));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateTask_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            var taskDto = new TaskListDto
            {
                Title = "new",
                Description = "desc",
                UserId = 1,
                DueDate = System.DateTime.UtcNow
            };

            await this.userTaskDbContext.Users.AddAsync(new User
            {
                Id = 1,
                UserName = "u1",
                Email = "u1@mail.com",
                Password = "p"
            });

            await this.userTaskDbContext.Tasks.AddAsync(new TasksList
            {
                Id = 5,
                Title = taskDto.Title,
                Description = taskDto.Description,
                UserId = taskDto.UserId,
                DueDate = taskDto.DueDate
            });
            await this.userTaskDbContext.SaveChangesAsync();

            this.mockDataBalkService
                .Setup(s => s.CreateTaskAsync(taskDto))
                .ReturnsAsync(new OkObjectResult("created"));

            var result = await dataBalkController.CreateTask(taskDto);

            Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTask_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            int id = 2;
            var taskDto = new TaskListDto
            {
                Id = id,
                Title = "updated",
                Description = "updated desc",
                UserId = 1,
                DueDate = System.DateTime.UtcNow
            };

            await this.userTaskDbContext.Users.AddAsync(new User
            {
                Id = 1,
                UserName = "u1",
                Email = "u1@mail.com",
                Password = "p"
            });
            await this.userTaskDbContext.Tasks.AddAsync(new TasksList
            {
                Id = id,
                Title = taskDto.Title,
                Description = taskDto.Description,
                UserId = taskDto.UserId,
                DueDate = taskDto.DueDate
            });
            await this.userTaskDbContext.SaveChangesAsync();

            this.mockDataBalkService
                .Setup(s => s.UpdateTaskAsync(id, taskDto))
                .ReturnsAsync(new OkObjectResult("updated"));

            var result = await dataBalkController.UpdateTask(id, taskDto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteAllTaskFast_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            this.mockDataBalkService
                .Setup(s => s.DeleteAllTaskFastAsync())
                .ReturnsAsync(new OkObjectResult("deleted"));

            var result = await dataBalkController.DeleteAllTaskFast();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteTask_StateUnderTest_ExpectedBehavior()
        {
            var dataBalkController = this.CreateDataBalkController();
            int id = 7;

            await this.userTaskDbContext.Tasks.AddAsync(new TasksList
            {
                Id = id,
                Title = "t",
                Description = "d",
                UserId = 1,
                DueDate = System.DateTime.UtcNow
            });
            await this.userTaskDbContext.SaveChangesAsync();

            this.mockDataBalkService
                .Setup(s => s.DeleteTaskAsync(id))
                .ReturnsAsync(new OkObjectResult("deleted"));

            var result = await dataBalkController.DeleteTask(id);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Register_WhenUserExists_ReturnsBadRequest()
        {
            var controller = this.CreateDataBalkController();
            var request = new UserDto { UserName = "test", Email = "test@mail.com", Password = "pw" };

            this.mockAuthService
                .Setup(s => s.RegisterAsync(request))
                .ReturnsAsync((User)null);

            var result = await controller.Register(request);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Login_WhenInvalidCredentials_ReturnsBadRequest()
        {
            var controller = this.CreateDataBalkController();
            var request = new UserDto { UserName = "test", Email = "test@mail.com", Password = "bad" };

            this.mockAuthService
                .Setup(s => s.LoginAsync(request))
                .ReturnsAsync((TokenResponseDto)null);

            var result = await controller.Login(request);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllUsers_WhenNoneFound_ReturnsNotFound()
        {
            var controller = this.CreateDataBalkController();

            this.mockDataBalkService
                .Setup(s => s.GetAllUserAsync())
                .ReturnsAsync((List<UserDto>)null);

            var result = await controller.GetAllUsers();

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetUserById_WhenMissing_ReturnsNotFound()
        {
            var controller = this.CreateDataBalkController();

            this.mockDataBalkService
                .Setup(s => s.GetUserByIdAsync(123))
                .ReturnsAsync((UserDto)null);

            var result = await controller.GetUserById(123);

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateUser_WhenDtoIsNull_ReturnsBadRequest()
        {
            var controller = this.CreateDataBalkController();

            var result = await controller.UpdateUser(1, null);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateUser_WhenUserMissing_ReturnsNotFound()
        {
            var controller = this.CreateDataBalkController();
            var dto = new UserDto { UserName = "u", Email = "u@mail.com", Password = "p" };

            var result = await controller.UpdateUser(999, dto);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteUser_WhenUserMissing_ReturnsNotFound()
        {
            var controller = this.CreateDataBalkController();

            var result = await controller.DeleteUser(404);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllTasks_WhenNoneFound_ReturnsNotFound()
        {
            var controller = this.CreateDataBalkController();

            this.mockDataBalkService
                .Setup(s => s.GetAllTasksAsync())
                .ReturnsAsync((List<TaskListDto>)null);

            var result = await controller.GetAllTasks();

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetTaskById_WhenMissing_ReturnsNotFound()
        {
            var controller = this.CreateDataBalkController();

            this.mockDataBalkService
                .Setup(s => s.GetTaskByIdAsync(404))
                .ReturnsAsync((TaskListDto)null);

            var result = await controller.GetTaskById(404);

            Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateTask_WhenDtoIsNull_ReturnsBadRequest()
        {
            var controller = this.CreateDataBalkController();

            var result = await controller.CreateTask(null);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateTask_WhenUserMissing_ReturnsBadRequest()
        {
            var controller = this.CreateDataBalkController();
            var dto = new TaskListDto
            {
                Title = "t",
                Description = "d",
                UserId = 321,
                DueDate = System.DateTime.UtcNow
            };

            var result = await controller.CreateTask(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTask_WhenDtoIsNull_ReturnsBadRequest()
        {
            var controller = this.CreateDataBalkController();

            var result = await controller.UpdateTask(1, null);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTask_WhenUserMissing_ReturnsBadRequest()
        {
            var controller = this.CreateDataBalkController();
            var dto = new TaskListDto
            {
                Id = 1,
                Title = "t",
                Description = "d",
                UserId = 999,
                DueDate = System.DateTime.UtcNow
            };

            var result = await controller.UpdateTask(1, dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateTask_WhenServiceReturnsNull_ReturnsNotFound()
        {
            var controller = this.CreateDataBalkController();
            var dto = new TaskListDto
            {
                Id = 22,
                Title = "t",
                Description = "d",
                UserId = 1,
                DueDate = System.DateTime.UtcNow
            };

            await this.userTaskDbContext.Users.AddAsync(new User
            {
                Id = 1,
                UserName = "u1",
                Email = "u1@mail.com",
                Password = "p"
            });
            await this.userTaskDbContext.SaveChangesAsync();

            this.mockDataBalkService
                .Setup(s => s.UpdateTaskAsync(22, dto))
                .ReturnsAsync((IActionResult)null);

            var result = await controller.UpdateTask(22, dto);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteTask_WhenMissing_ReturnsNotFound()
        {
            var controller = this.CreateDataBalkController();

            var result = await controller.DeleteTask(333);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            this.mockRepository.VerifyAll();
        }
    }
}
