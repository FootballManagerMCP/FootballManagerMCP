using System.Globalization;
using System.Text;

namespace FootballManager.Application.Text;

// Names as the game writes them, in its display language, compared the way a person would type them.
internal static class GameNames
{
    // Lower case without accents: "Atlético" and "atletico" are the same name.
    public static string Normalise(string name)
    {
        var decomposed = name.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var letters = decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(letters.ToArray()).Normalize(NormalizationForm.FormC);
    }
}
