using FluentAssertions;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.Validators.Auth;

namespace PharmacyManagement.Application.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var result = _validator.Validate(new LoginRequest { Username = "admin", Password = "Admin@12345" });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_username_fails()
    {
        var result = _validator.Validate(new LoginRequest { Username = "", Password = "x" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Empty_password_fails()
    {
        var result = _validator.Validate(new LoginRequest { Username = "admin", Password = "" });
        result.IsValid.Should().BeFalse();
    }
}
