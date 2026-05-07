using System;

namespace rabota.ViewModels;

public class SessionViewModel
{
    public int SessionId { get; set; }
    public string FilmTitle { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public int Duration { get; set; }
    public DateTime StartTime { get; set; }
    public string HallName { get; set; } = null!;
    public decimal Price { get; set; }

    public string FormattedTime => StartTime.ToString("dd.MM.yyyy HH:mm");
    public string DurationStr => $"{Duration} мин";
}