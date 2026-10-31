using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballManager.Plugin.Inspection;

// Elements are addressed by path: "<document index>/<child index>/<child index>/...", as printed by /ui.
internal static class UiDocuments
{
    public static List<UIDocument> All()
    {
        // Sorted by instance id so document indices stay stable between calls.
        var found = UnityEngine.Object.FindObjectsByType(Il2CppType.Of<UIDocument>(), FindObjectsSortMode.InstanceID);
        var documents = new List<UIDocument>();

        foreach (var item in found)
        {
            documents.Add(item.Cast<UIDocument>());
        }

        return documents;
    }

    public static VisualElement Resolve(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

        if (segments.Count == 0)
        {
            throw new ArgumentException("An element path is required, e.g. path=0/2/1 (see /ui).");
        }

        var documents = All();

        if (segments[0] >= documents.Count)
        {
            throw new ArgumentException($"There are only {documents.Count} UI documents.");
        }

        var element = documents[segments[0]].rootVisualElement;

        foreach (var index in segments.Skip(1))
        {
            if (index >= element.hierarchy.childCount)
            {
                throw new ArgumentException($"'{element.name}' has only {element.hierarchy.childCount} children.");
            }

            element = element.hierarchy.ElementAt(index);
        }

        return element;
    }
}
