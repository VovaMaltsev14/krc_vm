using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ShopDomain.Model;
using ShopInfrastructure;

namespace ShopInfrastructure.Services;

public class CategoryImportService : IImportService<Category>
{
    private readonly ShopDbContext _context;

    public CategoryImportService(ShopDbContext context)
    {
        _context = context;
    }

    public async Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanRead)
            throw new ArgumentException("Неможливо прочитати вхідний потік", nameof(stream));

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        foreach (var row in worksheet.RowsUsed().Skip(1)) // Пропускаємо заголовок
        {
            await AddProductAsync(row, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task AddProductAsync(IXLRow row, CancellationToken cancellationToken)
    {
        var productName = row.Cell(1).GetString().Trim();
        var categoryName = row.Cell(2).GetString();
        var subcategoryName = row.Cell(3).GetString();
        var measurement = row.Cell(4).GetString().Trim();
        var priceString = row.Cell(5).GetString().Replace("грн", "").Trim();
        var quantity = int.TryParse(row.Cell(6).GetString(), out var q) ? q : 0;
        var about = row.Cell(7).GetString().Trim();
        
        string discount;
        var rawDiscount = row.Cell(8).Value.ToString().Trim();
        if (double.TryParse(rawDiscount, out var parsedDouble) && parsedDouble > 0 && parsedDouble < 1)
        {
            discount = (parsedDouble * 100).ToString("0") + "%";
        }
        else
        {
            discount = rawDiscount;
        }

        
        var manufacturerName = row.Cell(9).GetString().Trim();

        // Отримати або створити виробника
        var manufacturer = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.MnName == manufacturerName, cancellationToken);

        if (manufacturer == null)
        {
            manufacturer = new Manufacturer { MnName = manufacturerName };
            _context.Manufacturers.Add(manufacturer);
        }

        // Отримати або створити батьківську категорію
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CgName == categoryName && c.ParentCategoryId == null, cancellationToken);

        if (category == null)
        {
            category = new Category
            {
                CgName = categoryName,
                CgDescription = "Імпортовано з Excel",
            };
            _context.Categories.Add(category);
    
            // Зберегти, щоб отримати коректний Id
            await _context.SaveChangesAsync(cancellationToken);
        }


        // Отримати або створити підкатегорію
        var subCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.CgName == subcategoryName && c.ParentCategoryId == category.Id, cancellationToken);

        if (subCategory == null)
        {
            subCategory = new Category
            {
                CgName = subcategoryName,
                CgParentCategory = category.CgName,
                ParentCategoryId = category.Id,
                CgDescription = "Імпортовано з Excel (підкатегорія)"
            };
            _context.Categories.Add(subCategory);
        }
        
        await AddOrLinkProductAsync(productName, measurement, priceString, quantity, about, discount, manufacturer, subCategory, cancellationToken);
    }

    private async Task AddOrLinkProductAsync(string productName, string measurement, string priceString,
        int quantity, string about, string discount, Manufacturer manufacturer, Category subCategory,
        CancellationToken cancellationToken)
    {
        // Перевірка, чи існує вже продукт з такою назвою, прив'язаний до підкатегорії
        var existingProduct = await _context.Products
            .Include(p => p.ProductCategories)
            .FirstOrDefaultAsync(p =>
                    p.PdName == productName &&
                    p.ProductCategories.Any(pc => pc.CategoryId == subCategory.Id),
                cancellationToken);

        if (existingProduct != null)
        {
            // Продукт з таким ім’ям вже прив’язаний — нічого не робимо
            return;
        }

        // Перевірка: продукт є, але не прив’язаний — створюємо лише зв'язок
        existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.PdName == productName, cancellationToken);

        if (existingProduct != null)
        {
            _context.ProductCategories.Add(new ProductCategory
            {
                Product = existingProduct,
                Category = subCategory
            });
            return;
        }

        // Продукту ще нема — створюємо і додаємо
        var product = new Product
        {
            PdName = productName,
            PdMeasurements = measurement,
            PdPrice = decimal.TryParse(priceString, out var price) ? price : null,
            PdQuantity = quantity,
            PdAbout = about,
            PdDiscount = discount,
            Manufacturer = manufacturer
        };
        _context.Products.Add(product);

        _context.ProductCategories.Add(new ProductCategory
        {
            Product = product,
            Category = subCategory
        });
    }

}
