#region usings

using Microsoft.AspNetCore.SignalR;

using Password.Cracker.Api.Services;

#endregion

namespace Password.Cracker.Api.Hubs;

public class CrackerHub : Hub<ICrackerHub>
{
    #region Constants and Fields

    private readonly CrackService service;

    #endregion

    public CrackerHub(CrackService service)
    {
        this.service = service;
    }

    public void Crack(string hash, string alph, int len)
    {
        service.Crack(hash, alph, len);
    }
}
