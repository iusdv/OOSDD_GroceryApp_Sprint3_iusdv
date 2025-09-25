using Grocery.Core.Models;

namespace Grocery.Core.Interfaces.Services
{
    public interface IRegistrationService
    {
        Task<(bool ok, string? error, Client? client)> RegisterAsync(string email, string password);
    }
}