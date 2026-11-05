namespace FootballManager.Plugin.Writes;

// What a recording catches besides the tactics events the recorder listens to by type:
//   Interop  native events (and the switch for everything below)
//   Writes   binding writes, sent one by one or batched
//   Ui       every UI event, by class, with its fields
//   Data     the screens' data traffic (SetData...), kept apart in its own larger buffer
// The hooks behind them go in the first time they're asked for and stay; these only switch what they record.
internal sealed record RecorderOptions(bool Interop = true, bool Writes = true, bool Ui = true, bool Data = false);
