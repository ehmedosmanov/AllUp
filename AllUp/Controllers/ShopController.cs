using AllUp.DAL;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _db;
        public ShopController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> CategoryDetails(int catId)
        {
            Category? category = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync(x => x.Id == catId);
            if (category == null)
            {
                return NotFound();
            }

            ViewBag.CategoryName = category.Name;
            
            return View(category);
        }
    }
}
