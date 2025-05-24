using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ShopDomain.Model;
using ShopInfrastructure;

namespace ShopInfrastructure.Services;

public class CategoryExportService : IExportService<Category>
{
    private static readonly IReadOnlyList<string> HeaderNames = new[]
    {
        "Назва товару",
        "Категорія",
        "Підкатегорія",
        "Розмірність",
        "Ціна",
        "К-сть",
        "Опис",
        "Знижка",
        "Виробник"
    };

    private readonly ShopDbContext _context;

    public CategoryExportService(ShopDbContext context)
    {
        _context = context;
    }

    private static void WriteHeader(IXLWorksheet worksheet)
    {
        for (int columnIndex = 0; columnIndex < HeaderNames.Count; columnIndex++)
        {
            worksheet.Cell(1, columnIndex + 1).Value = HeaderNames[columnIndex];
        }
        worksheet.Row(1).Style.Font.Bold = true;
    }

    private void WriteProduct(IXLWorksheet worksheet, Product product, int rowIndex)
    {
        var columnIndex = 1;
        var productCategory = product.ProductCategories.FirstOrDefault()?.Category;

        worksheet.Cell(rowIndex, columnIndex++).Value = product.PdName;
        worksheet.Cell(rowIndex, columnIndex++).Value = productCategory?.ParentCategory?.CgName ?? "";
        worksheet.Cell(rowIndex, columnIndex++).Value = productCategory?.CgName ?? "";
        worksheet.Cell(rowIndex, columnIndex++).Value = product.PdMeasurements ?? "";
        worksheet.Cell(rowIndex, columnIndex++).Value = product.PdPrice?.ToString("0.00") + " грн";
        worksheet.Cell(rowIndex, columnIndex++).Value = product.PdQuantity;
        worksheet.Cell(rowIndex, columnIndex++).Value = product.PdAbout ?? "";
        worksheet.Cell(rowIndex, columnIndex++).Value = product.PdDiscount ?? "";
        worksheet.Cell(rowIndex, columnIndex++).Value = product.Manufacturer?.MnName ?? "";
    }

    private void WriteProducts(IXLWorksheet worksheet, List<Product> products)
    {
        WriteHeader(worksheet);
        int rowIndex = 2;

        foreach (var product in products)
        {
            WriteProduct(worksheet, product, rowIndex++);
        }
    }

    public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanWrite)
        {
            throw new ArgumentException("Stream is not writable", nameof(stream));
        }

        var products = await _context.Products
            .Include(p => p.Manufacturer)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                    .ThenInclude(c => c.ParentCategory)
            .ToListAsync(cancellationToken);

        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Товари");

        WriteProducts(worksheet, products);
        workbook.SaveAs(stream);
    }
}
