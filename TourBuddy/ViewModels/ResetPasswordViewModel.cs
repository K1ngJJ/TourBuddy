using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TourBuddy.Helpers;
using TourBuddy.Services.Auth;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TourBuddy.ViewModels
{
    public partial class ResetPasswordViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;

        // Properties
        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _token;
        public string Token
        {
            get => _token;
            set => SetProperty(ref _token, value);
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
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
        private ICommand _resetPasswordCommand;
        public ICommand ResetPasswordCommand => _resetPasswordCommand ??= new Command(async () => await ResetPasswordAsync());

        private ICommand _goBackCommand;
        public ICommand GoBackCommand => _goBackCommand ??= new Command(async () => await GoBackAsync());

        // Constructor
        public ResetPasswordViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Reset Password";
        }

        public void Initialize(string email)
        {
            Email = email;
        }

        // Methods
        private async Task ResetPasswordAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ClearErrorMessage();

                // Validate passwords
                var (isValid, message) = ValidationHelper.ValidatePasswordReset(NewPassword, ConfirmPassword);
                if (!isValid)
                {
                    SetErrorMessage(message);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Token))
                {
                    SetErrorMessage("Verification code is required.");
                    return;
                }

                bool success = await _authService.ResetPasswordAsync(Email, Token, NewPassword);

                if (success)
                {
                    // Clear sensitive data
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;
                    Token = string.Empty;

                    // Show success message
                    await Application.Current.MainPage.DisplayAlert(
                        "Password Reset",
                        "Your password has been reset successfully. You can now log in with your new password.",
                        "OK");

                    // Navigate back to login
                    await Shell.Current.GoToAsync("//LoginPage");
                }
                else
                {
                    SetErrorMessage("The verification code is invalid or expired.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password reset error: {ex.Message}");
                SetErrorMessage("An error occurred while resetting the password. Please try again.");
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
