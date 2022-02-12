#region usings

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Password.Cracker.Cli;

public class CrackUtils
{
    #region Constants and Fields

    private readonly byte[] alphabet;
    private readonly int length;
    private readonly byte[] target;

    #endregion

    public CrackUtils(byte[] alphabet, int length, byte[] target)
    {
        this.alphabet = alphabet;
        this.length = length;
        this.target = target;

        Total = (long) Math.Pow(alphabet.Length, length);
    }

    public bool Found { get; private set; }
    public string Result { get; private set; }
    public long Total { get; }
    public long TotalHashed { get; private set; }

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

    public void Crack(long from, long to)
    {
        var sha256 = SHA256.Create();

        byte[] currentTry = CalcStart(from);
        var result = new byte[length];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = alphabet[0];
        }

        for (var x = 0; x < to - from; x++)
        {
            if (Found) return;
            
            byte[] hash = sha256.ComputeHash(result);
            ++TotalHashed;

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

    public async Task<string> CrackAsync(int count)
    {
        long piece = Total / count;

        var tasks = new Task[count];

        long from;
        long to = 0;

        for (var i = 0; i < count; i++)
        {
            from = to;
            to += piece;
            long from1 = from;
            long to1 = to;
            tasks[i] = Task.Run(() => Crack(from1, to1));
        }

        await Task.WhenAll(tasks);
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

    public static byte[] PrepareAlphabet(string input)
    {
        return Encoding.UTF8.GetBytes(input.ToCharArray());
    }

    private bool CmpBytes(byte[] arr1, byte[] arr2)
    {
        for (var i = 0; i < arr1.Length; i++)
        {
            if (arr1[i] != arr2[i]) return false;
        }

        return true;
    }
}
