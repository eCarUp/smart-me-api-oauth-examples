using ApplicationCodeWithPkceMauiApp.Presentations;
using ApplicationCodeWithPkceMauiApp.ViewModels;

namespace ApplicationCodeWithPkceMauiApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLoginWithSmartMeClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new OAuthLoginPage(new OAuthLoginViewModel()));
        }
    }
}
