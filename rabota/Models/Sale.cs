using System;
using System.Collections.Generic;

namespace rabota.Models;

public partial class Sale
{
    public int SaleId { get; set; }

    public int TicketId { get; set; }

    public int UserId { get; set; }

    public DateTime? SaleTime { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
