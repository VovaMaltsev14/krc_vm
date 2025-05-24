using System;
using System.Collections.Generic;

namespace ShopDomain.Model;

public partial class ShippingCompany : Entity
{
    //public int ScId { get; set; }

    //  public virtual int Id { get; set; } 

    public string? ScName { get; set; }

    public decimal? ScPricing { get; set; }

    public string? ScAvgTimeNeed { get; set; }

    public string? ScContactInfo { get; set; }

    public virtual ICollection<Shiping> Shipings { get; set; } = new List<Shiping>();
}
