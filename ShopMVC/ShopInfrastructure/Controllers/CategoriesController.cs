using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopDomain;
using ShopDomain.Model;
using ShopInfrastructure;
using ShopInfrastructure.Services;


namespace ShopInfrastructure.Controllers
{
    
    public class CategoriesController : Controller
    {
        private readonly ShopDbContext _context;

        private readonly IDataPortServiceFactory<Category> _categoryDataPortServiceFactory;

        
        public CategoriesController(
            ShopDbContext context,
            IDataPortServiceFactory<Category> categoryDataPortServiceFactory)
        {
            _context = context;
            _categoryDataPortServiceFactory = categoryDataPortServiceFactory;
        }


        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Import(IFormFile fileExcel, CancellationToken cancellationToken = default)
        {
            var importService = _categoryDataPortServiceFactory.GetImportService(fileExcel.ContentType);
            
            using var stream = fileExcel.OpenReadStream();
            
            await importService.ImportFromStreamAsync(stream, cancellationToken);
            
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Export([FromQuery] string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            CancellationToken cancellationToken = default)
        {
            var exportService = _categoryDataPortServiceFactory.GetExportService(contentType);

            var memoryStream = new MemoryStream();

            await exportService.WriteToAsync(memoryStream, cancellationToken);

            await memoryStream.FlushAsync(cancellationToken);
            memoryStream.Position = 0;


            return new FileStreamResult(memoryStream, contentType)
            {
                FileDownloadName = $"categiries_{DateTime.UtcNow.ToShortDateString()}.xlsx"
            };
        }

        
        // GET: Categories
        public async Task<IActionResult> Index()
        {
            // Отримуємо всі категорії
            var categories = await _context.Categories.ToListAsync();

            // Формуємо словник, де ключ — головна категорія, а значення — список підкатегорій
            var categoryDictionary = categories
                .Where(c => string.IsNullOrEmpty(c.CgParentCategory)) // Отримуємо лише головні категорії
                .ToDictionary(
                    mainCategory => mainCategory, // Ключ — головна категорія
                    mainCategory => categories    // Підкатегорії
                        .Where(subCategory => subCategory.CgParentCategory == mainCategory.CgName)
                        .ToList() // Значення — список підкатегорій
                );

            // Передаємо словник у вигляді моделі
            return View(categoryDictionary);
        }



        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Products", new { id = category.Id, name = category.CgName });
        }
        
        [Authorize(Roles = "admin")]
        // GET: Categories/Create
        public IActionResult Create()
        {
            ViewBag.ParentCategories = _context.Categories
                .Where(c => string.IsNullOrEmpty(c.CgParentCategory)) // Вибираємо тільки головні категорії
                .ToList();
    
            return View();
        }

        [Authorize(Roles = "admin")]
        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, int? parentCategoryId)
        {
            if (ModelState.IsValid)
            {
                if (parentCategoryId.HasValue)
                {
                    var parentCategory = await _context.Categories.FindAsync(parentCategoryId.Value);
                    if (parentCategory != null)
                    {
                        category.CgParentCategory = parentCategory.CgName; // Призначаємо головну категорію
                    }
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Якщо валідація не пройшла, повертаємо список головних категорій
            ViewBag.ParentCategories = _context.Categories
                .Where(c => string.IsNullOrEmpty(c.CgParentCategory))
                .ToList();

            return View(category);
        }

        [Authorize(Roles = "admin")]
        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        
        [Authorize(Roles = "admin")]
        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CgName,CgParentCategory, CgImage, CgDescription")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
