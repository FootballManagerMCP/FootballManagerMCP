using System.Text;
using FootballManager.Plugin.Data;
using SI.Bindable;
using SI.Core;
using UnityEngine.UIElements;

namespace FootballManager.Plugin.Inspection;

// Reads an on-screen StreamedTable: its column headers, the rows currently built (the list is virtualised, so only
// rows near the viewport exist as elements) and the data items behind the rows.
internal static class TableReader
{
    private const int MaxDescribedItems = 50;

    public static string Read(string? path)
    {
        var table = string.IsNullOrWhiteSpace(path) ? FindFirstTable() : UiDocuments.Resolve(path);

        if (table is null)
        {
            return "No StreamedTable on screen.";
        }

        var output = new StringBuilder();
        output.AppendLine($"table: {table.GetIl2CppType().Name}#{table.name}");

        var headers = UiTreeDumper.Descendants(table)
            .Where(e => e.ClassListContains("streamed-table__header"))
            .OrderBy(e => e.worldBound.x)
            .Select(CellText);
        output.AppendLine("columns: " + string.Join(" | ", headers));

        var rows = UiTreeDumper.Descendants(table)
            .Where(e => e.ClassListContains("streamed-table__row") && UiTreeDumper.IsVisible(e))
            .OrderBy(e => e.worldBound.y);

        foreach (var row in rows)
        {
            var cells = UiTreeDumper.Descendants(row)
                .Where(e => e.ClassListContains("streamed-table__cell"))
                .OrderBy(e => e.worldBound.x)
                .Select(CellText);
            output.AppendLine("row: " + string.Join(" | ", cells));
        }

        AppendItems(output, table);
        return output.ToString();
    }

    private static VisualElement? FindFirstTable()
    {
        foreach (var document in UiDocuments.All())
        {
            var table = UiTreeDumper.Descendants(document.rootVisualElement)
                .FirstOrDefault(e => e.GetIl2CppType().Name == "StreamedTable" && UiTreeDumper.IsVisible(e));

            if (table is not null)
            {
                return table;
            }
        }

        return null;
    }

    private static string CellText(VisualElement cell)
    {
        var parts = new List<string>();

        foreach (var element in UiTreeDumper.Descendants(cell).Prepend(cell))
        {
            var text = UiTreeDumper.TextOf(element);

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var links = RichText.Links(text);
            var plain = RichText.Strip(text).Trim();
            parts.Add(links.Count == 0 ? plain : $"{plain} [{string.Join(", ", links)}]");
        }

        return string.Join(" ", parts);
    }

    // The records behind the rows (all of them, not just the built rows).
    private static void AppendItems(StringBuilder output, VisualElement table)
    {
        var items = table.TryCast<StreamedListView>()?._SourceData_k__BackingField
            ?.TryCast<Il2CppSystem.Collections.Generic.List<TypedValue>>();

        if (items is null)
        {
            output.AppendLine("items: none");
            return;
        }

        output.AppendLine($"items: {items.Count}");

        for (var i = 0; i < Math.Min(items.Count, MaxDescribedItems); i++)
        {
            output.AppendLine($"  [{i}] {Json.Compact(TypedValueReader.Read(items[i]))}");
        }
    }
}
