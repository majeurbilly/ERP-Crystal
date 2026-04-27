using System.Text.Json;
using Crystal.API.Controllers;
using Crystal.Core;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Crystal.UnitTests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_Returns_Ok_With_LoginResponse_When_Service_Succeeds()
    {
        // Arrange
        LoginRequest request = new()
        {
            Username = "testuser",
            Password = "ValidPass1!"
        };

        LoginResponse expected = new()
        {
            Token = "fake-jwt-token-for-unit-test",
            UserId = "user-id-123",
            UserName = "testuser",
            Roles = new[] { ApplicationRoles.Employee }
        };

        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();

        mockAuthService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        AuthController controller = new(mockAuthService.Object);

        IActionResult result = await controller.Login(request, CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

        LoginResponse? value = Assert.IsType<LoginResponse>(ok.Value);

        Assert.Equal(expected.Token, value.Token);
        Assert.Equal(expected.UserId, value.UserId);
        Assert.Equal(expected.UserName, value.UserName);
        Assert.Equal(expected.Roles, value.Roles);

        mockAuthService.Verify(
            s => s.LoginAsync(
                It.Is<LoginRequest>(r =>
                    r.GetLoginIdentifier() == request.GetLoginIdentifier() && r.Password == request.Password),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_Returns_Unauthorized_When_Service_Returns_Null()
    {
        // Arrange
        LoginRequest request = new()
        {
            Username = "unknown",
            Password = "WrongPass1!"
        };

        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();

        mockAuthService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginResponse?)null);

        AuthController controller = new(mockAuthService.Object);

        // Act
        IActionResult result = await controller.Login(request, CancellationToken.None);

        // Assert
        UnauthorizedObjectResult unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);

        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);

        mockAuthService.Verify(
            s => s.LoginAsync(
                It.Is<LoginRequest>(r =>
                    r.GetLoginIdentifier() == request.GetLoginIdentifier() && r.Password == request.Password),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_Returns_Ok_When_Service_Succeeds()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "new@example.com",
            UserName = "newuser",
            Password = "ValidPass1!",
            Role = ApplicationRoles.Employee
        };

        RegisterResult serviceResult = new()
        {
            Succeeded = true
        };

        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();

        mockAuthService
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        AuthController controller = new(mockAuthService.Object);

        // Act
        IActionResult result = await controller.Register(request, CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

        mockAuthService.Verify(
            s => s.RegisterAsync(
                It.Is<RegisterRequest>(r =>
                    r.Email == request.Email
                    && r.UserName == request.UserName
                    && r.Password == request.Password
                    && r.Role == request.Role),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_Returns_BadRequest_When_Service_Reports_Invalid_Role()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "badrole@example.com",
            UserName = "badroleuser",
            Password = "ValidPass1!",
            Role = "Invalid"
        };

        RegisterResult serviceResult = new()
        {
            Succeeded = false,
            Errors = new[] { "Invalid role" }
        };

        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();

        mockAuthService
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        AuthController controller = new(mockAuthService.Object);

        // Act
        IActionResult result = await controller.Register(request, CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);

        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        mockAuthService.Verify(
            s => s.RegisterAsync(
                It.Is<RegisterRequest>(r =>
                    r.Email == request.Email
                    && r.UserName == request.UserName
                    && r.Password == request.Password
                    && r.Role == request.Role),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_Returns_BadRequest_With_Identity_Messages_When_User_Creation_Fails()
    {
        // Arrange 
        RegisterRequest request = new()
        {
            Email = "fail@example.com",
            UserName = "failuser",
            Password = "weak",
            Role = ApplicationRoles.Employee
        };

        string[] identityErrors =
        [
            "Le mot de passe doit contenir une majuscule.",
            "Le mot de passe doit contenir au moins 6 caractères."
        ];

        RegisterResult serviceResult = new()
        {
            Succeeded = false,
            Errors = identityErrors
        };

        Mock<IAuthService> mockAuthService = new Mock<IAuthService>();

        mockAuthService
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        AuthController controller = new(mockAuthService.Object);

        // Act
        IActionResult result = await controller.Register(request, CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);

        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        string json = JsonSerializer.Serialize(badRequest.Value);

        using JsonDocument doc = JsonDocument.Parse(json);

        JsonElement errorsElement = doc.RootElement.GetProperty("errors");

        string[] receivedErrors = errorsElement.EnumerateArray().Select(e => e.GetString()!).ToArray();

        Assert.Equal(identityErrors, receivedErrors);

        mockAuthService.Verify(
            s => s.RegisterAsync(
                It.Is<RegisterRequest>(r =>
                    r.Email == request.Email
                    && r.UserName == request.UserName
                    && r.Password == request.Password
                    && r.Role == request.Role),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
