using Avalonia.Controls;
using Avalonia.Interactivity;
using rabota.Services;
using System.Threading.Tasks;

namespace rabota;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
        var regBtn = this.FindControl<Button>("RegisterButton");
        if (regBtn != null) regBtn.Click += OnRegister;
        var cancelBtn = this.FindControl<Button>("CancelButton");
        if (cancelBtn != null) cancelBtn.Click += (s, e) => Close();
    }

    private async void OnRegister(object? sender, RoutedEventArgs e)
    {
        var fullNameBox = this.FindControl<TextBox>("FullNameBox");
        var usernameBox = this.FindControl<TextBox>("UsernameBox");
        var passwordBox = this.FindControl<TextBox>("PasswordBox");
        var roleCombo = this.FindControl<ComboBox>("RoleCombo");
        var errorText = this.FindControl<TextBlock>("ErrorText");

        string fullName = fullNameBox?.Text ?? "";
        string username = usernameBox?.Text ?? "";
        string password = passwordBox?.Text ?? "";
        string role = (roleCombo?.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Администратор" ? "admin" : "cashier";

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            if (errorText != null) errorText.Text = "Заполните все поля";
            return;
        }

        bool success = await App.DB!.RegisterUserAsync(username, password, role, fullName);
        if (success)
        {
            var dialog = new Window
            {
                Title = "Успех",
                Content = "Пользователь зарегистрирован",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            await dialog.ShowDialog(this);
            Close();
        }
        else
        {
            if (errorText != null) errorText.Text = "Пользователь с таким логином уже существует";
        }
    }
}