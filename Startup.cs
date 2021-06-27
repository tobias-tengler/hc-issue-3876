using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HotChocolate;
using System;
using HotChocolate.Types;
using HotChocolate.Subscriptions;
using System.Threading.Tasks;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using System.Threading;
using HotChocolate.AspNetCore.Subscriptions.Messages;
using HotChocolate.Execution;
using Microsoft.Extensions.Logging;
using HotChocolate.Fetching;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class Subscription
{
    [Subscribe]
    public User UserAdded([EventMessage] User user)
    {
        return user;
    }
}

public class Mutation
{
    public async Task<User> AddUser([Service] ITopicEventSender sender)
    {
        var user = new User()
        {
            Name = "Test" + new Random().Next(100000),
        };

        await sender.SendAsync("UserAdded", user);

        return user;
    }
}

public class Query
{
    public string Test => "Test";
}

public class SocketInterceptor : DefaultSocketSessionInterceptor
{
    public override ValueTask<ConnectionStatus> OnConnectAsync(ISocketConnection connection, InitializeConnectionMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine("OnConnectAsync");

        return base.OnConnectAsync(connection, message, cancellationToken);
    }

    public override ValueTask OnCloseAsync(ISocketConnection connection, CancellationToken cancellationToken)
    {
        Console.WriteLine("OnCloseAsync");

        return base.OnCloseAsync(connection, cancellationToken);
    }

    public override ValueTask OnRequestAsync(ISocketConnection connection, IQueryRequestBuilder requestBuilder, CancellationToken cancellationToken)
    {
        Console.WriteLine("OnRequestAsync");

        return base.OnRequestAsync(connection, requestBuilder, cancellationToken);
    }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddInMemorySubscriptions();

        services
            .AddGraphQLServer()
            .AddSocketSessionInterceptor<SocketInterceptor>()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseWebSockets();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGraphQL();
        });
    }
}