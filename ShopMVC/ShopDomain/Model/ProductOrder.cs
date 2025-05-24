using System;
using System.Collections.Generic;

namespace ShopDomain.Model;

public partial class ProductOrder : Entity
{
    //public int PoId { get; set; }

    public int? ProductId { get; set; }

    public int? OrderId { get; set; }

    public decimal? PoPrice { get; set; }

    public int? PoQuantity { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
