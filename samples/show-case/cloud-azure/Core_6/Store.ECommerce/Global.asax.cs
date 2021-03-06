using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NServiceBus;

public class MvcApplication :
    HttpApplication
{
    public static IEndpointInstance EndpointInstance;

    protected void Application_End()
    {
        EndpointInstance?.Stop().GetAwaiter().GetResult();
    }

    protected void Application_Start()
    {
        AsyncStart().GetAwaiter().GetResult();
    }

    static async Task AsyncStart()
    {
        var endpointConfiguration = new EndpointConfiguration("Store.ECommerce");
        endpointConfiguration.PurgeOnStartup(true);
        endpointConfiguration.ApplyCommonConfiguration(transport =>
        {
            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(Store.Messages.Commands.SubmitOrder).Assembly, "Store.Messages.Commands", "Store.Sales");
            routing.RegisterPublisher(typeof(Store.Messages.Events.DownloadIsReady), "Store.ContentManagement");
            routing.RegisterPublisher(typeof(Store.Messages.Events.OrderCancelled), "Store.Sales");
            routing.RegisterPublisher(typeof(Store.Messages.Events.OrderPlaced), "Store.Sales");
        });

        EndpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        AreaRegistration.RegisterAllAreas();
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
}
