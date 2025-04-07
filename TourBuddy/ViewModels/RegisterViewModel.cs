using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TourBuddy.Helpers;
using TourBuddy.Services.Auth;
using TourBuddy.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TourBuddy.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;

        // Properties
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // Commands
        private ICommand _registerCommand;
        public ICommand RegisterCommand => _registerCommand ??= new Command(async () => await RegisterAsync());

        private ICommand _goBackCommand;
        public ICommand GoBackCommand => _goBackCommand ??= new Command(async () => await GoBackAsync());

        // Constructor
        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Register";
        }

        // Methods
        private async Task RegisterAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ClearErrorMessage();

                // Validate input
                var (isValid, message) = ValidationHelper.ValidateRegisterInput(
                    Username, Email, Password, ConfirmPassword);

                if (!isValid)
                {
                    SetErrorMessage(message);
                    return;
                }

                bool success = await _authService.RegisterUserAsync(Username, Email, Password);

                if (success)
                {
                    // Clear fields
                    Username = string.Empty;
                    Email = string.Empty;
                    Password = string.Empty;
                    ConfirmPassword = string.Empty;

                    // Show success message
                    await Application.Current.MainPage.DisplayAlert(
                        "Registration Successful",
                        "Your account has been created successfully. You can now log in.",
                        "OK");

                    // Navigate back to login
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    SetErrorMessage("The email is already in use or there was an error during registration.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                SetErrorMessage("An error occurred while attempting to register. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private void SetErrorMessage(string message)
        {
            ErrorMessage = message;
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
