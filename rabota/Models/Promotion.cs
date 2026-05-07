using System;
using System.Collections.Generic;

namespace rabota.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string Name { get; set; } = null!;

    public decimal DiscountPercent { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public virtual ICollection<Film> Films { get; set; } = new List<Film>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
