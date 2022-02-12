using Microsoft.AspNetCore.SignalR;

using Password.Cracker.Api.Services;

namespace Password.Cracker.Api.Hubs;

public class CrackerHub : Hub<ICrackerHub>
{
    private readonly CrackService service;

    public CrackerHub(CrackService service)
    {
        this.service = service;
    }

    public void Crack(string hash, string alph, int len)
    {
        service.Crack(hash, alph, len);
    }
}
