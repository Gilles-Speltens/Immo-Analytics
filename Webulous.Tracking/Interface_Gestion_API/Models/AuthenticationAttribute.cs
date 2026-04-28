
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Interface_Gestion_API.Models
{
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var isAuthenticated = filterContext.HttpContext.Session.GetString("isAuthenticated");

            if (isAuthenticated == null || !isAuthenticated.Equals("true"))
            {
                filterContext.Result = new RedirectResult("/Home/SignIn");
            }
        }
    }
}
