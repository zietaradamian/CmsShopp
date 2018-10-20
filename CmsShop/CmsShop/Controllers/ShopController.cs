using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }
        public ActionResult CategoryMenuPartial()
        {
            //deklarujemy liste kategori VM
            List<CategoryVM> categoryVMList;

            //inicjalizacja listy
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            //zwracamy partialView z listą
            return PartialView(categoryVMList);
        }
        public ActionResult Category(string name)
        {
            //deklaracja ProductVm list
            List<ProductVM> productVMList;
            using (Db db = new Db())
            {
                //pobranie id kategorii
                CategorieDTO catDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = catDTO.Id;

                //inicjalizacja listy produktów
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();
                //pobieramy nazwę kategorii
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }
            return View(productVMList);
        }
        //Get: Shop/produkt-szczegoly/name
        [ActionName("produkt-szczegoly")]
        public ActionResult ProductDetails(string name)
        {
            //deklaracja productVm i producktDto
            ProductVM model;
            ProductDTO dto;
            //inicjalizacja productID
            int id = 0;

            using(Db db = new Db())
            {
                //sprawdzamy czy produkt istnieje
                if (!db.Products.Any(x=>x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                //inicjalizacja productDto
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //pobiranie id
                id = dto.Id;

                //inicjalizacja modelu
                model = new ProductVM(dto);
            }
            //pobieramy galerie zdjeć dla wybranego produktu

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                           .Select(fn => Path.GetFileName(fn));


            return View("ProductDetails", model);

        }
    }
}