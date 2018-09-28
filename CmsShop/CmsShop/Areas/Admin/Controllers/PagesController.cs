using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
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
        // Get: Admin/Pages/EditPage
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
                string slug= "home";

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
                if (db.Pages.Where(x=> x.Id != id).Any(x=> x.Title == model.Title) ||
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

    }
}