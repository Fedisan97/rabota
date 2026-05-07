using System;
using System.Collections.Generic;

namespace rabota.Models;

public partial class Price
{
    public int PriceId { get; set; }

    public int SessionId { get; set; }

    public string SeatType { get; set; } = null!;

    public decimal PriceValue { get; set; }

    public virtual Session Session { get; set; } = null!;
}
