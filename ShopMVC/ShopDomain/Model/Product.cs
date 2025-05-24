using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;

namespace ShopDomain.Model;

public partial class Product : Entity
{
    //public int PdId { get; set; }

    [Display (Name = "Продукт")]
    [Required(ErrorMessage = "Поле не повино бути пустим")]
    public string? PdName { get; set; }
    
    [Display (Name = "Ціна")]
    public decimal? PdPrice { get; set; }

    [Display (Name = "Розмірність")]
    public string? PdMeasurements { get; set; }

    [Display (Name = "Кількість")]
    public int? PdQuantity { get; set; }
    
    [Display (Name = "Знижка")]
    public string? PdDiscount { get; set; }

    [Display (Name = "Опис")]
    public string? PdAbout { get; set; }

    public string? PdImagePath { get; set; }

    [Display(Name="Виробник")]
    public int? ManufacturerId { get; set; }
    [Display(Name="Виробник")]
    public virtual Manufacturer? Manufacturer { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductCart> ProductCarts { get; set; } = new List<ProductCart>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public virtual ICollection<ProductOrder> ProductOrders { get; set; } = new List<ProductOrder>();
}
