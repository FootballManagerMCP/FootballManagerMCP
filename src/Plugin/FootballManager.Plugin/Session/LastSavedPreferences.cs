using BepInEx;

namespace FootballManager.Plugin.Session;

// The game's most recent save (what the start screen's "load most recent" opens) lives in small preference files it
// rewrites whenever a save is loaded or saved: Preferences/version N/last_saved_game.xml and its siblings, under the
// user's local app data. Loading the investigation save points them at it, so the user's next "load most recent" would
// open the copy. They're kept aside before a load and put back before quitting, or when the game next starts if it
// didn't quit through the bridge.
internal static class LastSavedPreferences
{
    private const string FilePattern = "last_save*.xml";

    private static readonly string PreferencesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sports Interactive", "Football Manager 27", "Preferences");

    private static readonly string KeptFolder = Path.Combine(Paths.ConfigPath, "MCPFM", "last-saved-preferences");

    public static bool AreKept => Directory.Exists(KeptFolder) && Directory.EnumerateFiles(KeptFolder, "*", SearchOption.AllDirectories).Any();

    // Keeps the files as they are now. A set kept earlier and not put back yet stays as it is: it holds the user's own
    // most recent save, while the files may name the investigation save by now.
    public static IReadOnlyList<string> Keep(string investigationSave)
    {
        if (AreKept)
        {
            return Kept();
        }

        var files = Directory.Exists(PreferencesFolder)
            ? Directory.EnumerateFiles(PreferencesFolder, FilePattern, SearchOption.AllDirectories).ToList()
            : [];

        if (files.Count == 0)
        {
            throw new InvalidOperationException(
                $"Found no {FilePattern} under {PreferencesFolder}, so the user's most recent save couldn't be kept; not loading.");
        }

        var naming = files.FirstOrDefault(file => File.ReadAllText(file).Contains($"{investigationSave}.fm\"", StringComparison.OrdinalIgnoreCase));

        if (naming is not null)
        {
            throw new ArgumentException(
                $"The game's most recent save is already the investigation save ({Relative(naming)}), so the user's own can't be kept. "
                + "The user needs to load their own save once first.");
        }

        foreach (var file in files)
        {
            var kept = Path.Combine(KeptFolder, Relative(file));
            Directory.CreateDirectory(Path.GetDirectoryName(kept)!);
            File.Copy(file, kept, overwrite: true);
        }

        return Kept();
    }

    // Puts the kept files back and forgets them; with nothing kept, does nothing.
    public static IReadOnlyList<string> Restore()
    {
        if (!AreKept)
        {
            return [];
        }

        var restored = Kept();

        foreach (var relative in restored)
        {
            var target = Path.Combine(PreferencesFolder, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(Path.Combine(KeptFolder, relative), target, overwrite: true);
        }

        Directory.Delete(KeptFolder, recursive: true);
        return restored;
    }

    private static List<string> Kept()
    {
        return Directory.EnumerateFiles(KeptFolder, "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(KeptFolder, file))
            .ToList();
    }

    private static string Relative(string file)
    {
        return Path.GetRelativePath(PreferencesFolder, file);
    }
}
