using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ActiLink.DTOs;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using ActiLink.Model;
using ActiLink.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;


namespace ActiLink.IntegrationTests
{
    [TestClass]
    public class AuthorizationTests
    {
        private HttpClient _client = null!;
        private string _token = null!;

        [TestInitialize]
        public void Setup()
        {
            var factory = new CustomWebApplicationFactory();
            _client = factory.CreateClient();

            using var scope = factory.TestServices.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Organizer>>();
            var tokenProvider = scope.ServiceProvider.GetRequiredService<JwtTokenProvider>();

            var user = new User("TestUser", "testuser@example.com");
            var result = userManager.CreateAsync(user, "Test_password123!").Result;
            Assert.IsTrue(result.Succeeded, "Nie udało się stworzyć użytkownika");

            _token = tokenProvider.GenerateAccessToken(user);
        }

        [TestMethod]
        public async Task GetUsers_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/users");
            // Act
            var response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetUsers_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.value");
            // Act
            var response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [TestMethod]
        public async Task GetUsers_WithValidToken_AcceptsToken()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            // Act
            var response = await _client.SendAsync(request);
            // Assert
            Assert.IsTrue(response.StatusCode != HttpStatusCode.Unauthorized, "Should be authorized.");
        }
        [TestMethod]
        public async Task GetUserById_WithoutToken_ReturnsUnauthorized()
        {
            var id = Guid.NewGuid();
            var response = await _client.GetAsync($"/users/{id}");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetUserById_WithInvalidToken_ReturnsUnauthorized()
        {
            var id = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Get, $"/users/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.value");
            var response = await _client.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetUserById_WithValidToken_AcceptsToken()
        {
            var id = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Get, $"/users/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _client.SendAsync(request);
            Assert.IsTrue(response.StatusCode != HttpStatusCode.Unauthorized, "Should be authorized");
        }

        [TestMethod]
        public async Task CreateEvent_WithoutToken_ReturnsUnauthorized()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/events")
            {
                Content = JsonContent.Create(new
                {
                    StartTime = DateTime.UtcNow.AddDays(1),
                    EndTime = DateTime.UtcNow.AddDays(2),
                    Latitude = 50.061,
                    Longitude = 19.938,
                    Price = 10.5m,
                    MinUsers = 5,
                    MaxUsers = 20,
                    RelatedHobbyIds = new List<Guid>() 
                })
            };

            var response = await _client.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [TestMethod]
        public async Task CreateEvent_WithInvalidToken_ReturnsUnauthorized()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/events");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.value");

            request.Content = JsonContent.Create(new
            {
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Latitude = 50.061,
                Longitude = 19.938,
                Price = 10.5m,
                MinUsers = 5,
                MaxUsers = 20
            });

            var response = await _client.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [TestMethod]
        public async Task CreateEvent_WithValidToken_AcceptsToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/events");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            request.Content = JsonContent.Create(new
            {
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Latitude = 50.061,
                Longitude = 19.938,
                Price = 10.5m,
                MinUsers = 5,
                MaxUsers = 20
            });

            var response = await _client.SendAsync(request);
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Should be authorized");
            Assert.IsTrue(response.StatusCode is HttpStatusCode.Created or HttpStatusCode.BadRequest);
        }
        [TestMethod]
        public async Task GetEventById_WithoutToken_ReturnsUnauthorized()
        {
            var id = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Get, $"/events/{id}");
            var response = await _client.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [TestMethod]
        public async Task GetEventById_WithInvalidToken_ReturnsUnauthorized()
        {
            var id = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Get, $"/events/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.value");

            var response = await _client.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [TestMethod]
        public async Task GetEventById_WithValidToken_AcceptsToken()
        {
            var id = Guid.NewGuid(); 
            var request = new HttpRequestMessage(HttpMethod.Get, $"/events/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(request);
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.OK);
        }
        [TestMethod]
        public async Task GetAllEvents_WithoutToken_ReturnsUnauthorized()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/events");
            var response = await _client.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [TestMethod]
        public async Task GetAllEvents_WithInvalidToken_ReturnsUnauthorized()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/events");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.value");

            var response = await _client.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [TestMethod]
        public async Task GetAllEvents_WithValidToken_AcceptsToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/events");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
