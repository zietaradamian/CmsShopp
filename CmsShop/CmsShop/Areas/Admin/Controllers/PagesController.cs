﻿using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Deklaracja listy pagevm
            List<PageVM> pagesList;


            using (Db db = new Db())
            {
                //Inicjalizacja listy
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();


            }


            //zwracanie stron do widoku
            return View(pagesList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {

            return View();
        }
        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Sprawdzanie model state

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                string slug;

                //inicjalizacja pageDTO
                PageDTO dto = new PageDTO();

                //gdy nie ma adresu strony zostaje przypisany tytul
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //zapobieganie dodanie takiej samej nazwy strony
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Ten tytuł lub adres strony już istnieje.");
                    return View(model);
                }

                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                dto.Sorting = 1000;

                //Zapis dto
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "Dodałeś nową strone";

            return RedirectToAction("AddPage");
        }
        // Get: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //deklaracja PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //pobieramy strone z bazy po Id
                PageDTO dto = db.Pages.Find(id);
                //sprawdzanie czy istnieje dana strona
                if (dto == null)
                {
                    return Content("Strona nie istnieje");
                }

                model = new PageVM(dto);
            }


            return View(model);
        }


        // POST: Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //pobranie id strony
                int id = model.Id;
                //inicjalizacja slug
                string slug = "home";

                //pobieranie strony do edycji
                PageDTO dto = db.Pages.Find(id);



                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                //sprawdzanie duplikacji strony,adresu
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Strona lub adres strony już istnieje!");
                    return View(model);
                }
                //modyfikacja danych
                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                //zapis do bazy
                db.SaveChanges();
            }

            TempData["SM"] = "Wyedytowałeś strone";

            return RedirectToAction("EditPage");
        }
        // Get: Admin/Pages/Details/id
        [HttpGet]
        public ActionResult Details(int id)
        {
            //deklaracja PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //pobieranie strony o id
                PageDTO dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("Strona nie istnieje!");
                }

                //inicjalizacja pagevm

                model = new PageVM(dto);



            }
            return View(model);
        }
        // Get: Admin/Pages/Delete/id
        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (Db db = new Db())
            {
                //Pobranie strony do usunięcia
                PageDTO dto = db.Pages.Find(id);
                //usuwanie strony z bazy
                db.Pages.Remove(dto);

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        // POST: Admin/Pages/ReorderPages
        [HttpPost]
        public ActionResult ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;
                PageDTO dto;
                //sortowanie stron zapis na bazie
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;

                }
            }
            return View();
        }

        // Get: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Deklaracja SidebarVm

            SidebarVM model;

            using (Db db = new Db())
            {
                //Pobieranie SidebarDTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //inicjalizacja modelu
                model = new SidebarVM(dto);
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            //Deklaracja SidebarVm
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                //modyfikacja Sidebar
                dto.Body = model.Body;

                db.SaveChanges();

            }
            //komunikat o modyfkacji sidebar
            TempData["SM"] = "Zmodyfikowałeś pasek boczny";
            return RedirectToAction("EditSidebar");
        }

    }
}