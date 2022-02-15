#region usings

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Password.Cracker.Utils;

#endregion

// ReSharper disable UnusedVariable

namespace Password.Cracker.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

        byte[] pw0 = CrackUtils.HexStringToByte("A746222F09D85605C52D4E636788D6FFDC274698B98B8C5F3244C06958683A69");
        byte[] pw1 = CrackUtils.HexStringToByte("3086E346353248775A2C5D74E36A9C9B9BD226A1EE401F830AC499633DC00031");
        byte[] pw2 = CrackUtils.HexStringToByte("26775436073E00D207E192857EE3730CFCA19DE16F01F0780096EF217C2919EF");
        byte[] pw3 = CrackUtils.HexStringToByte("43C19A093B34B581DDCC7207F6BD094F6940DB69F035C444425ED84D2CAC037D");

        int length;
        byte[] alphabet;
        byte[] pw;

        // pw0 - snow
        //length = 4;
        //alphabet = CrackUtils.GetBytes("abcdefghijklmnopqrstuvwxyz");
        //pw = pw0;

        // pw1 - HTLGKR
        //length = 6;
        //alphabet = CrackUtils.GetBytes("abcdefghijklmnopqrstuvwxyz".ToUpper());
        //pw = pw1;

        // p2 - CoV19
        length = 5;
        alphabet = CrackUtils.GetBytes("abcdefghijklmnopqrstuvwxyz".ToUpper() + "abcdefghijklmnopqrstuvwxyz" + "0123456789");
        pw = pw2;
        
        int threadCount = Environment.ProcessorCount+1;
        var crack = new CrackUtils(alphabet, length, pw, threadCount);

        //crack.Crack(0, 100, out int x);

        
        Task.Run(() =>
        {
            long last = 0;

            Process proc = Process.GetCurrentProcess();

            while (!crack.Found)
            {
                long now = crack.TotalHashed;
                long diff = now - last;
                last = now;
                Console.WriteLine($"Status: {now} | {diff} | MEM " + ((double) proc.PrivateMemorySize64 / 1000000));
                Thread.Sleep(1000);
            }
        });
        
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        crack.CrackAsync();
        string result = crack.GetResultAsync().Result;
        stopwatch.Stop();


        
        Console.WriteLine("Result: " + result);

        Console.WriteLine("Took: " + stopwatch.ElapsedMilliseconds + " ms");

        Console.WriteLine(crack.Total);
        Console.WriteLine(crack.TotalHashed);
        


        //Console.WriteLine(crackFabel.WordlistCrack(FabelwesenUtils.GetFabelwesen(), pw3));
    }
}
