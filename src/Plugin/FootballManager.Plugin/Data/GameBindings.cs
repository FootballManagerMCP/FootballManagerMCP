using FM.UI;
using SI.Bindable;
using SI.Core;

namespace FootballManager.Plugin.Data;

// The game's binding tree: every value the UI shows is a node in it, filled by the native game over data channels.
// Root-level nodes hold game-wide records the UI paths start from, e.g. "Human" (the user's manager).
internal static class GameBindings
{
    public static BindingSubsystem Get()
    {
        return EmbeddedDataHandler.s_bindingSubsystem
            ?? throw new InvalidOperationException("The game's binding subsystem isn't up yet (still loading?).");
    }

    public static Bindings.Node FindRoot(string name)
    {
        return FindChild(Get().m_root, name)
            ?? throw new ArgumentException($"No root binding named '{name}'. See /bindings for the available ones.");
    }

    public static Bindings.Node? FindChild(Bindings.Node parent, string name)
    {
        for (var node = parent.m_firstChild; node is not null; node = node.m_nextSibling)
        {
            if (string.Equals(node.m_name, name, StringComparison.OrdinalIgnoreCase))
            {
                return node;
            }
        }

        return null;
    }

    public static TypedValue? ValueOf(Bindings.Node node)
    {
        var key = node.m_key;
        return Get().Get(ref key);
    }
}
