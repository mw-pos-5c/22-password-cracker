#region usings

using System;

using Password.Cracker.Utils;

#endregion

namespace Password.Cracker.Api.Services;

public class CrackService
{
    #region Constants and Fields

    private CrackUtils? currentCrack;

    #endregion

    public void Crack(string hash, string alph, int len)
    {
        if (currentCrack != null) return;
        int threadCount = Environment.ProcessorCount + 1;

        currentCrack = new CrackUtils(alph, len, hash, threadCount);

        if (alph.Length + len == 0)
        {
            var wordlist = FabelwesenUtils.GetFabelwesen();
            Console.WriteLine("Wordlist: " + wordlist.Length);
            currentCrack.WordlistCrack(wordlist);
        }
        else
        {
            currentCrack.CrackAsync();
        }
    }

    public CrackUtils? GetCurrentCrackAttempt()
    {
        return currentCrack;
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

    public void StopCurrentAttempt()
    {
        if (currentCrack == null)
        {
            return;
        }

        currentCrack.Dispose();
        currentCrack = null;
    }
}
