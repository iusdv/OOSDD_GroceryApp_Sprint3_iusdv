using Grocery.Core.Helpers;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IClientService _clientService;

        public RegistrationService(IClientService clientService)
        {
            _clientService = clientService;
        }

        public async Task<(bool ok, string? error, Client? client)> RegisterAsync(string email, string password)
        {
            await Task.Yield();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Please fill in all fields.", null);

            if (!email.Contains("@"))
                return (false, "Email address looks invalid.", null);
            
            if (_clientService.Get(email.Trim()) is not null)
                return (false, "Email is already in use.", null);
            
            var displayName = email.Split('@')[0];

            var client = new Client(
                id: 0,
                name: displayName,
                emailAddress: email.Trim(),
                password: PasswordHelper.HashPassword(password)
            );

            _clientService.Add(client);
            return (true, null, client);
        }
    }
}