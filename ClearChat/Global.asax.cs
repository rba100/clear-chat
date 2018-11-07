using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using RedGate.Shared.Authentication.Federation;

namespace ClearChat
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        static Uri Fake = new Uri("https://fakests.coredev-uat-1.testnet.red-gate.com/FederationMetadata/2007-06/FederationMetadata.xml");
        static Uri Live = new Uri("https://login.red-gate.com/FederationMetadata/2007-06/FederationMetadata.xml");

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RedgateFederationAuthentication.Configure(new Uri("urn:internal:red-gate"),
                                                      Fake);
        }
    }
}
