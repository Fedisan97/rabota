using System;
using System.Collections.Generic;

namespace rabota.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int SessionId { get; set; }

    public int? ViewerId { get; set; }

    public int SeatRow { get; set; }

    public int SeatNumber { get; set; }

    public decimal Price { get; set; }

    public bool? IsSold { get; set; }

    public string? QrCode { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual Session Session { get; set; } = null!;

    public virtual Viewer? Viewer { get; set; }
}
