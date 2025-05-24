using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace ShopDomain.Model;

public class User : IdentityUser
{
    //public int UrId { get; set; }

    //public string? UrNickname { get; set; }

    public DateTime? UrBirthdate { get; set; }

    //public string? UrEmail { get; set; }

    public string? UrRole { get; set; }

    public int? UrCountryId { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Country? UrCountry { get; set; }
}

