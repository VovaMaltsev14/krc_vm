using System;
using System.Collections.Generic;

namespace ShopDomain.Model;

public partial class ProductCategory : Entity
{
    //public int PctId { get; set; }

    public int? ProductId { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Product? Product { get; set; }
}
