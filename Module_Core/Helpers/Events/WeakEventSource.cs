using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MTM_Receiving_Application.Module_Core.Helpers.Events;

/// <summary>
/// Stores event subscriptions without strongly rooting instance targets.
/// </summary>
internal sealed class WeakEventSource
{
    private readonly List<WeakHandler> _handlers = new();
    private readonly object _syncRoot = new();

    public void Subscribe(EventHandler? handler)
    {
        if (handler is null)
        {
            return;
        }

        lock (_syncRoot)
        {
            _handlers.Add(new WeakHandler(handler.Method, handler.Target));
            CleanupDeadHandlers();
        }
    }

    public void Unsubscribe(EventHandler? handler)
    {
        if (handler is null)
        {
            return;
        }

        lock (_syncRoot)
        {
            _handlers.RemoveAll(existing => existing.Matches(handler.Method, handler.Target));
            CleanupDeadHandlers();
        }
    }

    public void Raise(object sender, EventArgs args)
    {
        foreach (var handler in GetLiveHandlers())
        {
            handler(sender, args);
        }
    }

    private List<EventHandler> GetLiveHandlers()
    {
        lock (_syncRoot)
        {
            var liveHandlers = _handlers
                .Select(handler => handler.TryCreate<EventHandler>())
                .Where(handler => handler is not null)
                .Cast<EventHandler>()
                .ToList();

            CleanupDeadHandlers();
            return liveHandlers;
        }
    }

    private void CleanupDeadHandlers()
    {
        _handlers.RemoveAll(handler => handler.IsDead);
    }
}

/// <summary>
/// Stores event subscriptions without strongly rooting instance targets.
/// </summary>
/// <typeparam name="TEventArgs">The event argument type.</typeparam>
internal sealed class WeakEventSource<TEventArgs>
{
    private readonly List<WeakHandler> _handlers = new();
    private readonly object _syncRoot = new();

    public void Subscribe(EventHandler<TEventArgs>? handler)
    {
        if (handler is null)
        {
            return;
        }

        lock (_syncRoot)
        {
            _handlers.Add(new WeakHandler(handler.Method, handler.Target));
            CleanupDeadHandlers();
        }
    }

    public void Unsubscribe(EventHandler<TEventArgs>? handler)
    {
        if (handler is null)
        {
            return;
        }

        lock (_syncRoot)
        {
            _handlers.RemoveAll(existing => existing.Matches(handler.Method, handler.Target));
            CleanupDeadHandlers();
        }
    }

    public void Raise(object sender, TEventArgs args)
    {
        foreach (var handler in GetLiveHandlers())
        {
            handler(sender, args);
        }
    }

    private List<EventHandler<TEventArgs>> GetLiveHandlers()
    {
        lock (_syncRoot)
        {
            var liveHandlers = _handlers
                .Select(handler => handler.TryCreate<EventHandler<TEventArgs>>())
                .Where(handler => handler is not null)
                .Cast<EventHandler<TEventArgs>>()
                .ToList();

            CleanupDeadHandlers();
            return liveHandlers;
        }
    }

    private void CleanupDeadHandlers()
    {
        _handlers.RemoveAll(handler => handler.IsDead);
    }
}

internal sealed class WeakHandler
{
    private readonly WeakReference<object>? _targetReference;

    public WeakHandler(MethodInfo method, object? target)
    {
        Method = method;
        if (target is not null)
        {
            _targetReference = new WeakReference<object>(target);
        }
    }

    public MethodInfo Method { get; }

    public bool IsDead => _targetReference?.TryGetTarget(out _) == false;

    public bool Matches(MethodInfo method, object? target)
    {
        if (Method != method)
        {
            return false;
        }

        if (_targetReference is null)
        {
            return target is null;
        }

        return _targetReference.TryGetTarget(out var existingTarget)
            && ReferenceEquals(existingTarget, target);
    }

    public TDelegate? TryCreate<TDelegate>()
        where TDelegate : Delegate
    {
        if (_targetReference is null)
        {
            return Delegate.CreateDelegate(typeof(TDelegate), Method) as TDelegate;
        }

        if (!_targetReference.TryGetTarget(out var target))
        {
            return null;
        }

        return Delegate.CreateDelegate(typeof(TDelegate), target, Method) as TDelegate;
    }
}
