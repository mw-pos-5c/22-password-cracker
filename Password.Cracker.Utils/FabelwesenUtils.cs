#region usings

using System.Linq;

using HtmlAgilityPack;

#endregion

namespace Password.Cracker.Utils;

public class FabelwesenUtils
{
    public static string[] GetFabelwesen()
    {
        var web = new HtmlWeb();
        HtmlDocument doc = web.Load("https://de.wikipedia.org/wiki/Liste_von_Fabelwesen");
        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@id=\"mw-content-text\"]/div/ul/li/a");
        return nodes.Select(node => node.InnerHtml).ToArray();
    }
}
