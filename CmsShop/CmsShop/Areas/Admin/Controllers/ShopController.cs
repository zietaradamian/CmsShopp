using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //deklaracja listy kategorii
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            return View(categoryVMList);
        }
        //POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Deklaracja id
            string id;

            using (Db db = new Db())
            {
                //Sprawdzanie czy dana kategoria już istnieje
                if (db.Categories.Any(x => x.Name == catName))
                {
                    return "tytulzajety";
                }

                //Inicjalizcja DTO
                CategorieDTO dto = new CategorieDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;

                //zapis do bazy
                db.Categories.Add(dto);
                db.SaveChanges();

                //pobieranie id
                id = dto.Id.ToString();
            }

            return id;
        }
        //POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //inicjalizacja licznika (pomocniczy)
                int count = 1;

                //deklaracja dto
                CategorieDTO dto;

                //sortowanie kategorii

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    //zapis do bazy
                    db.SaveChanges();

                    count++;
                }
            }
            return View();

        }
        //Get: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //pobieranie kategorie
                CategorieDTO dto = db.Categories.Find(id);

                //Usuwanie kategorii

                db.Categories.Remove(dto);
                db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }
        //POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //sprawdzanie czy taka kategoria istnieje
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "tytulzajety";

                //pobieranie kategorii
                CategorieDTO dto = db.Categories.Find(id);
                //edycja kategorii
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //zapis na bazie

                db.SaveChanges();

            }
            return "Ok";
        }

        //Get: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Inicjalizacja modelu
            ProductVM model = new ProductVM();

            //Pobieranie listy kategorii

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id","Name");
            }
            return View(model);
        }
        //POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //sprawdzanie czy model jest isvalid
            if(!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //sprawdzanie czy nazwa produktu jest unikalna
            using(Db db = new Db())
            {
                if(db.Products.Any(x=> x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajeta!");
                    return View(model);
                }
            }

            //deklaracja produkt id
            int id;

            //dodanie produktu i zapis na bazie

            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Prize = model.Prize;
                product.CategoryId = model.CategoryId;

                CategorieDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //pobranie dodanego produktu
                id = product.Id;
            }
            //Ustawienie komunikatu TempData

            TempData["SM"] = "Dodałeś produkt";

            #region Upload Image



            #endregion 

            return View();
        }

    }
}