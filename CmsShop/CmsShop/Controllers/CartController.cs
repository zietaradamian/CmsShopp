using CmsShop.Models.Data;
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
            // inicjalizacja koszyka
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // sprawdzamy czy nasz koszyk jest pusty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Twój koszyk jest pusty";
                return View();
            }

            // obliczenie wartosci podsumowania koszyka i przekazanie do ViewBag
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
                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //ustawiamy ilość i cena na 0
                qty = 0;
                price = 0m;
            }

            return PartialView(model);
        }
        public ActionResult AddToCartPartial(int id)
        {
            //inicjalizacja cartvm list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //iniclizacja cartvm
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //pobieramy produkt
                ProductDTO product = db.Products.Find(id);

                //sprawdzamy czy ten produkt jest już w koszyku
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //w zaleznosci czy produkt jest w koszyku jeśli jest zwiekszyamy ilosc

                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Prize,
                        Image = product.ImageName
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }
            //pobieramy wartosci qty i price i dodajemy do modelu
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;
            //zapis do sesji
            Session["cart"] = cart;

            return PartialView(model);
        }
    }
}