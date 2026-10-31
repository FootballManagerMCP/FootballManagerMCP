using SI.Bindable.Reference.Core;
using ContextList = Il2CppSystem.Collections.Generic.List<SI.Bindable.Reference.Core.ContextID>;
using PropertyList = Il2CppSystem.Collections.Generic.List<SI.Bindable.Reference.Core.PropertyID>;

namespace FootballManager.Plugin.Data;

// The game's per-record-type property metadata, behind one shape for PropertySchema.
internal sealed record SchemaSource(
    Action<PropertyList> GetProperties,
    Func<uint, BindingKind> GetPropertyType,
    Action<uint, ContextList> GetContexts,
    Func<uint, string>? GetDescription);
