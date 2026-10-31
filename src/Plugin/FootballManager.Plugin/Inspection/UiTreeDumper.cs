using System.Text;
using UnityEngine.UIElements;

namespace FootballManager.Plugin.Inspection;

internal static class UiTreeDumper
{
    private const int MaxTextLength = 80;

    public static string Dump(bool visibleOnly, int maxDepth)
    {
        var output = new StringBuilder();
        var documents = UiDocuments.All();

        for (var i = 0; i < documents.Count; i++)
        {
            var document = documents[i];
            output.AppendLine($"[{i}] UIDocument on '{document.gameObject.name}' (active: {document.isActiveAndEnabled})");
            AppendTree(output, document.rootVisualElement, i.ToString(), 1, visibleOnly, maxDepth);
        }

        return output.ToString();
    }

    public static string Find(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "Pass the text to look for, e.g. /find?text=Squad";
        }

        var output = new StringBuilder();
        var documents = UiDocuments.All();

        for (var i = 0; i < documents.Count; i++)
        {
            AppendMatches(output, documents[i].rootVisualElement, i.ToString(), text);
        }

        return output.Length == 0 ? $"No element shows text containing '{text}'." : output.ToString();
    }

    private static void AppendTree(StringBuilder output, VisualElement element, string path, int depth, bool visibleOnly, int maxDepth)
    {
        if (visibleOnly && !IsVisible(element))
        {
            return;
        }

        output.Append(' ', depth * 2).AppendLine(Describe(element, path));

        if (depth >= maxDepth)
        {
            return;
        }

        var hierarchy = element.hierarchy;

        for (var i = 0; i < hierarchy.childCount; i++)
        {
            AppendTree(output, hierarchy.ElementAt(i), $"{path}/{i}", depth + 1, visibleOnly, maxDepth);
        }
    }

    private static void AppendMatches(StringBuilder output, VisualElement element, string path, string text)
    {
        var elementText = TextOf(element);

        if (elementText is not null && elementText.Contains(text, StringComparison.OrdinalIgnoreCase))
        {
            output.AppendLine(Describe(element, path));
            output.Append("    in: ").AppendLine(AncestorNames(element));
        }

        var hierarchy = element.hierarchy;

        for (var i = 0; i < hierarchy.childCount; i++)
        {
            AppendMatches(output, hierarchy.ElementAt(i), $"{path}/{i}", text);
        }
    }

    private static string Describe(VisualElement element, string path)
    {
        var description = new StringBuilder($"{path} {element.GetIl2CppType().Name}");

        if (!string.IsNullOrEmpty(element.name))
        {
            description.Append('#').Append(element.name);
        }

        var classes = element.classList;

        for (var i = 0; i < classes.Count; i++)
        {
            description.Append(" .").Append(classes[i]);
        }

        var text = TextOf(element);

        if (!string.IsNullOrWhiteSpace(text))
        {
            description.Append(" \"").Append(Truncate(RichText.Strip(text))).Append('"');

            foreach (var link in RichText.Links(text))
            {
                description.Append(" link=").Append(link);
            }
        }

        var bounds = element.worldBound;
        description.Append($" @{bounds.x:0},{bounds.y:0} {bounds.width:0}x{bounds.height:0}");
        return description.ToString();
    }

    private static string AncestorNames(VisualElement element)
    {
        var names = new List<string>();

        for (var parent = element.hierarchy.parent; parent is not null; parent = parent.hierarchy.parent)
        {
            names.Add(string.IsNullOrEmpty(parent.name) ? parent.GetIl2CppType().Name : parent.name);
        }

        names.Reverse();
        return string.Join(" > ", names);
    }

    public static bool IsVisible(VisualElement element)
    {
        var bounds = element.worldBound;
        return element.visible && bounds.width > 0 && bounds.height > 0;
    }

    public static string? TextOf(VisualElement element)
    {
        return element.TryCast<TextElement>()?.text;
    }

    public static IEnumerable<VisualElement> Descendants(VisualElement element)
    {
        var hierarchy = element.hierarchy;

        for (var i = 0; i < hierarchy.childCount; i++)
        {
            var child = hierarchy.ElementAt(i);
            yield return child;

            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static string Truncate(string text)
    {
        var singleLine = text.Replace('\n', ' ').Replace('\r', ' ');
        return singleLine.Length <= MaxTextLength ? singleLine : singleLine[..MaxTextLength] + "…";
    }
}
