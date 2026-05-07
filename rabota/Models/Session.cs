using System;
using System.Collections.Generic;

namespace rabota.Models;

public partial class Session
{
    public int SessionId { get; set; }

    public int FilmId { get; set; }

    public int HallId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public virtual Film Film { get; set; } = null!;

    public virtual Hall Hall { get; set; } = null!;

    public virtual ICollection<Price> Prices { get; set; } = new List<Price>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
