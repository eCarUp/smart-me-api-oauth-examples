using ApplicationCodeWithPkceMauiApp.ViewModels;

namespace ApplicationCodeWithPkceMauiApp.Presentations;

public partial class OAuthLoginPage : ContentPage
{
    OAuthLoginViewModel? vm => BindingContext as OAuthLoginViewModel;

    public OAuthLoginPage(OAuthLoginViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.TryLoginAndRedirectWithOauth();
    }
}