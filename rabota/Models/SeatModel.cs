namespace rabota.Models;

public class SeatModel
{
    public int Row { get; set; }
    public int SeatNumber { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsSelected { get; set; }
    public decimal Price { get; set; }
}