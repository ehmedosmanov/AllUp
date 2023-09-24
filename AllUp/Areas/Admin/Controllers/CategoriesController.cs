using AllUp.DAL;
using AllUp.Helpers;
using AllUp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public CategoriesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _db.Categories.Include(x => x.Parent).Include(x => x.Children).ToListAsync();
            return View(categories);
        }

        #region Create
        public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, int? mainCatId)
        {

            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();


            if (category.IsMain)//parent
            {
                #region Exist
                bool isExist = await _db.Categories.AnyAsync(x => x.Name == category.Name);

                if (isExist)
                {
                    ModelState.AddModelError("Name", "this category alerady exist");
                    return View();
                }

                #endregion

                #region SaveImg
                if (category.Photo == null)
                {
                    ModelState.AddModelError("Photo", "Please Selecet a Photo");
                    return View();
                }
                if (!category.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please Upload a Photo");
                    return View();
                }
                if (category.Photo.IsOlder1MB())
                {
                    ModelState.AddModelError("Photo", "Max 1Mb");
                    return View();
                }

                string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                category.Img = await category.Photo.SaveFileAsync(folder);
                #endregion

            }
            else // child 
            {
                category.ParentId = mainCatId;
            }

            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        

        #region Update
        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();

            if (id == null)
            {
                return NotFound();
            }

            Category dbcategory = await _db.Categories.Where(x => x.Id == id).FirstOrDefaultAsync();


            if (dbcategory == null)
            {
                return BadRequest();
            }
            return View(dbcategory);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Category category, int? id, int? mainCatId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();

            if (id == null)
            {
                return NotFound();
            }

            Category? dbcategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (dbcategory.IsMain)
            {
                #region exist
                bool isexist = await _db.Categories.AnyAsync(x => x.Name == category.Name && dbcategory.Id != id);
                if (isexist)
                {

                    ModelState.AddModelError("name", "this service already exist");
                    return View(dbcategory);
                }
                #endregion

                #region SaveImg
                if (category.Photo != null)
                {
                    if (!category.Photo.IsImage())
                    {
                        ModelState.AddModelError("Photo", "Please Upload a Photo");
                        return View();
                    }
                    if (category.Photo.IsOlder1MB())
                    {
                        ModelState.AddModelError("Photo", "Max 1mb");
                        return View();
                    }


                    string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                    string fullpath = Path.Combine(folder, dbcategory.Img);
                    if (System.IO.File.Exists(fullpath))
                    {
                        System.IO.File.Delete(fullpath);
                    }
                    dbcategory.Img = await category.Photo.SaveFileAsync(folder);
                }
                #endregion

            }
            else
            {
                dbcategory.ParentId = mainCatId;
            }
            dbcategory.Name = category.Name;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        #endregion


        #region Activity

        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category? dbcat = await _db.Categories.Where(x => x.Id == id).Include(x => x.Children).FirstOrDefaultAsync();


            ToggleDeactive(dbcat);

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }



        private void ToggleDeactive(Category category)
        {
            category.IsDeactive = !category.IsDeactive;

            if (category.Children != null)
            {
                foreach (Category child in category.Children)
                {
                    ToggleDeactive(child);
                }
            }
        }

        #endregion


        #region Search
        public async Task<IActionResult> Search(string key)
        {
            List<Category> categories = await _db.Categories.Where(x => x.Name.Contains(key)).ToListAsync();
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return PartialView("_SearchCategoryPartial", categories);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category? category = await _db.Categories
                .Include(x => x.Parent)
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        #endregion
    }
}
