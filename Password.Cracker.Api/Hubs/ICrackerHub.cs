namespace Password.Cracker.Api.Hubs;

public interface ICrackerHub
{
    public Task FoundPassword(string pw);
}
