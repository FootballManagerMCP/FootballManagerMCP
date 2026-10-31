using System.Text;
using SI.Bindable;

namespace FootballManager.Plugin.Data;

// Lists binding nodes and their values: /bindings for the roots, path=Human.Team to start lower down.
internal static class BindingTreeDumper
{
    private const int MaxChildren = 400;
    private const int MaxValueLength = 200;

    public static string Dump(string? path, int depth)
    {
        var node = GameBindings.Get().m_root;

        if (!string.IsNullOrWhiteSpace(path))
        {
            foreach (var segment in path.Split('.'))
            {
                node = GameBindings.FindChild(node, segment)
                    ?? throw new ArgumentException($"'{node.m_name}' has no child binding named '{segment}'.");
            }
        }

        var output = new StringBuilder();
        Append(output, node, 0, depth);
        return output.ToString();
    }

    private static void Append(StringBuilder output, Bindings.Node node, int indent, int depth)
    {
        var children = Children(node).ToList();
        output.Append(' ', indent * 2)
            .Append(string.IsNullOrEmpty(node.m_name) ? "<root>" : node.m_name)
            .Append($" [{children.Count} children, flags {(byte)node.m_flags}]");

        var value = ReadValue(node);

        if (value != "null")
        {
            output.Append(" = ").Append(value.Length <= MaxValueLength ? value : value[..MaxValueLength] + "…");
        }

        output.AppendLine();

        if (depth <= 0)
        {
            return;
        }

        foreach (var child in children.Take(MaxChildren))
        {
            Append(output, child, indent + 1, depth - 1);
        }

        if (children.Count > MaxChildren)
        {
            output.Append(' ', (indent + 1) * 2).AppendLine($"… {children.Count - MaxChildren} more");
        }
    }

    private static string ReadValue(Bindings.Node node)
    {
        try
        {
            return Json.Compact(TypedValueReader.Read(GameBindings.ValueOf(node)));
        }
        catch (Exception exception)
        {
            return $"<unreadable: {exception.Message.Split('\n')[0]}>";
        }
    }

    private static IEnumerable<Bindings.Node> Children(Bindings.Node node)
    {
        for (var child = node.m_firstChild; child is not null; child = child.m_nextSibling)
        {
            yield return child;
        }
    }
}
