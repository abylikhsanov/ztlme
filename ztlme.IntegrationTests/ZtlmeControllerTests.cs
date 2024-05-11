using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using ztlme.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ztlme.Data;
using Microsoft.Extensions.DependencyInjection;
using ztlme.Models;


namespace ztlme.IntegrationTests;

public class ZtlmeControllerTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
//public class ZtlmeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program>
        _factory;

    private readonly HttpClient _client;

    public ZtlmeControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private void CreateDbScope()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<DataContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
    
    private StringContent CreateJsonContent<T>(T obj)
    {
        var jsonContent = JsonSerializer.Serialize(obj);
        return new StringContent(jsonContent, Encoding.UTF8, "application/json");
    }

    [Fact]
    public async Task SignupUserRequest_CreditScoreHigh()
    {
        CreateDbScope();
        SignupUserReqDto request = new SignupUserReqDto
        {
            PersonalNumber = "30070721151"
        };
        var content = CreateJsonContent(request);
        
        // Act
        var response = await _client.PostAsync("/api/Auth/signup", content);
        
        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode} ({response.ReasonPhrase}). Server response: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        var signupUserResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<SignupUserResDto>>();
        signupUserResponse?.Data?.CanBeSignedUp.Should().BeTrue();
        signupUserResponse?.Data?.DocumentSigned.Should().BeFalse();
    }

    [Fact]
    public async Task SignupUserRequest_CreditScoreLow()
    {
        CreateDbScope();
        SignupUserReqDto request = new SignupUserReqDto
        {
            PersonalNumber = "12345678912"
        };

        var content = CreateJsonContent(request);
        
        // Act
        var response = await _client.PostAsync("/api/Auth/signup", content);
        
        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode} ({response.ReasonPhrase}). Server response: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        var signupUserResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<SignupUserResDto>>();
        signupUserResponse?.Data?.CanBeSignedUp.Should().BeFalse();
        signupUserResponse?.Data?.DocumentSigned.Should().BeFalse();
    }

    [Fact]
    public async Task SignupUserRequest_CreditScoreLowExisting()
    {
        CreateDbScope();
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<DataContext>();
            db.Users.Add(new User
            {
                UserName = "12345678912",
                CreditScoreOk = false
            });
            await db.SaveChangesAsync();
        }
        SignupUserReqDto request = new SignupUserReqDto
        {
            PersonalNumber = "12345678912"
        };

        var content = CreateJsonContent(request);
        
        // Act
        var response = await _client.PostAsync("/api/Auth/signup", content);
        
        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode} ({response.ReasonPhrase}). Server response: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        var signupUserResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<SignupUserResDto>>();
        signupUserResponse?.Data?.CanBeSignedUp.Should().BeFalse();
        signupUserResponse?.Data?.DocumentSigned.Should().BeFalse();
        signupUserResponse?.Data?.CreditScoreApiCalled?.Should().BeFalse();
    }
    
    [Fact]
    public async Task SignupUserRequest_CreditScoreHighExisting()
    {
        CreateDbScope();
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<DataContext>();
            db.Users.Add(new User
            {
                UserName = "30070721151",
                CreditScoreOk = true,
                SignedBlob = "SomeBlob"
            });
            await db.SaveChangesAsync();
        }
        SignupUserReqDto request = new SignupUserReqDto
        {
            PersonalNumber = "30070721151"
        };

        var content = CreateJsonContent(request);
        
        // Act
        var response = await _client.PostAsync("/api/Auth/signup", content);
        
        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode} ({response.ReasonPhrase}). Server response: {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        var signupUserResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<SignupUserResDto>>();
        signupUserResponse?.Data?.CanBeSignedUp.Should().BeTrue();
        signupUserResponse?.Data?.DocumentSigned.Should().BeTrue();
        signupUserResponse?.Data?.CreditScoreApiCalled?.Should().BeFalse();
    }
    
}