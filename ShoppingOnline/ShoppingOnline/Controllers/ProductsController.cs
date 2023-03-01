using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingOnline.Data;
using ShoppingOnline.Models;

namespace ShoppingOnline.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager)); ;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Genre);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [Authorize(Roles = "Admin")]
        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Set<Genre>(), "Id", "Name");
            return View();
        }

        // POST: Products/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Quantity,ImageUrl,GenreId")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenreId"] = new SelectList(_context.Set<Genre>(), "Id", "Name", product.GenreId);
            return View(product);
        }

		[Authorize(Roles = "Admin")]
		// GET: Products/Edit/5
		public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["GenreId"] = new SelectList(_context.Set<Genre>(), "Id", "Name", product.GenreId);
            return View(product);
        }

        // POST: Products/Edit/5        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Quantity,ImageUrl,GenreId")] Product product)
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
            ViewData["GenreId"] = new SelectList(_context.Set<Genre>(), "Id", "Name", product.GenreId);
            return View(product);
        }

		[Authorize(Roles = "Admin")]
		// GET: Products/Delete/5
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Genre)
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
            if (_context.Products == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Product'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return _context.Products.Any(e => e.Id == id);
        }

        public Product getDetailProduct(int id)
        {
            var product = _context.Products.Find(id);
            return product;
        }

        [Authorize]
        //ADD CART
        public IActionResult addCart(int id)
        {
            var cart = HttpContext.Session.GetString("cart");//get key cart
            if (cart == null)
            {
                var product = getDetailProduct(id);
                List<Cart> listCart = new List<Cart>()
               {
                   new Cart
                   {
                       Product = product,
                       Quantity = 1
                   }
               };
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(listCart));

            }
            else
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                bool check = true;
                for (int i = 0; i < dataCart.Count; i++)
                {
                    if (dataCart[i].Product.Id == id)
                    {
                        dataCart[i].Quantity++;
                        check = false;
                    }
                }
                if (check)
                {
                    dataCart.Add(new Cart
                    {
                        Product = getDetailProduct(id),
                        Quantity = 1
                    });
                }
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
                // var cart2 = HttpContext.Session.GetString("cart");//get key cart
                //  return Json(cart2);
            }

            return RedirectToAction(nameof(ListCart));

        }

        public IActionResult ListCart()
        {
            var cart = HttpContext.Session.GetString("cart");//get key cart
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                if (dataCart.Count > 0)
                {
                    ViewBag.carts = dataCart;
                    return View();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult deleteCart(int id)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);

                for (int i = 0; i < dataCart.Count; i++)
                {
                    if (dataCart[i].Product.Id == id)
                    {
                        dataCart.RemoveAt(i);
                    }
                }
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
                return RedirectToAction(nameof(ListCart));
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult increaseQuantity(int id)
        {
            var cart = HttpContext.Session.GetString("cart");//get key cart
            List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
            for (int i = 0; i < dataCart.Count; i++)
            {
                if (dataCart[i].Product.Id == id)
                {
                    dataCart[i].Quantity++;
                }
            }
            HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
            return RedirectToAction(nameof(ListCart));
        }

        public IActionResult decreaseQuantity(int id)
        {
            var cart = HttpContext.Session.GetString("cart");//get key cart
            List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
            for (int i = 0; i < dataCart.Count; i++)
            {
                if (dataCart[i].Product.Id == id)
                {
                    dataCart[i].Quantity--;
                    if (dataCart[i].Quantity <= 0) // remove item if quantity is zero
                    {
                        dataCart.RemoveAt(i);
                    }
                }
            }
            HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
            return RedirectToAction(nameof(ListCart));
        }

        public async Task<ActionResult> CheckOut()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("./Login");
            }
            if (ModelState.IsValid)
            {
                var cart = HttpContext.Session.GetString("cart");
                if (cart != null)
                {
                    List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                    for (int i = 0; i < dataCart.Count; i++)
                    {
                        Order order = new Order()
                        {
                            UserId = _userManager.GetUserId(User),
                            ProductId = dataCart[i].Product.Id,
                            Qty = dataCart[i].Quantity,
                            Price = Convert.ToDouble(dataCart[i].Quantity * dataCart[i].Product.Price),
                            OrderTime = DateTime.Now
                        };
                        _context.Orders.Add(order);
                        _context.SaveChanges();
                        deleteCart(dataCart[i].Product.Id);
                    }

                }
                return RedirectToAction(nameof(ThankYou));
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ThankYou()
        {
            return View();
        }

    }
}
