using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
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
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            return View(model);
        }
        //POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //sprawdzanie czy model jest isvalid
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //sprawdzanie czy nazwa produktu jest unikalna
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
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

            //Utworzenie struktury katalogów
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(orginalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            if (file != null && file.ContentLength > 0)
            {
                //sprawdzenie rozszerzenia pliku obrazka
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/png" &&
                    ext != "image/pjpg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieporawidłowe rozszerzenie obrazka");
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        return View(model);
                    }
                }

                //inicjalizacja nazwy obrazka
                string imageName = file.FileName;

                //zapis nazwy obrazka do bazy

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                //zapis orginalny obrazek
                file.SaveAs(path);
                //zapis miniaturki obrazka
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }

            #endregion

            return RedirectToAction("AddProduct");
        }
        //Get: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Deklaracja listy ProductVM
            List<ProductVM> listOfProductVM;
            //ustawianie nr strony

            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Pobieranie listy produktów
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductVM(x))
                                    .ToList();

                //Lista kategorii do dropdownList
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //ustawiamy wybrana kategorie
                ViewBag.SelectCat = catId.ToString();

            }

            //ustawienie stronicowania (paginacji)
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;


            //zwracamy widok z listą produktów
            return View(listOfProductVM);
        }
        //Get: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //deklaracja productVm
            ProductVM model;
            using (Db db = new Db())
            {
                //pobieranie produktu do edycji
                ProductDTO dto = db.Products.Find(id);

                //sprawdzenie czy produkt istnieje
                if (dto == null)
                {
                    return Content("Ten produkt nie istnieje!");
                }
                //inicjalizacja modelu
                model = new ProductVM(dto);
                //Lista kategorii
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //ustawiamy image
                model.GalleryImages = Directory.EnumerateDirectories(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                        .Select(fn => Path.GetFileName(fn));

            }

            return View(model);
        }
        //POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //pobieranie id produktu do edycji
            int id = model.Id;

            // pobranie kategorii dla listy rozwijanej
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            //ustawiamy image
            model.GalleryImages = Directory.EnumerateDirectories(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                       .Select(fn => Path.GetFileName(fn));

            //sprawdzanie modelstate
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //sprawdzenie czy nazwa produktu jest unikalna
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta!");
                    return View(model);
                }
            }

            //edycja produktu i zapis na bazie
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Prize = model.Prize;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;
                CategorieDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();
            }

            TempData["SM"] = "Edytowałeś produkt";

            #region ImageUpload

            //sprawdzanie czy jest plik

            if (file != null && file.ContentLength > 0)
            {
                //sprawdzenie rozszerzenie pliku czy to jest obrazek
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/png" &&
                    ext != "image/pjpg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieporawidłowe rozszerzenie obrazka");
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        return View(model);
                    }
                }
                //Utworzenie struktury katalogów
                var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                var pathString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Usuwamy stare pliki z katalogów
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }
                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //Dodawanie nowych obrazków i zapis na bazie nazwy plików
                string ImageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = ImageName;
                    db.SaveChanges();
                }
                var path = string.Format("{0}\\{1}", pathString1, ImageName);
                var path2 = string.Format("{0}\\{1}", pathString2, ImageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            return RedirectToAction("EditProduct");
        }
        //Get: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            //usunięcie produktu z bazy
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }
            //usuniecie folderu z obrazkami
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString,true);
            }

            return RedirectToAction("Products");
        }
        //Post: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public ActionResult SaveGalleryImages(int id)
        {
            //petla po obrazkach
            foreach (string fileName in Request.Files)
            {
                //inicjalizacja
                HttpPostedFileBase file = Request.Files[fileName];
                //sprawdzenie czy mamy plik i czy nie jest pusty
                if (file != null && file.ContentLength > 0)
                {
                    //ustawienie scieżek do katalogów
                    var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                    string pathString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //zapis obrazków i miniaturek
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);

                }
            }
            return View();
        }

    }
}