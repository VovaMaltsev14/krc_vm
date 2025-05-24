using System;
using System.Collections.Generic;

namespace ShopDomain.Model;

public partial class Receipt : Entity
{
    //public int RpId { get; set; }

    public DateTime? RpDateCreated { get; set; }

    public int? RpQuantity { get; set; }

    public decimal? RpTotal { get; set; }

    public string? RpPayment { get; set; }

    public string? RpDiscount { get; set; }

    public string? RpAbout { get; set; }

    public int? ShippingId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Shiping? Shipping { get; set; }
}
