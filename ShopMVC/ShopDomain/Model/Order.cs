using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopDomain.Model;

public partial class Order : Entity
{
    //public int OdId { get; set; }

    public string? OdUser { get; set; }
    [Display (Name="Всього")]
    public decimal? OdTotal { get; set; }
    [Display (Name = "Знижка")]
    public double? OdDiscount { get; set; }
    [Display (Name = "Оплата")]
    public string? OdPayment { get; set; }
    [Display (Name = "Нотатки")]
    public string? OdNotes { get; set; }

    public int? ReceiptId { get; set; }

    public int? ProductId { get; set; }

    public int? ShippingId { get; set; }
    
    [Display(Name = "Дата створення")]
    public DateTime? CreatedAt => Receipt?.RpDateCreated;
    
    [Display (Name="Номер телефону користувача")]
    //[ForeignKey("OdUser")]
    public virtual User? OdUserNavigation { get; set; }

    public virtual ICollection<ProductOrder> ProductOrders { get; set; } = new List<ProductOrder>();
    [Display (Name="")]
    public virtual Product? Product { get; set; }
    [Display (Name="Чек")]
    public virtual Receipt? Receipt { get; set; }
    [Display (Name="Доставка")]
    public virtual Shiping? Shipping { get; set; }
}
