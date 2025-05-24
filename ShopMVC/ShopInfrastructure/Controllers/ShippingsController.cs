using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopDomain;
using ShopDomain.Model;

namespace ShopInfrastructure.Controllers
{
    public class ShippingsController : Controller
    {
        private readonly ShopDbContext _context;

        public ShippingsController(ShopDbContext context)
        {
            _context = context;
        }

        // GET: Shippings
        public async Task<IActionResult> Index()
        {
            var shopDbContext = _context.Shipings.Include(s => s.Country).Include(s => s.ShippingCompany);
            return View(await shopDbContext.ToListAsync());
        }

        // GET: Shippings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shiping = await _context.Shipings
                .Include(s => s.Country)
                .Include(s => s.ShippingCompany)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shiping == null)
            {
                return NotFound();
            }

            return View(shiping);
        }

        // GET: Shippings/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Id");
            ViewData["ShippingCompanyId"] = new SelectList(_context.ShippingCompanies, "Id", "Id");
            return View();
        }

        // POST: Shippings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShAdress,CountryId,ShippingCompanyId,Id")] Shiping shiping)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shiping);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Id", shiping.CountryId);
            ViewData["ShippingCompanyId"] = new SelectList(_context.ShippingCompanies, "Id", "Id", shiping.ShippingCompanyId);
            return View(shiping);
        }

        // GET: Shippings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shiping = await _context.Shipings.FindAsync(id);
            if (shiping == null)
            {
                return NotFound();
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Id", shiping.CountryId);
            ViewData["ShippingCompanyId"] = new SelectList(_context.ShippingCompanies, "Id", "Id", shiping.ShippingCompanyId);
            return View(shiping);
        }

        // POST: Shippings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ShAdress,CountryId,ShippingCompanyId,Id")] Shiping shiping)
        {
            if (id != shiping.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shiping);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShipingExists(shiping.Id))
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
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Id", shiping.CountryId);
            ViewData["ShippingCompanyId"] = new SelectList(_context.ShippingCompanies, "Id", "Id", shiping.ShippingCompanyId);
            return View(shiping);
        }

        // GET: Shippings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shiping = await _context.Shipings
                .Include(s => s.Country)
                .Include(s => s.ShippingCompany)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shiping == null)
            {
                return NotFound();
            }

            return View(shiping);
        }

        // POST: Shippings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shiping = await _context.Shipings.FindAsync(id);
            if (shiping != null)
            {
                _context.Shipings.Remove(shiping);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShipingExists(int id)
        {
            return _context.Shipings.Any(e => e.Id == id);
        }
    }
}
