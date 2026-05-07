using Avalonia.Controls;
using Avalonia.Interactivity;
using rabota.ViewModels;
using rabota.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rabota;

public partial class MainWindow : Window
{
    private List<SessionViewModel> _allSessions = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadGenresAndSessions();
        ApplyFiltersBtn.Click += async (s, e) => await ApplyFilters();
        ClearDateBtn.Click += (s, e) => { DateFilter.SelectedDate = null; _ = ApplyFilters(); };
    }

    private async Task LoadGenresAndSessions()
    {
        try
        {
            StatusText.Text = "Загрузка данных...";
            var genres = await App.DB!.GetGenresAsync();
            GenreFilter.ItemsSource = genres;
            GenreFilter.Items.Insert(0, "Все");
            GenreFilter.SelectedIndex = 0;

            _allSessions = await App.DB!.GetSessionsForPosterAsync();
            SessionsList.ItemsSource = _allSessions;
            StatusText.Text = $"Найдено сеансов: {_allSessions.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private async Task ApplyFilters()
    {
        string? genre = GenreFilter.SelectedItem?.ToString();
        if (genre == "Все") genre = null;

        DateTime? date = DateFilter.SelectedDate?.DateTime;
        string? search = string.IsNullOrWhiteSpace(SearchBox.Text) ? null : SearchBox.Text;

        string? sort = null;
        if (SortCombo.SelectedIndex == 1) sort = "price";
        else if (SortCombo.SelectedIndex == 2) sort = "time";

        StatusText.Text = "Применение фильтров...";
        try
        {
            var filtered = await App.DB!.GetSessionsForPosterAsync(genre, date, search, sort);
            SessionsList.ItemsSource = filtered;
            StatusText.Text = $"Найдено сеансов: {filtered.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка фильтрации: {ex.Message}";
        }
    }

    private async void OnBuyTicketClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int sessionId)
        {
            if (App.CurrentUser == null)
            {
                var error = new Window
                {
                    Title = "Ошибка",
                    Content = "Пользователь не авторизован",
                    Width = 300,
                    Height = 150
                };
                await error.ShowDialog(this);
                return;
            }

            var seatSelectionWindow = new SeatSelectionWindow(sessionId, App.CurrentUser.UserId);
            await seatSelectionWindow.ShowDialog(this);
            await ApplyFilters();
        }
    }
}