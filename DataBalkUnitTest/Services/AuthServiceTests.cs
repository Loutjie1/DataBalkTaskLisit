using DataBalkTaskLisit.UserTaskDbContext;
using DataBalkTaskLisitAPI.Dtos;
using DataBalkTaskLisitAPI.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace DataBalkUnitTest.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private MockRepository mockRepository;

        private Mock<UserTaskDbContext> mockUserTaskDbContext;
        private Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockUserTaskDbContext = this.mockRepository.Create<UserTaskDbContext>();
            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private AuthService CreateService()
        {
            return new AuthService(
                this.mockUserTaskDbContext.Object,
                this.mockConfiguration.Object);
        }

        [Test]
        public async Task LoginAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            UserDto request = null;

            // Act
            var result = await service.LoginAsync(
                request);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task RegisterAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            UserDto request = null;

            // Act
            var result = await service.RegisterAsync(
                request);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task RefreshTokensAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            RefreshTokenRequestDto request = null;

            // Act
            var result = await service.RefreshTokensAsync(
                request);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
