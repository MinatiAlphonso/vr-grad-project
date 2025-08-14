using UnityEngine;

public abstract class ScriptableHandEffect : ScriptableObject, IHandEffect
{
    public virtual HandEffectType EffectType { get; }

    public virtual void Initialize(Transform t) { }
    public virtual bool OnDisappear(Grab controller) { return false; }
    public virtual bool OnGrab(Grab controller) { return false; }
    public virtual bool OnHaptics(Grab controller) { return false; }
    public virtual bool OnHover(Grab controller) { return false; }
    public virtual bool OnRelease(Grab controller) { return false; }
    public virtual bool OnRemove(Grab controller) { return false; }
}
