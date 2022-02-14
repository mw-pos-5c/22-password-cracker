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
    private readonly int[] sharedIterators;
    private readonly byte[] target;
    private readonly Thread?[] threads;

    #endregion

    public CrackUtils(byte[] alphabet, int length, byte[] target, int threadCount)
    {
        this.alphabet = alphabet;
        this.length = length;
        this.target = target;
        threads = new Thread[threadCount];
        sharedIterators = new int[threadCount];

        Total = (long) Math.Pow(alphabet.Length, length);
    }

    public CrackUtils(string alph, int len, string hash, int threadCount)
        : this(GetBytes(alph), len, HexStringToByte(hash), threadCount)
    {
    }

    public bool Found { get; private set; }
    public double PerCent => (double) TotalHashed / Total;
    public bool Ready { get; private set; }
    public string Result { get; private set; }

    public int ThreadsRunning => threads.Count(thread => thread != null && thread.IsAlive);
    public long Total { get; }
    public long TotalHashed => sharedIterators.Sum();

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
                    if (i == length - 1)
                    {
                        return;
                    }

                    currentTry[i] = 0;
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

            to = i != count - 1
                ? to + piece
                : Total;

            long from1 = from;
            long to1 = to;
            int i1 = i;

            threads[i] = new Thread(() =>
            {
                Crack(from1, to1, out sharedIterators[i1]);

                // Console.WriteLine($"[{from1} - {to1}] Not hashed {(to1 - from1 - sharedIterators[i1])}");
            });
            threads[i]?.Start();
        }

        Ready = true;
    }

    public void Dispose()
    {
        Result = "";
        Found = true;

        foreach (Thread? thread in threads)
        {
            thread?.Join();
        }
    }

    public static byte[] GetBytes(string input)
    {
        return Encoding.UTF8.GetBytes(input);
    }

    public async Task<string?> GetResultAsync()
    {
        await Task.Run(() =>
        {
            foreach (Thread? thread in threads)
            {
                thread?.Join();
            }
        });

        return Result;
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

    public void WordlistCrack(string[] wordlist)
    {
        var sha256 = SHA256.Create();

        foreach (string word in wordlist)
        {
            if (CmpBytes(sha256.ComputeHash(Encoding.UTF8.GetBytes(word)), target))
            {
                string result = word;
                Found = true;
                Result = result;
                break;
            }
        }

        Ready = true;
    }

    private static bool CmpBytes(byte[] arr1, byte[] arr2)
    {
        for (var i = 0; i < arr1.Length; i++)
        {
            if (arr1[i] != arr2[i]) return false;
        }

        return true;
    }
}
