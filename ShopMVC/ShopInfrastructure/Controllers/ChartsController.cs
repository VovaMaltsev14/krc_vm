using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInfrastructure;

namespace ShopMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private record ProductPriceHistoryResponseItem(DateTime? OrderDate, decimal? PricePerUnit, int? QuantitySold);

        private readonly ShopDbContext _context;

        public ChartsController(ShopDbContext context)
        {
            _context = context;
        }

        [HttpGet("productPriceHistory/{productId}")]
        public async Task<JsonResult> GetProductPriceHistory(int productId, CancellationToken cancellationToken)
        {
            var rawData = await _context.ProductOrders
                .Where(po => po.ProductId == productId && po.PoQuantity > 0 && po.Order != null)
                .Include(po => po.Order)
                .ThenInclude(o => o.Receipt)
                .Select(po => new
                {
                    OrderDate = (po.Order!.Receipt!.RpDateCreated ?? po.Order.CreatedAt ?? DateTime.UtcNow).Date,
                    PricePerUnit = (po.PoPrice/* / (decimal?)po.PoQuantity*/) * (1 - ParseDiscount(po.Product.PdDiscount ?? "0%")),
                    Quantity = po.PoQuantity
                })
                .ToListAsync(cancellationToken);

            var responseItems = rawData
                .GroupBy(po => new { po.OrderDate, po.PricePerUnit })
                .Select(group => new ProductPriceHistoryResponseItem(
                    group.Key.OrderDate,
                    group.Key.PricePerUnit,
                    group.Sum(po => po.Quantity)
                ))
                .OrderBy(item => item.OrderDate)
                .ToList();

            return new JsonResult(responseItems);
        }




        private static decimal ParseDiscount(string discount)
        {
            if (string.IsNullOrWhiteSpace(discount)) return 0;
            if (discount.EndsWith("%")) discount = discount.TrimEnd('%');

            return decimal.TryParse(discount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedDiscount)
                ? parsedDiscount / 100
                : 0;
        }
    }
}
