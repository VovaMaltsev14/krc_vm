using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDomain.Model;

public partial class Category : Entity
{
    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Категорія")]
    public string? CgName { get; set; }

    [Display(Name = "Опис")]
    public string? CgDescription { get; set; }

    public string? CgParentCategory { get; set; }
    
    public int? ParentCategoryId { get; set; } // Додаємо зв'язок із батьківською категорією

    public string? CgImage { get; set; }
    
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}
