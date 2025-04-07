using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TourBuddy.Helpers;
using TourBuddy.Models;
using TourBuddy.Services.Auth;
using TourBuddy.Services.Database;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TourBuddy.ViewModels
{
    public partial class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly SyncService _syncService;

        // Properties
        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        private string _currentPassword;
        public string CurrentPassword
        {
            get => _currentPassword;
            set => SetProperty(ref _currentPassword, value);
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

        private bool _isPasswordChanging;
        public bool IsPasswordChanging
        {
            get => _isPasswordChanging;
            set => SetProperty(ref _isPasswordChanging, value);
        }

        private bool _isSyncing;
        public bool IsSyncing
        {
            get => _isSyncing;
            set => SetProperty(ref _isSyncing, value);
        }

        private string _syncStatus;
        public string SyncStatus
        {
            get => _syncStatus;
            set => SetProperty(ref _syncStatus, value);
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
        private ICommand _changePasswordCommand;
        public ICommand ChangePasswordCommand => _changePasswordCommand ??= new Command(async () => await ChangePasswordAsync());

        private ICommand _syncDataCommand;
        public ICommand SyncDataCommand => _syncDataCommand ??= new Command(async () => await SyncDataAsync());

        private ICommand _logoutCommand;
        public ICommand LogoutCommand => _logoutCommand ??= new Command(async () => await LogoutAsync());

        // Constructor
        public ProfileViewModel(IAuthService authService, SyncService syncService)
        {
            _authService = authService;
            _syncService = syncService;
            Title = "My Profile";
            _syncService.SyncCompleted += OnSyncCompleted;
        }

        // Methods
        public async Task LoadUserDataAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ClearErrorMessage();

                CurrentUser = await _authService.GetCurrentUserAsync();

                if (CurrentUser == null)
                {
                    // Initialize empty user to avoid errors
                    CurrentUser = new User
                    {
                        Username = "Not Logged In",
                        Email = "no@email.com"
                    };

                    // Navigate to login
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user data: {ex.Message}");
                SetErrorMessage("Error loading user data.");

                // Initialize empty user to avoid errors
                if (CurrentUser == null)
                {
                    CurrentUser = new User
                    {
                        Username = "Error",
                        Email = "error@email.com"
                    };
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ChangePasswordAsync()
        {
            if (IsBusy || IsPasswordChanging)
                return;

            try
            {
                IsPasswordChanging = true;
                ClearErrorMessage();

                // Validate
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                {
                    SetErrorMessage("Current password is required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    SetErrorMessage("New password is required.");
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    SetErrorMessage("Passwords do not match.");
                    return;
                }

                if (CurrentUser == null)
                {
                    SetErrorMessage("No logged-in user.");
                    return;
                }

                bool success = await _authService.ChangePasswordAsync(
                    CurrentUser.Id, CurrentPassword, NewPassword);

                if (success)
                {
                    // Clear fields
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;

                    // Show message
                    await Application.Current.MainPage.DisplayAlert(
                        "Password Changed",
                        "Your password has been successfully changed.",
                        "OK");
                }
                else
                {
                    SetErrorMessage("Current password is incorrect.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Change password error: {ex.Message}");
                SetErrorMessage("An error occurred while changing the password.");
            }
            finally
            {
                IsPasswordChanging = false;
            }
        }

        private async Task SyncDataAsync()
        {
            if (IsSyncing)
                return;

            try
            {
                IsSyncing = true;
                SyncStatus = "Syncing...";

                await _syncService.SyncDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sync error: {ex.Message}");
                SyncStatus = "Error syncing.";
            }
        }

        private void OnSyncCompleted(object sender, bool success)
        {
            IsSyncing = false;
            SyncStatus = success ? "Sync successful" : "Sync failed";

            // Clear status after a delay
            Task.Delay(3000).ContinueWith(_ => {
                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => {
                    SyncStatus = string.Empty;
                });
            });
        }

        private async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        protected void SetErrorMessage(string message)
        {
            ErrorMessage = message;
        }

        protected void ClearErrorMessage()
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
