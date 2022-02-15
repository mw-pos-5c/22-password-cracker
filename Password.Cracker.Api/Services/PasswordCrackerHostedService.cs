#region usings

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

using Password.Cracker.Api.Hubs;
using Password.Cracker.Utils;

#endregion

namespace Password.Cracker.Api.Services;

public class PasswordCrackerHostedService : IHostedService
{
    #region Constants and Fields

    private readonly CrackService crackService;

    private readonly IHubContext<CrackerHub> hub;

    private PeriodicTimer timer;

    #endregion

    public PasswordCrackerHostedService(IHubContext<CrackerHub> hub, CrackService crackService)
    {
        this.hub = hub;
        this.crackService = crackService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        Task.Run(async () =>
            {
                while (await timer.WaitForNextTickAsync())
                {
                    SendCrackStatusUpdate();
                    Thread.Sleep(500);
                }
            },
            cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Dispose();
        return Task.CompletedTask;
    }

    private void SendCrackStatusUpdate()
    {
        CrackUtils? current = crackService.GetCurrentCrackAttempt();
        if (current == null)
        {
            return;
        }

        double perCent = current.PerCent;

        Console.WriteLine("Progress: " + perCent);

        hub.Clients.All.SendAsync("StatusUpdate", perCent).Wait();

        string? result = crackService.GetResult();
        if (result != null)
        {
            hub.Clients.All.SendAsync("FoundPassword", result).Wait();
        }
        else if (current.ThreadsRunning == 0 && current.Ready)
        {
            hub.Clients.All.SendAsync("FoundPassword", "* NO MATCH *").Wait();
            crackService.StopCurrentAttempt();
        }
    }
}
