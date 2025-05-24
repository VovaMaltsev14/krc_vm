using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopDomain;
using ShopDomain.Model;

namespace ShopInfrastructure.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ShopDbContext _context;

        public ProductsController(ShopDbContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Index");

            var results = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.PdName.Contains(query) || p.PdAbout.Contains(query))
                .ToListAsync();

            ViewBag.Query = query;
            return View(results);
        }


        
        // GET: Products
        // GET: Products
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null) return RedirectToAction("Index", "Categories");
            ViewBag.CategoryId = id;
            ViewBag.CategoryName = name;
            var productByCategory = _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .Include(pc => pc.Manufacturer)
                .Where(b => b.ProductCategories.Any(pc => pc.CategoryId == id));
            
            return View(await productByCategory.ToListAsync());
            
            
            // var shopDbContext = _context.Products.Include(p => p.Manufacturer);
            // return View(await shopDbContext.ToListAsync());
        }


        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            
            //
            var category = product.ProductCategories
                .Select(pc => pc.Category)
                .FirstOrDefault();

            ViewBag.CategoryId = category?.Id;
            ViewBag.CategoryName = category?.CgName;
            
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create(int categoryId)
        {
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = _context.Categories.Where(c => c.Id == categoryId).FirstOrDefault().CgName;
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "Id", "MnName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int categoryId, [Bind("PdName, PdPrice, PdMeasurements, PdQuantity, PdDiscount, PdAbout, ManufacturerId, Id")] Product product)
        { 
           //Category category = _context.Categories.Include(c=>c.ProductCategories).FirstOrDefault(c => c.Id == product.Id);
           //ProductCategory productCategory = _context.ProductCategories.Include(c=>c.CategoryId).FirstOrDefault(c => c.Id == product.Id);
           //product.ProductCategories = productCategory;
           
           product.ProductCategories.Add(new ProductCategory { CategoryId = categoryId });
           if (ModelState.IsValid)
           {
               _context.Add(product);
               await _context.SaveChangesAsync();
               return RedirectToAction(nameof(Index), new { id = categoryId, name = _context.Categories.Where(c => c.Id == categoryId).FirstOrDefault().CgName });
           }
           ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "Id", "MnName", product.ManufacturerId);
           return RedirectToAction(nameof(Index), new { id = categoryId, name = _context.Categories.Where(c => c.Id == categoryId).FirstOrDefault().CgName });
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "Id", "MnName", product.ManufacturerId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PdName,PdPrice,PdMeasurements,PdQuantity,PdDiscount,PdAbout,ManufacturerId,Id")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "Id", "MnName", product.ManufacturerId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Отримуємо всі замовлення, що містять цей продукт
                var affectedOrders = _context.Orders
                    .Where(o => o.ProductOrders.Any(po => po.ProductId == id))
                    .ToList();

                // Отримуємо всі чеки, що пов'язані з цими замовленнями
                var affectedReceiptIds = affectedOrders.Select(o => o.ReceiptId).Distinct().ToList();
                var affectedReceipts = _context.Receipts
                    .Where(r => affectedReceiptIds.Contains(r.Id))
                    .ToList();

                // Видаляємо всі залежні записи з ProductOrders
                var productOrders = _context.ProductOrders.Where(po => po.ProductId == id);
                _context.ProductOrders.RemoveRange(productOrders);

                // Видаляємо всі залежні записи з ProductCarts
                var productCarts = _context.ProductCarts.Where(pc => pc.ProductId == id);
                _context.ProductCarts.RemoveRange(productCarts);

                // Видаляємо всі залежні записи з ProductCategories (якщо це потрібно)
                var productCategories = _context.ProductCategories.Where(pc => pc.ProductId == id);
                _context.ProductCategories.RemoveRange(productCategories);

                await _context.SaveChangesAsync(); // Спочатку зберігаємо зміни, щоб видалити всі зв'язки

                // Оновлюємо всі замовлення, які містили цей продукт
                foreach (var order in affectedOrders)
                {
                    var remainingProducts = _context.ProductOrders
                        .Where(po => po.OrderId == order.Id)
                        .ToList();

                    order.OdTotal = remainingProducts.Sum(po => (po.PoPrice ?? 0) );

                    // Якщо в замовленні більше немає товарів, видаляємо його
                    if (!remainingProducts.Any())
                    {
                        _context.Orders.Remove(order);
                    }
                }

                await _context.SaveChangesAsync();

                // Оновлюємо чеки (Receipt), пов’язані із зміненими замовленнями
                foreach (var receipt in affectedReceipts)
                {
                    var relatedOrders = _context.Orders.Where(o => o.ReceiptId == receipt.Id).ToList();

                    if (!relatedOrders.Any())
                    {
                        _context.Receipts.Remove(receipt); // Видаляємо чек, якщо всі його замовлення зникли
                    }
                    else
                    {
                        // Загальна кількість товарів у чеку — сума всіх товарів у замовленнях
                        receipt.RpQuantity = relatedOrders
                            .Sum(o => _context.ProductOrders
                                .Where(po => po.OrderId == o.Id)
                                .Sum(po => po.PoQuantity ?? 0));

                        receipt.RpTotal = relatedOrders.Sum(o => o.OdTotal ?? 0);
                    }
                }

                await _context.SaveChangesAsync();

                // Нарешті видаляємо сам продукт
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        
        
    }
}
