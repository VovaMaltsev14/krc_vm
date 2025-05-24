using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDomain.Model;

public partial class Country : Entity
{
    //virtual public int Id { get; set; }

    [Display (Name = "Країна")]
    public string? CoName { get; set; }

    public virtual ICollection<Manufacturer> Manufacturers { get; set; } = new List<Manufacturer>();

    public virtual ICollection<Shiping> Shipings { get; set; } = new List<Shiping>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
