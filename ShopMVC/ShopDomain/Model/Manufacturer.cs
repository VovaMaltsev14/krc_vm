using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDomain.Model;

public partial class Manufacturer : Entity
{
    //public int MnId { get; set; }

    [Display (Name = "Виробник")]
    public string? MnName { get; set; }
    
    [Display (Name = "Контактна інформація")]
    public string? MnContactInfo { get; set; }

    [Display (Name = "Країна")]
    public int? CountryId { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
