using System.Text;

using Password.Cracker.Utils;

namespace Password.Cracker.Api.Services;

public class CrackService
{
    private CrackUtils? currentCrack;

    public void Crack(string hash, string alph, int len)
    {
        int threadCount = Environment.ProcessorCount;
        if (currentCrack != null) return;
        currentCrack = new CrackUtils(alph, len, hash, threadCount);
        currentCrack.CrackAsync();
    }

    public void StopCurrentAttempt()
    {
        if (currentCrack == null)
        {
            return;
        }
        
        currentCrack.Dispose();
        currentCrack = null;
    }

    public string? GetResult()
    {
        if (currentCrack is not {Found: true})
        {
            return null;
        }
        
        string result = currentCrack.Result;
        currentCrack = null;
        return result;
    }

    public CrackUtils? GetCurrentCrackAttempt()
    {
        return currentCrack;
    }
}
