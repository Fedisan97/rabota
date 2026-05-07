using System;
using System.Collections.Generic;

namespace rabota.Models;

public partial class Hall
{
    public int HallId { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
