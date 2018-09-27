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
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x=> new PageVM(x)).ToList();


            }


            //zwracanie stron do widoku
            return View(pagesList);
        }
    }
}