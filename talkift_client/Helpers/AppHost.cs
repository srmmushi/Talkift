using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Talkift.Client.Engines;
using Talkift.Client.Services;
using Talkift.Client.Services.V2;

namespace Talkift.Client;

public static class AppHost
{
    public static ServiceProvider Create()
    {
        var services = new ServiceCollection();

        AddTalkiftServices(services);
        AddViewModels(services);
        AddPages(services);
        AddControls(services);

        return services.BuildServiceProvider();
    }

    public static ServiceProvider Create(ServiceCollection additionalServices)
    {
        var services = new ServiceCollection();
        AddTalkiftServices(services);
        additionalServices(services);
        AddViewModels(services);
        AddPages(services);
        AddControls(services);

        return services.BuildServiceProvider();
    }
}
