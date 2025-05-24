using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopDomain;
using ShopDomain.Model;
using Microsoft.AspNetCore.Identity;

namespace ShopInfrastructure.Controllers
{
    public class CartsController : Controller
    {
        private readonly ShopDbContext _context;
        private readonly UserManager<User> _userManager;

        public CartsController(ShopDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        private async Task<Cart> GetOrCreateCartForCurrentUserAsync()
        {
            string? userId = User.Identity?.IsAuthenticated == true ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            if (userId != null)
            {
                var existingCart = await _context.Carts
                    .Include(c => c.ProductCarts)
                    .ThenInclude(pc => pc.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (existingCart != null)
                    return existingCart;

                var newCart = new Cart
                {
                    UserId = userId,
                    CtPrice = 0,
                    CtQuantity = 0
                };

                _context.Carts.Add(newCart);
                await _context.SaveChangesAsync();

                return newCart;
            }

            // Anonymous user - work with SessionId
            const string sessionKey = "CartSessionId";
            string? sessionId = HttpContext.Session.GetString(sessionKey);

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString(sessionKey, sessionId);
                await HttpContext.Session.CommitAsync();
            }

            var sessionCart = await _context.Carts
                .Include(c => c.ProductCarts)
                .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (sessionCart != null)
                return sessionCart;

            var newSessionCart = new Cart
            {
                SessionId = sessionId,
                UserId = null,  // Explicitly set to null for anonymous users
                CtPrice = 0,
                CtQuantity = 0
            };

            _context.Carts.Add(newSessionCart);
            await _context.SaveChangesAsync();

            return newSessionCart;
        }





        [HttpPost]
        public async Task<IActionResult> RemoveProduct(int productId)
        {
            //var cartId = HttpContext.Session.GetInt32("CartId");
            var cart = await GetOrCreateCartForCurrentUserAsync();

            if (cart == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var productCart = await _context.ProductCarts
                .FirstOrDefaultAsync(pc => pc.CartId == cart.Id && pc.ProductId == productId);

            if (productCart == null)
            {
                Console.WriteLine($"Product with ID {productId} not found in cart {cart}");
            }
            else
            {
                Console.WriteLine($"Found product {productCart.ProductId} in cart {productCart.CartId}. Removing...");
                _context.ProductCarts.Remove(productCart);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var product = await _context.Products
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            if (product.PdQuantity < quantity)
            {
                ModelState.AddModelError("Quantity", "Недостатня кількість товару на складі!");
                return View("~/Views/Products/Details.cshtml", product);
            }

            // ✅ Отримуємо або створюємо кошик (для залогіненого чи гостьового користувача)
            var cart = await GetOrCreateCartForCurrentUserAsync();

            var productCart = cart.ProductCarts.FirstOrDefault(pc => pc.ProductId == productId);
            if (productCart == null)
            {
                productCart = new ProductCart
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    PcQuantity = quantity,
                    PcPrice = product.PdPrice * quantity
                };
                _context.ProductCarts.Add(productCart);
            }
            else
            {
                if (product.PdQuantity < productCart.PcQuantity + quantity)
                {
                    ModelState.AddModelError("Quantity", "Недостатньо товару!");
                    return View("~/Views/Products/Details.cshtml", product);
                }

                productCart.PcQuantity += quantity;
                productCart.PcPrice += product.PdPrice * quantity;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Carts");
        }





        [HttpGet]
        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCartForCurrentUserAsync();
            //     _context.Carts
            //     .Include(c => c.ProductCarts)
            //     .ThenInclude(pc => pc.Product)
            //     .FirstOrDefaultAsync(); // Поки без користувачів, просто беремо перший кошик
            //
            // if (cart == null)
            // {
            //     cart = new Cart();
            //     _context.Carts.Add(cart);
            //     await _context.SaveChangesAsync();
            // }

            // Передаємо компанії доставки у ViewBag
            ViewBag.ShippingCompanies = await _context.ShippingCompanies.ToListAsync();

            return View(cart);
        }

        // GET: Carts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToAction("Index", "Categories");

            var cart = await _context.Carts
                .Include(c => c.ProductCarts)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UrNickname");
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CtQuantity,CtPrice,UserId,Id")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UrNickname", cart.UserId);
            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UrNickname", cart.UserId);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CtQuantity,CtPrice,UserId,Id")] Cart cart)
        {
            if (id != cart.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UrNickname", cart.UserId);
            return View(cart);
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}
