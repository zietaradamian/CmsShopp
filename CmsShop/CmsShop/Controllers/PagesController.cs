using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{pages}
        public ActionResult Index(string page="")
        {
            //ustawiamy adres naszej strony
            if (page == "")
            {
                page = "home";
            }
            //deklaracja pageVM i pageDTO
            PageVM model;
            PageDTO dto;
            //Sprawdzamy czy strona istnieje
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x=> x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page="" });
                }
            }

            //pobieramy pageDto 
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }
            //ustawianie tytułu naszej strony

            ViewBag.PageTitle = dto.Title;

            //sprawdzaamy ma pasek boczny (sidebar)

            if (dto.HasSideBar == true)
            {
                ViewBag.Sidebar = "Tak";
            }
            else
            {
                ViewBag.Sidebar = "Nie";
            }
            //inizjalizacja pageVm
            model = new PageVM(dto);
            //zwracanie widoku z pagevm
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //deklaracja pageVm
            List<PageVM> pageVMList;
            //pobranie stron
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray()
                        .OrderBy(x => x.Sorting)
                        .Where(x => x.Slug != "home")
                        .Select(x => new PageVM(x)).ToList();
            }
            //zwracamy model do partialview
            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //deklarujemy model
            SidebarVM sidebarVm;
            //inicjalizujemy model
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                sidebarVm = new SidebarVM(dto);
            }
            //zwracamy partialview z modelem
            return PartialView(sidebarVm);
        }
    }
}