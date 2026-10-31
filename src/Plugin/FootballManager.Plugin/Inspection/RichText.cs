using System.Text.RegularExpressions;

namespace FootballManager.Plugin.Inspection;

// FM renders names and values as TextMeshPro-style rich text: <color=…><link="…">Name</link></color>. Bound string
// values carry their link differently, as \u0001<encoded link>\u0002Name\u0001.
internal static class RichText
{
    private static readonly Regex Tag = new("<[^>]*>");
    private static readonly Regex Link = new("<link=\"?([^\">]+)\"?>");
    private static readonly Regex RecordLink = new("\u0001[^\u0001\u0002]*\u0002");

    public static string Strip(string text)
    {
        return Tag.Replace(text, "").Replace('"', '\'');
    }

    public static string StripRecordLinks(string text)
    {
        return RecordLink.Replace(text, "").Replace("\u0001", "");
    }

    public static List<string> Links(string text)
    {
        return Link.Matches(text).Select(m => m.Groups[1].Value).ToList();
    }
}
