using FootballManager.Plugin.Inspection;
using SI.Bindable;
using SI.Core;
using DateTime = System.DateTime;

namespace FootballManager.Plugin.Data;

// Asks the game for properties of one record (paths like "Name" or "Team.Players"), the way the UI does: each path is
// bound under a container node of ours, the native game answers over its data channels a few frames later, and the
// container is removed afterwards. Main thread only.
//
// Only the callbacks say a value arrived: the key Request returns isn't the node holding the value. A path the game
// can't answer (a null value, a missing context) never calls back, so once answers have stopped for a moment the rest
// are reported as pending.
internal sealed class PropertyRequest : IDisposable
{
    private const string ContainerPrefix = "MCPFM";

    private static readonly TimeSpan QuietPeriod = TimeSpan.FromMilliseconds(400);

    private readonly BindingSubsystem _bindings;
    private readonly Bindings.Key _container;
    private readonly IReadOnlyList<RequestContext> _contexts;
    private readonly Il2CppSystem.Object _owner = new();
    private readonly List<PropertyBinding> _bindingsByPath = [];
    private readonly object? _source;
    private bool _disposed;

    private PropertyRequest(BindingSubsystem bindings, Bindings.Key container, IReadOnlyList<RequestContext> contexts, object? source)
    {
        _bindings = bindings;
        _container = container;
        _contexts = contexts;
        _source = source;
    }

    public bool IsComplete
    {
        get
        {
            if (_bindingsByPath.All(b => b.Received || b.Error is not null))
            {
                return true;
            }

            var answered = _bindingsByPath.Where(b => b.Received).ToList();
            return answered.Count > 0 && DateTime.UtcNow - answered.Max(b => b.LastCallbackUtc) >= QuietPeriod;
        }
    }

    public static PropertyRequest Start(
        TypedValue? source,
        IReadOnlyList<string> paths,
        IReadOnlyList<RequestContext> contexts,
        IReadOnlyDictionary<string, PropertyCapture>? captures = null)
    {
        if (source is null)
        {
            throw new InvalidOperationException("The source record has no value yet.");
        }

        if (paths.Count == 0)
        {
            throw new ArgumentException("No properties requested (props=Name,Age,...).");
        }

        var bindings = GameBindings.Get();
        var container = BindingUtils.CreateUniqueChild(
            bindings, bindings.m_root.m_key, Il2CppSystem.MemoryExtensions.AsSpan(ContainerPrefix), Bindings.NodeFlags.None);
        var request = new PropertyRequest(bindings, container, contexts, TypedValueReader.Read(source));

        try
        {
            foreach (var path in paths)
            {
                request.Add(source, path, captures?.GetValueOrDefault(path) ?? PropertyCapture.None);
            }
        }
        catch
        {
            request.Dispose();
            throw;
        }

        return request;
    }

    // One request per (source, contexts) pair, e.g. one per player, or one per tactic position.
    public static List<PropertyRequest> StartAll(
        IEnumerable<(TypedValue Source, IReadOnlyList<RequestContext> Contexts)> requestsToStart, IReadOnlyList<string> paths)
    {
        var requests = new List<PropertyRequest>();

        try
        {
            foreach (var (source, contexts) in requestsToStart)
            {
                requests.Add(Start(source, paths, contexts));
            }
        }
        catch
        {
            requests.ForEach(r => r.Dispose());
            throw;
        }

        return requests;
    }

    public Dictionary<string, object?> Finish(bool debug)
    {
        try
        {
            return Results(debug);
        }
        finally
        {
            Dispose();
        }
    }

    // The bindings with whatever they captured, which stays valid once this request is gone.
    public IReadOnlyList<PropertyBinding> FinishWithCaptures()
    {
        Dispose();
        return _bindingsByPath;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Exception? failure = null;

        foreach (var binding in _bindingsByPath.Where(b => b.Callback is not null && b.Key.m_key != 0))
        {
            try
            {
                var key = binding.Key;
                _bindings.Unbind(ref key, binding.Callback);
            }
            catch (Exception exception)
            {
                failure ??= exception;
            }
        }

        // Removing the container takes every node and channel of the request with it.
        try
        {
            var container = _container;
            _bindings.Remove(ref container);
        }
        catch (Exception exception)
        {
            failure ??= exception;
        }

        if (failure is not null)
        {
            throw new InvalidOperationException("Cleaning up a property request failed.", failure);
        }
    }

    private void Add(TypedValue source, string path, PropertyCapture capture)
    {
        var binding = new PropertyBinding(path, capture);
        _bindingsByPath.Add(binding);

        var unknown = path.Split('.').FirstOrDefault(segment => !IdentifierCatalog.IsProperty(segment));

        if (unknown is not null)
        {
            binding.Error = $"'{unknown}' is not a game property (see /catalog?filter=).";
            return;
        }

        binding.Callback = new Action<Bindings.Key, TypedValue>(binding.OnValue);

        // Each binding gets its own copies: the tree may take ownership of a value and return it to the pool.
        if (_contexts.Count == 0)
        {
            binding.Key = BindingUtils.Request(
                _bindings, _container, TypedValue.CreateCopyFrom(source), path, _owner, binding.Callback);
            return;
        }

        var builder = BindingUtils.BuildRequest(_bindings, _container, TypedValue.CreateCopyFrom(source));

        foreach (var context in _contexts)
        {
            builder = builder.AddContext(
                Il2CppSystem.MemoryExtensions.AsSpan(context.Name), TypedValue.CreateCopyFrom(context.Value));
        }

        binding.Key = builder.Bind(Il2CppSystem.MemoryExtensions.AsSpan(path), _owner, binding.Callback);
    }

    private Dictionary<string, object?> Results(bool debug)
    {
        var values = new Dictionary<string, object?>();
        var pending = new List<string>();
        var errors = new Dictionary<string, string>();

        foreach (var binding in _bindingsByPath)
        {
            if (binding.Error is not null)
            {
                errors[binding.Path] = binding.Error;
            }
            else if (binding.Received)
            {
                values[binding.Path] = binding.Value;
            }
            else
            {
                pending.Add(binding.Path);
            }
        }

        var result = new Dictionary<string, object?> { ["source"] = _source, ["values"] = values };

        if (pending.Count > 0)
        {
            result["pending"] = pending;
        }

        if (errors.Count > 0)
        {
            result["errors"] = errors;
        }

        if (debug)
        {
            result["debug"] = _bindingsByPath
                .Select(b => new { path = b.Path, key = b.Key.m_key, callbacks = b.CallbackCount })
                .ToList();
        }

        return result;
    }
}
