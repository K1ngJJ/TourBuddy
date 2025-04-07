using TourBuddy.ViewModels;

namespace TourBuddy.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ForgotPasswordViewModel _viewModel;
    public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}