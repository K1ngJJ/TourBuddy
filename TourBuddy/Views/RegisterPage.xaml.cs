using TourBuddy.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

using Microsoft.Maui.Graphics;



namespace TourBuddy.Views;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _viewModel;
    public RegisterPage(RegisterViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
