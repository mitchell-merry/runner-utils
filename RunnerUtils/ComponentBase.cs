using System.Collections.Generic;

namespace RunnerUtils;

public abstract class ComponentBase
{
    public static List<ComponentBase> components = [];
    
    public bool enabled;
    public abstract string Identifier { get; }
    public abstract bool ShowOnFairPlay { get; }

    public virtual void Toggle() {
        if (enabled)
        {
            Disable();
        }
        else
        {
            Enable();
        }
    }

    public virtual void Enable() {
        enabled = true;
    }

    public virtual void Disable() {
        enabled = false;
    }
}

public abstract class ComponentBase<T> : ComponentBase where T : ComponentBase<T>, new()
{
    public static T Instance { get; } = new();

    static ComponentBase() {
        components.Add(Instance);
    }
}
