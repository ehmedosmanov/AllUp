using AllUp.DAL;
using AllUp.Helpers;
using AllUp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class ProductController:Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = await _db.Products.
                Include(x => x.ProductImages).
                Include(x => x.ProductDetail).
                Include(x => x.Brand).
                Include(x => x.TagProducts).
                ThenInclude(x=> x.Tag).
                Include(x => x.ProductCategories).
                ThenInclude(x => x.Category)
                .ToListAsync();
            return View(products);
        }

        #region Create
         public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.Brands = await _db.Brands.ToListAsync();
            ViewBag.Tags = await _db.Tags.ToListAsync();
            Category? firstMainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync();
            ViewBag.ChildCategories = firstMainCategory?.Children;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, int brandId, List<int> tagsId, int mainCatId, int? childCatId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.Brands = await _db.Brands.ToListAsync();
            ViewBag.Tags = await _db.Tags.ToListAsync();
            Category? firstMainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync();
            ViewBag.ChildCategories = firstMainCategory?.Children;
            if(mainCatId == null)
            {

                return NotFound();
            }

            #region IsExist
            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name);
            if (isExist)
            {
                ModelState.AddModelError("Name", "this product is alerady exist");
                return View();
            }
            #endregion


            #region Save Images
            if (product.Photos == null)
            {
                ModelState.AddModelError("Photo", "Please select file");
                return View();
            }

            List<ProductImage> images = new List<ProductImage>();
            foreach (IFormFile photo in product.Photos)
            {
                ProductImage productImage = new ProductImage();
                #region SaveImg
                if (!photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please Upload a Photo");
                    return View();
                }
                if (photo.IsOlder1MB())
                {
                    ModelState.AddModelError("Photo", "Max 1Mb");
                    return View();
                }


                string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                productImage.Url = await photo.SaveFileAsync(folder);
                images.Add(productImage);
                #endregion
            }

            #endregion


            List<TagProduct> tagProducts = new List<TagProduct>();
            foreach (int tagId in tagsId)
            {
                TagProduct tagProduct = new TagProduct
                {
                    TagId = tagId,
                };

                tagProducts.Add(tagProduct);    
            }
            product.TagProducts = tagProducts;


            List<ProductCategory> categories = new List<ProductCategory>();
            ProductCategory productCategory = new ProductCategory
            {
                CategoryId = mainCatId
            };
            categories.Add(productCategory);

            if (childCatId != null)
            {
                ProductCategory childProductCategory = new ProductCategory
                {
                    CategoryId = (int)childCatId
                };
                categories.Add(childProductCategory);
            }
           
            product.ProductCategories = categories;
            product.ProductImages = images;
            product.BrandId = brandId;
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion



        #region Update

        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.Brands = await _db.Brands.ToListAsync();
            ViewBag.Tags = await _db.Tags.ToListAsync();
            Category? firstMainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync();
            ViewBag.ChildCategories = firstMainCategory?.Children;
            if (id == null)
            {
                return NotFound();
            }

            Product? dbproduct = await _db.Products.
                Include(x => x.ProductDetail).
                Include(x => x.ProductImages).
                Include(x => x.Brand).
                Include(x => x.TagProducts).
                Include(x => x.ProductCategories).
                ThenInclude(x => x.Category).
                ThenInclude(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dbproduct == null)
            {
                return BadRequest();
            }


            return View(dbproduct);
        }







        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product, int brandId, List<int> tagsId, int mainCatId, int? childCatId)
        {
            

            if (id == null)
            {
                return NotFound();
            }

            Product? dbproduct = await _db.Products.
                Include(x => x.ProductDetail).
                Include(x => x.ProductImages).
                Include(x => x.Brand).
                Include(x => x.TagProducts).
                Include(x => x.ProductCategories).
                ThenInclude(x => x.Category).
                ThenInclude(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dbproduct == null)
            {
                return BadRequest();
            }

            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.Brands = await _db.Brands.ToListAsync();
            ViewBag.Tags = await _db.Tags.ToListAsync();
            Category? firstMainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync();
            ViewBag.ChildCategories = firstMainCategory?.Children;

            

            if (mainCatId == null)
            {

                return NotFound();
            }

            #region IsExist
            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name && x.Id != id);
            if (isExist)
            {
                ModelState.AddModelError("Name", "this product is alerady exist");
                return View();
            }
            #endregion


            #region Save Images
            if (product.Photos != null)
            {
                List<ProductImage> images = new List<ProductImage>();
                foreach (IFormFile photo in product.Photos)
                {
                    ProductImage productImage = new ProductImage();
                    #region SaveImg
                    if (!photo.IsImage())
                    {
                        ModelState.AddModelError("Photo", "Please Upload a Photo");
                        return View();
                    }
                    if (photo.IsOlder1MB())
                    {
                        ModelState.AddModelError("Photo", "Max 1Mb");
                        return View();
                    }


                    string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                    productImage.Url = await photo.SaveFileAsync(folder);
                    images.Add(productImage);
                    #endregion
                }
                dbproduct.ProductImages.AddRange(images);
            }        
            #endregion


            List<TagProduct> tagProducts = new List<TagProduct>();
            foreach (int tagId in tagsId)
            {
                TagProduct tagProduct = new TagProduct
                {
                    TagId = tagId,
                };

                tagProducts.Add(tagProduct);
            }
            dbproduct.TagProducts = tagProducts;


            List<ProductCategory> categories = new List<ProductCategory>();
            ProductCategory productCategory = new ProductCategory
            {
                CategoryId = mainCatId
            };
            categories.Add(productCategory);

            if (childCatId != null)
            {
                ProductCategory childProductCategory = new ProductCategory
                {
                    CategoryId = (int)childCatId
                };
                categories.Add(childProductCategory);
            }

            dbproduct.ProductCategories = categories;
            dbproduct.Name = product.Name;
            dbproduct.Price = product.Price;
            dbproduct.ProductDetail = product.ProductDetail;
            dbproduct.BrandId = brandId;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        public IActionResult DeleteImage(int? delId, int proId)
        {
            int count = _db.ProductImages.Count(x => x.ProductId == proId);
            if (count == 2)
            {
                return Content("1");
            }
            ProductImage? productImage = _db.ProductImages.FirstOrDefault(x => x.Id == delId);
            _db.ProductImages.Remove(productImage);
            _db.SaveChanges();

            return Content("0");
        }


        public async Task<IActionResult> GetChildCategories(int mainId)
        {
            List<Category> childCategories = await _db.Categories.Where(x => x.ParentId == mainId).ToListAsync();
            return PartialView("_GetChildCategoriesPartial", childCategories);
        }


    }
}
