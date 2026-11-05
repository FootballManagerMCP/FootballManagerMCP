using Il2CppInterop.Runtime.InteropTypes;
using SI.Core;

namespace FootballManager.Plugin.Writes;

// The game's registered subsystems, e.g. its UI event system (which its screens raise events on and its handler modules
// listen to) and its interop subsystem (which passes events and binding writes on to the native game).
internal static class Subsystems
{
    public static T Get<T>() where T : Il2CppObjectBase
    {
        var subsystems = GameSubsystems.s_subsystems
            ?? throw new InvalidOperationException("The game's subsystems aren't registered yet (still loading?).");

        for (var i = 0; i < subsystems.Count; i++)
        {
            var subsystem = subsystems[i]?.TryCast<T>();

            if (subsystem is not null)
            {
                return subsystem;
            }
        }

        throw new InvalidOperationException($"The game has no {typeof(T).Name} registered.");
    }
}
