using System;
using System.Collections.Generic;

namespace rabota.Models;

public partial class Film
{
    public int FilmId { get; set; }

    public string Title { get; set; } = null!;

    public int Duration { get; set; }

    public string? Genre { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
