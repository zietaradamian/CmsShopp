﻿using CmsShop.Models.Data;
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
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id","Name");

                //ustawiamy wybrana kategorie
                ViewBag.SelectCat = catId.ToString();

            }

            //ustawienie stronicowania (paginacji)
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;


            //zwracamy widok z listą produktów
            return View(listOfProductVM);
        }

    }
}