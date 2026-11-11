using FM.UI;
using FootballManager.Plugin.Data;
using FootballManager.Plugin.Writes;
using SI.Core;

namespace FootballManager.Plugin.Session;

// Presses the game's Continue button: raises the event the button raises (ContinueGameEvent, no fields), which the
// game's continue manager turns into the next step of its flow, as for a click: the next unread message, a screen of
// the user's continue flow, a match-day step, or processing the next days. What it did shows afterwards in the
// continue reference's State (root "continue") and the game's date.
internal static class ContinueButton
{
    public static async Task<string> PressAsync(bool confirmed, bool dryRun, TimeSpan timeout)
    {
        if (!dryRun)
        {
            await WriteGuard.EnsureAllowedAsync(confirmed, timeout);
        }

        await MainThreadDispatcher.RunAsync(() =>
        {
            if (!dryRun)
            {
                Subsystems.Get<EventSubsystem>().SendEvent(new ContinueGameEvent());
            }

            return true;
        });

        return Json.Serialize(new Dictionary<string, object?>
        {
            ["pressed"] = !dryRun,
            ["next"] = dryRun
                ? "Nothing was pressed. The button's state is /get?root=continue&props=State."
                : "Watch /get?root=continue&props=State: Processing while the game advances, then the next step."
        });
    }
}
