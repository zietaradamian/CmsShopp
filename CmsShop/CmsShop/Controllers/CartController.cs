using CmsShop.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //inicjalizacja koszyka
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //sprawdzamy czy nasz koszyk jest pusty

            if (cart.Count == 0 || Session["cart"] != null)
            {
                ViewBag.Message = "Twój koszyk jest pusty";
                return View();
            }

            //Obliczenie wartości koszyka (podsumowanie calej ceny)
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;

            return View(cart);
        }
        public ActionResult CartPartial()
        {
            //inicjalizacja CartVM
            CartVM model = new CartVM();

            //inicjalizacja Ilość i cena
            int qty = 0;
            decimal price = 0;

            //sprawdzamy czy mamy dane koszyka w zapisane w sesji
            if (Session["cart"] != null)
            {
                //pobieranie wartosci qty i price z sesji
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;

                }
            }
            else
            {
                //ustawiamy ilość i cena na 0
                qty = 0;
                price = 0m;
            }

            return PartialView(model);
        }
    }
}