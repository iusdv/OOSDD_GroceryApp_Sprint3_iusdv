using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IRegistrationService _registrationService;
        private readonly GlobalViewModel _global;

        // ---- Login fields ----
        [ObservableProperty]
        private string email = "user3@mail.com";

        [ObservableProperty]
        private string password = "user3";

        [ObservableProperty]
        private string loginMessage;

        // ---- Register fields (email + password only) ----
        [ObservableProperty]
        private string registerEmail = string.Empty;

        [ObservableProperty]
        private string registerPassword = string.Empty;

        [ObservableProperty]
        private string registerMessage;

        public LoginViewModel(IAuthService authService,
                              IRegistrationService registrationService,
                              GlobalViewModel global)
        {
            _authService = authService;
            _registrationService = registrationService;
            _global = global;
        }

        [RelayCommand]
        private void Login()
        {
            LoginMessage = string.Empty;

            Client? authenticatedClient = _authService.Login(Email, Password);
            if (authenticatedClient != null)
            {
                LoginMessage = $"Welkom {authenticatedClient.Name}!";
                _global.Client = authenticatedClient;

                // Enter the app (Shell with tabs)
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                LoginMessage = "Ongeldige inloggegevens.";
            }
        }

        [RelayCommand]
        private async Task Register()
        {
            RegisterMessage = string.Empty;

            var (ok, error, _client) = await _registrationService.RegisterAsync(RegisterEmail, RegisterPassword);
            if (!ok)
            {
                RegisterMessage = error ?? "Registreren is mislukt.";
                return;
            }

            // Auto-login after successful registration
            var loggedIn = _authService.Login(RegisterEmail, RegisterPassword);
            if (loggedIn is null)
            {
                RegisterMessage = "Geregistreerd, maar inloggen is mislukt. Probeer in te loggen.";
                return;
            }

            _global.Client = loggedIn;
            Application.Current.MainPage = new AppShell();
        }
    }
}
