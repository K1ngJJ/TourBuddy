namespace TourBuddy.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using TourBuddy.ViewModels;

public partial class ResetPasswordPage : ContentPage
{
    private readonly ResetPasswordViewModel _viewModel;

    public ResetPasswordPage(ResetPasswordViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}