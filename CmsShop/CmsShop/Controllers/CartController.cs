using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        public JsonResult IncrementProduct(int productId)
        {
            //Inicjalizacja Listy CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //pobieramy Cartvm
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
            //zwiekszamy ilosc w produktu w koszyku

            model.Quantity++;

            //przygotowanie danych do zwrócenia json

            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public JsonResult DecrementProduct(int productId)
        {
            //Inicjalizacja Listy CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //pobieramy Cartvm
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
            //zmiejszamy ilosc w produktu w koszyku
            if (model.Quantity > 1)
            {
                model.Quantity--;
            }
            else
            {
                model.Quantity = 0;
                cart.Remove(model);
            }
            //przygotowanie danych do zwrócenia json

            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public void RemoveProduct(int productId)
        {
            //Inicjalizacja Listy CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //pobieramy Cartvm
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            //usuwamy produkt

            cart.Remove(model);
        }
        public ActionResult PayPalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            return PartialView(cart);
        }
        [HttpPost]
        public void PlaceOrder()
        {
            // ppobieramy zawartosc koszyka z sesi
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // pobranie nazwy uzytkownika
            string username = User.Identity.Name;

            // deklarujemy numer zamowienia
            int orderId = 0;

            using (Db db = new Db())
            {
                // inicjalizacja OrderDTO
                OrderDTO oredrDTO = new OrderDTO();

                // pobieramu user id
                var user = db.Users.FirstOrDefault(x => x.UserName == username);
                int userId = user.Id;

                // ustawienie orderDTO i zapis 
                oredrDTO.UserId = userId;
                oredrDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(oredrDTO);
                db.SaveChanges();

                // pobieramy id zapisanego zamowienia
                orderId = oredrDTO.OrderId;

                // inicjalizacja OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }

            // wysylanie emaila do admina
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("edb057912ae9dc", "e5f758e024e7f2"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "Nowe zamowienie", "Masz nowe zamowienie. Numer zamówienia " + orderId);

            // reset session
            Session["cart"] = null;
        }



    }
}