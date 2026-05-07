using Avalonia.Controls;
using Avalonia.Interactivity;
using rabota.Models;
using rabota.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rabota;

public partial class SeatSelectionWindow : Window
{
    private readonly int _sessionId;
    private readonly int _userId;
    private int _hallCapacity;
    private DateTime _sessionStartTime;
    private List<SeatModel> _seats = new();
    private SeatModel? _selectedSeat;
    private Viewer? _selectedViewer;
    private List<Viewer> _foundViewers = new();

    public SeatSelectionWindow(int sessionId, int userId)
    {
        InitializeComponent();
        _sessionId = sessionId;
        _userId = userId;

        Loaded += async (s, e) => await LoadSessionData();
        SearchViewerBtn.Click += OnSearchViewer;
        ViewerListBox.SelectionChanged += OnViewerSelected;
        CreateViewerBtn.Click += OnCreateViewer;
        ConfirmButton.Click += OnSellTicket;
        CancelButton.Click += (s, e) => Close();
    }

    private async Task LoadSessionData()
    {
        try
        {
            var info = await App.DB!.GetSessionInfoAsync(_sessionId);
            FilmTitleText.Text = info.filmTitle;
            SessionTimeText.Text = $"Время: {info.startTime:dd.MM.yyyy HH:mm}";
            HallNameText.Text = $"Зал: {info.hallName}";
            _hallCapacity = info.capacity;
            _sessionStartTime = info.startTime;
            await LoadSeats();
        }
        catch (Exception ex)
        {
            var error = new Window
            {
                Title = "Ошибка",
                Content = $"Не удалось загрузить сеанс: {ex.Message}",
                Width = 300,
                Height = 150
            };
            await error.ShowDialog(this);
            Close();
        }
    }

    private async Task LoadSeats()
    {
        var occupied = await App.DB!.GetOccupiedSeatsAsync(_sessionId);
        _seats.Clear();
        int rows = (int)Math.Ceiling(_hallCapacity / 10.0);
        int seatsPerRow = 10;
        for (int row = 1; row <= rows; row++)
        {
            for (int seatNum = 1; seatNum <= seatsPerRow; seatNum++)
            {
                if ((row - 1) * seatsPerRow + seatNum > _hallCapacity) break;
                bool occupiedFlag = occupied.Any(o => o.row == row && o.number == seatNum);
                decimal price = (row <= 2 && seatNum <= 5) ? 600 : 350;
                _seats.Add(new SeatModel
                {
                    Row = row,
                    SeatNumber = seatNum,
                    IsOccupied = occupiedFlag,
                    IsSelected = false,
                    Price = price
                });
            }
        }
        RefreshSeatsDisplay();
    }

    private void RefreshSeatsDisplay() => SeatsGrid.ItemsSource = _seats;

    private async void OnSearchViewer(object? sender, RoutedEventArgs e)
    {
        string search = SearchViewerBox.Text ?? "";
        if (string.IsNullOrWhiteSpace(search))
        {
            ViewerListBox.ItemsSource = null;
            return;
        }
        _foundViewers = await App.DB!.FindViewersAsync(search);
        ViewerListBox.ItemsSource = _foundViewers;
    }

    private void OnViewerSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selectedViewer = ViewerListBox.SelectedItem as Viewer;
        ConfirmButton.IsEnabled = _selectedViewer != null && _selectedSeat != null;
    }

    private async void OnCreateViewer(object? sender, RoutedEventArgs e)
    {
        string name = NewViewerNameBox.Text ?? "";
        string phone = NewViewerPhoneBox.Text ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            var error = new Window { Title = "Ошибка", Content = "Введите ФИО", Width = 300, Height = 150 };
            await error.ShowDialog(this);
            return;
        }
        int newId = await App.DB!.CreateViewerAsync(name, phone);
        _selectedViewer = new Viewer { ViewerId = newId, FullName = name, Phone = phone };
        _foundViewers.Insert(0, _selectedViewer);
        ViewerListBox.ItemsSource = _foundViewers;
        ViewerListBox.SelectedItem = _selectedViewer;
    }

    public void OnSeatClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is SeatModel seat)
        {
            if (seat.IsOccupied) return;
            if (_sessionStartTime < DateTime.Now)
            {
                _ = ShowError("Сеанс уже прошёл");
                return;
            }
            if (_selectedSeat != null) _selectedSeat.IsSelected = false;
            _selectedSeat = seat;
            _selectedSeat.IsSelected = true;

            SelectedInfo.Text = $"Ряд {seat.Row}, место {seat.SeatNumber}";
            PriceInfo.Text = $"Цена: {seat.Price} ₽";
            RefreshSeatsDisplay();
            ConfirmButton.IsEnabled = _selectedViewer != null;
        }
        else
        {
            _ = ShowError("Не удалось определить место");
        }
    }

    private async void OnSellTicket(object? sender, RoutedEventArgs e)
    {
        if (_selectedSeat == null || _selectedViewer == null)
        {
            await ShowError("Выберите место и зрителя");
            return;
        }
        if (_sessionStartTime < DateTime.Now)
        {
            await ShowError("Сеанс уже прошёл, продажа невозможна");
            return;
        }

        ConfirmButton.IsEnabled = false;
        var (success, error) = await App.DB!.SellTicketAsync(_sessionId, _selectedViewer.ViewerId,
            _selectedSeat.Row, _selectedSeat.SeatNumber, _selectedSeat.Price, _userId);
        if (success)
        {
            var successWin = new Window
            {
                Title = "Успех",
                Content = "Билет успешно продан!",
                Width = 250,
                Height = 120,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            await successWin.ShowDialog(this);
            _selectedSeat.IsOccupied = true;
            _selectedSeat.IsSelected = false;
            _selectedSeat = null;
            ConfirmButton.IsEnabled = false;
            SelectedInfo.Text = "";
            PriceInfo.Text = "";
            RefreshSeatsDisplay();
        }
        else
        {
            await ShowError(error);
            ConfirmButton.IsEnabled = true;
        }
    }

    private async Task ShowError(string message)
    {
        var error = new Window
        {
            Title = "Ошибка",
            Content = message,
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        await error.ShowDialog(this);
    }
}