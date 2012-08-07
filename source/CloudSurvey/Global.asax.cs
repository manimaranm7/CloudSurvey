namespace CloudSurvey
{
    using System.Data.Entity;
    using System.Linq;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using System.Web.Security;

    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DataConfig.ConfigureDatabase();
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleDependencyResolver();

            // Create an admin account
            if (Membership.FindUsersByName("admin").Cast<MembershipUser>().FirstOrDefault() == null)
            {
                Membership.CreateUser("admin", "Contoso123!", "admin@contoso.com");
            }
        }
    }
}