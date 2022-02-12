#region usings

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Password.Cracker.Utils;

public class CrackUtils : IDisposable
{
    #region Constants and Fields

    private readonly byte[] alphabet;
    private readonly int length;
    private readonly byte[] target;
    private readonly Thread?[] threads;
    private readonly int[] globalIterators;
    #endregion

    public CrackUtils(byte[] alphabet, int length, byte[] target, int threadCount)
    {
        this.alphabet = alphabet;
        this.length = length;
        this.target = target;
        this.threads = new Thread[threadCount];
        this.globalIterators = new int[threadCount];

        Total = (long) Math.Pow(alphabet.Length, length);
    }

    public CrackUtils(string alph, int len, string hash, int threadCount) : this(GetBytes(alph), len, HexStringToByte(hash), threadCount)
    {
        
    }

    public int ThreadsRunning => threads.Count(thread => thread != null && thread.IsAlive);
    public bool Found { get; private set; }
    public string Result { get; private set; }
    public long Total { get; }
    public long TotalHashed => globalIterators.Sum();
    public double PerCent => (double) TotalHashed / Total;

    public byte[] CalcStart(long start)
    {
        long from = start;
        long basis = alphabet.Length;

        var result = new byte[length];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = (byte) (from % basis);
            from /= basis;
        }

        return result;
    }

    public void Crack(long from, long to, out int iterator)
    {
        using var sha256 = SHA256.Create();
        
        byte[] currentTry = CalcStart(from);
        var result = new byte[length];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = alphabet[0];
        }
        
        for (iterator = 0; iterator < to - from; iterator++)
        {
            if (Found) return;

            byte[] hash = sha256.ComputeHash(result);
            

            if (CmpBytes(hash, target))
            {
                Found = true;
                Result = Encoding.UTF8.GetString(result);
                return;
            }

            ++currentTry[0];
            for (var i = 0; i < length; i++)
            {
                if (currentTry[i] == alphabet.Length)
                {
                    currentTry[i] = 0;

                    if (i == length - 1)
                        return;

                    ++currentTry[i + 1];
                    result[i] = alphabet[currentTry[i]];
                }
                else
                {
                    result[i] = alphabet[currentTry[i]];
                    break;
                }
            }
        }
    }

    public void CrackAsync()
    {
        int count = threads.Length;
        long piece = Total / count;

        long from;
        long to = 0;

        for (var i = 0; i < count; i++)
        {
            from = to;
            to += piece;
            
            long from1 = from;
            long to1 = to;
            int i1 = i;
            
            threads[i] = new Thread(() => Crack(from1, to1, out globalIterators[i1]));
            threads[i].Start();
        }
    }

    public static byte[] HexStringToByte(string hex)
    {
        var data = new byte[hex.Length / 2];
        for (var i = 0; i < data.Length; i++)
        {
            string byteValue = hex.Substring(i * 2, 2);
            data[i] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return data;
    }

    public static byte[] GetBytes(string input)
    {
        return Encoding.UTF8.GetBytes(input);
    }
    
    private static bool CmpBytes(byte[] arr1, byte[] arr2)
    {
        for (var i = 0; i < arr1.Length; i++)
        {
            if (arr1[i] != arr2[i]) return false;
        }

        return true;
    }
    
    public static string? WordlistCrack(string[] input, byte[] hash)
    {
        var sha256 = SHA256.Create();
        
        for (var i = 0; i < input.Length; i++)
        {
            if (CmpBytes(sha256.ComputeHash(Encoding.UTF8.GetBytes(input[i])), hash))
            {
                return input[i];
            }
        }
        
        return null;
    }

    public async Task<string?> GetResultAsync()
    {
        await Task.Run(() =>
        {
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        });

        return Result;
    }

    public void Dispose()
    {
        Result = "";
        Found = true;
        
        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }
}
