
using Microsoft.AspNetCore.Mvc.Filters;

namespace Interface_Gestion_API.Models
{
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //redirect if not authenticated
            if (!(filterContext.HttpContext.Session.GetString("isAuthenticated") != null &&
                filterContext.HttpContext.Session.GetString("isAuthenticated").Equals("true")))
            {
                string loginUrl = "/Home/SignIn";
                filterContext.HttpContext.Response.Redirect(loginUrl, true);
            }
        }
    }
}
