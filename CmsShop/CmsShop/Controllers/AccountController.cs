using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Account;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }
        // GET: /account/login
        [HttpGet]
        public ActionResult Login()
        {
            //sprawdzanie czy użytkownik jest zalogowany
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("user-profile");
            }
            //zwracamy widok logowania
            return View();

        }
        // POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //sprawdzenie modelstate
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //sprawdzamy użytkownika
            bool isValid = false;
            using (Db db = new Db())
            {
                if (db.Users.Any(x=>x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }
            if (!isValid)
            {
                ModelState.AddModelError("", "Nieporawidłowa nazwa użytkownia lub hasło!");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }

        }
        // GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }
        // POST: /account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // sprawdzenie hasła
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Hasła nie pasują do siebie");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // sprawdzenie czy nazwa uzytkownika jest unikalna
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika " + model.UserName + " jest już zajęta");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                // utworzenie uzytkownika
                UserDTO usserDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password,
                };

                // dodanie uzytkownika
                db.Users.Add(usserDTO);
                // zapis na bazie
                db.SaveChanges();

                // dodanie roli dla uzytkownika
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = usserDTO.Id,
                    RoleId = 2
                };

                // dodanie roli
                db.UserRoles.Add(userRoleDTO);
                // zapis na bazie
                db.SaveChanges();
            }

            // TempData komunikat
            TempData["SM"] = "Jesteś teraz zarejestrowany i możesz się zalogować!";


            return Redirect("~/account/login");
        }
        // Get: /account/LogOut
        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }
        // 
        [Authorize]
        public ActionResult UserNavPartial()
        {
            //pobieramy username
            string userName = User.Identity.Name;

            //deklarujemy model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //pobieramy użytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };

            }
            return PartialView(model);
        }
        [Authorize]
        public ActionResult UserNavPartial2()
        {
            // Get username
            string username = User.Identity.Name;
            // Declare model
            UserNavPartialVM model;
            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);
                // Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            // Return partial view with model
            return PartialView(model);
        }
        [Authorize]
        public ActionResult UserNavPartial3()
        {
            // Get username
            string username = User.Identity.Name;
            string email = string.Empty;
            // Declare model
            UserNavPartialVM3 model;
            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);
                // Build the model
                // Build the model
                model = new UserNavPartialVM3()
                {
                    Email = dto.EmailAddress
                };
            }
            // Return partial view with model
            return PartialView(model);
        }

        // Get: /account/user-profile
        [ActionName("user-profile")]
        [HttpGet]
        [Authorize]
        public ActionResult UserProfile()
        {
            //pobieramy nazwę użytkownika
            string userName = User.Identity.Name;

            //deklarujemy model
            UserProfileVM model;

            using (Db db = new Db())
            {
                //pobieramy użytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);

                model = new UserProfileVM(dto);
            }
            return View("UserProfile",model);
        }
        // POST: /account/user-profile
        [ActionName("user-profile")]
        [HttpPost]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            //sprawdzanie modelstate
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }
            //sprawdznie czy hasła są takie same
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Hasła nie są takie same!");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //pobieramy nazwę użytkownika 
                string userName = User.Identity.Name;

                //sprawdznie czy ta nazwa użytkownika jest unikalna
                if (db.Users.Where(x=>x.Id != model.Id).Any(x=>x.UserName == userName))
                {
                    ModelState.AddModelError("", "Ta nazwa użytkownika" + model.UserName +" jest zajęta!");
                    userName = "";
                    return View("UserProfile", model);
                }
                //edycja dto
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
                db.SaveChanges();
            }

            //ustawienie komunikatu TD
            TempData["SM"] = "Edytowałeś swój profil";
            return Redirect("~/account/user-profile");
        }
        // Get: /account/Orders
        [HttpGet]
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //inicjalizacja Listy zamówień dla użytkownika
            List<OrderForUserVM> ordersForUser = new List<OrderForUserVM>();

            using (Db db = new Db())
            {
                //pobieramy Id użytkownika
                UserDTO user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                //pobieramy zamówienia dla użytkownika
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                foreach (var order in orders)
                {
                    //inicjalizacja slownika produktów
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    decimal total = 0m;

                    //pobieramy szczegoly zamowienia
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        //pobieramy produkt
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();
                        //pobieramy cene
                        decimal price = product.Prize;
                        //pobieramy nazwe
                        string productName = product.Name;

                        //dodajemy produkt do slownika
                        if(!productsAndQty.ContainsKey(productName))
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }
                    ordersForUser.Add(new OrderForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            return View(ordersForUser);
        }
    }
}