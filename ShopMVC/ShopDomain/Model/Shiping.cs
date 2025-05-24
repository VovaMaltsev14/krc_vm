using System;
using System.Collections.Generic;

namespace ShopDomain.Model;

public partial class Shiping : Entity
{
    //public int ShId { get; set; }

    public string? ShAdress { get; set; }

    public int? CountryId { get; set; }

    public int? ShippingCompanyId { get; set; }

    public string? ShTrackingNumber { get; set; }

    public string? ShStatus { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    public virtual ShippingCompany? ShippingCompany { get; set; }
}
