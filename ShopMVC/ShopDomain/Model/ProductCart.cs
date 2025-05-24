using System;
using System.Collections.Generic;

namespace ShopDomain.Model;

public partial class ProductCart : Entity
{
    //public int PcId { get; set; }

    public int? ProductId { get; set; }

    public int? CartId { get; set; }

    public int? PcQuantity { get; set; }

    public decimal? PcPrice { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Product? Product { get; set; }
}
