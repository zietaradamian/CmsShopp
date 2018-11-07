using CmsShop.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CmsShop
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
        protected void Application_AuthenticateRequest()
        {
            if (User == null)
            {
                return;
            }
            //pobieramy nazwę użytkownika
            string userName = Context.User.Identity.Name;

            string[] roles = null;
            using (Db db = new Db())
            {
                //pobieramy dane dla użytkownika z bazy aby dostać role
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);

                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();

            }

            //tworzy IPricipal object
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal UserObj = new GenericPrincipal(userIdentity, roles);

            //update context user
            Context.User = UserObj;
        }
    }
}
