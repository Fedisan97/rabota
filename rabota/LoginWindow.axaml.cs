using Avalonia.Controls;
using Avalonia.Interactivity;
using rabota.Services;
using System.Threading.Tasks;

namespace rabota;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        var loginBtn = this.FindControl<Button>("LoginButton");
        if (loginBtn != null) loginBtn.Click += OnLoginClick;
        var regBtn = this.FindControl<Button>("RegisterButton");
        if (regBtn != null) regBtn.Click += OnRegisterClick;
    }

    private async void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        var usernameBox = this.FindControl<TextBox>("UsernameBox");
        var passwordBox = this.FindControl<TextBox>("PasswordBox");
        var errorText = this.FindControl<TextBlock>("ErrorText");

        string username = usernameBox?.Text ?? "";
        string password = passwordBox?.Text ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            if (errorText != null) errorText.Text = "Введите логин и пароль";
            return;
        }

        var user = await App.DB!.AuthenticateAsync(username, password);
        if (user == null)
        {
            if (errorText != null) errorText.Text = "Неверный логин или пароль";
            return;
        }

        App.CurrentUser = user;
        var mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
    }

    private void OnRegisterClick(object? sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterWindow();
        registerWindow.ShowDialog(this);
    }
}