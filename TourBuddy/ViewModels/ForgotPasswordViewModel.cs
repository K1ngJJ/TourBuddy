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
    public partial class ForgotPasswordViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;

        // Properties
        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
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
        private ICommand _requestPasswordResetCommand;
        public ICommand RequestPasswordResetCommand => _requestPasswordResetCommand ??= new Command(async () => await RequestPasswordResetAsync());

        private ICommand _goBackCommand;
        public ICommand GoBackCommand => _goBackCommand ??= new Command(async () => await GoBackAsync());

        // Constructor
        public ForgotPasswordViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Forgot Password";
        }

        // Methods
        private async Task RequestPasswordResetAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ClearErrorMessage();

                // Validate email
                var (isValid, message) = ValidationHelper.ValidatePasswordResetRequest(Email);
                if (!isValid)
                {
                    SetErrorMessage(message);
                    return;
                }

                bool success = await _authService.RequestPasswordResetAsync(Email);

                if (success)
                {
                    // Navigate to ResetPasswordPage with the email parameter
                    var navigationParameter = new Dictionary<string, object>
                    {
                        { "Email", Email }
                    };

                    await Shell.Current.GoToAsync($"ResetPasswordPage", navigationParameter);
                }
                else
                {
                    SetErrorMessage("No account found with this email.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password reset request error: {ex.Message}");
                SetErrorMessage("An error occurred while requesting the password reset. Please try again.");
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
