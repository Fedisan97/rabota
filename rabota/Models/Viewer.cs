using System;
using System.Collections.Generic;

namespace rabota.Models;

public partial class Viewer
{
    public int ViewerId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
