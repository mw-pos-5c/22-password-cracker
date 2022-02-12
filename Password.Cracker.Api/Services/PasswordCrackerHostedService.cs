using Microsoft.AspNetCore.SignalR;

using Password.Cracker.Api.Hubs;
using Password.Cracker.Utils;

namespace Password.Cracker.Api.Services;

public class PasswordCrackerHostedService : IHostedService
{
    
    private readonly IHubContext<CrackerHub> hub;
    private readonly CrackService crackService;

    public PasswordCrackerHostedService(IHubContext<CrackerHub> hub, CrackService crackService)
    {
        this.hub = hub;
        this.crackService = crackService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            while (true) 
            {
                SendCrackStatusUpdate();
                Thread.Sleep(500);
            }
        });
        
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

        Console.WriteLine("Progress: "+perCent);
        
        hub.Clients.All.SendAsync("StatusUpdate", perCent).Wait();

        string? result = crackService.GetResult();
        if (result != null)
        {
            hub.Clients.All.SendAsync("FoundPassword", result).Wait();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
