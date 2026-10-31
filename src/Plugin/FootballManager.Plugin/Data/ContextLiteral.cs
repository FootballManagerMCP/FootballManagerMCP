using System.Globalization;
using SI.Core;

namespace FootballManager.Plugin.Data;

// A context given in a query as Name:value. The value's type follows its text: true/false, a whole number, or text
// (put it in single quotes to force text, e.g. format:'[%date#1]').
internal sealed record ContextLiteral(string Name, object Value)
{
    public static ContextLiteral Parse(string text)
    {
        var separator = text.IndexOf(':');

        if (separator <= 0)
        {
            throw new ArgumentException($"A context is Name:value, got '{text}'.");
        }

        var name = text[..separator];
        var raw = text[(separator + 1)..];

        if (raw.Length >= 2 && raw[0] == '\'' && raw[^1] == '\'')
        {
            return new ContextLiteral(name, raw[1..^1]);
        }

        if (bool.TryParse(raw, out var flag))
        {
            return new ContextLiteral(name, flag);
        }

        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            return new ContextLiteral(name, Narrowest(number));
        }

        return new ContextLiteral(name, raw);
    }

    // Main thread only: creates the game's value through its own factory.
    public RequestContext ToRequestContext()
    {
        var value = Value switch
        {
            bool flag => TypedValue.Create(flag),
            int number => TypedValue.Create(number),
            uint number => TypedValue.Create(number),
            long number => TypedValue.Create(number),
            string text => TypedValue.Create(text),
            _ => throw new InvalidOperationException($"Unsupported context value for '{Name}'.")
        };

        return new RequestContext(Name, value);
    }

    // Game ids run up to the full UInt32 range, past what an Int32 holds.
    private static object Narrowest(long number)
    {
        if (number is >= int.MinValue and <= int.MaxValue)
        {
            return (int)number;
        }

        return number is >= 0 and <= uint.MaxValue ? (uint)number : number;
    }
}
