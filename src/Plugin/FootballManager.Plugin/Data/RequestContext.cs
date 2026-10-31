using SI.Core;

namespace FootballManager.Plugin.Data;

// A value some properties need besides their record, e.g. "human" (whose knowledge an attribute is seen through) or
// "team" (whose squad morale is measured in). Names are the game's context identifiers.
internal sealed record RequestContext(string Name, TypedValue Value);
