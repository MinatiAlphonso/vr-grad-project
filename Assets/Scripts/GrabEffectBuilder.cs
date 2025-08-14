using UnityEngine;
using System.Collections.Generic;

public interface IHandEffect
{
    HandEffectType EffectType { get; }
    void Initialize(Transform t);
    bool OnGrab(Grab controller);
    bool OnRelease(Grab controller);
    bool OnHover(Grab controller);
    bool OnRemove(Grab controller);
    bool OnHaptics(Grab controller);
    bool OnDisappear(Grab controller);
}

public enum HandEffectType
{
    Grab, ColorHover, Haptics, Disappear
}

public class GrabEffectBuilder : GrabEffect
{
    [SerializeField]
    private List<ScriptableHandEffect> handEffects = new List<ScriptableHandEffect>();
    public List<ScriptableHandEffect> listHandEffects => handEffects;
    
    public HandEffectType selectedEffect { get; set; }

    public void Start()
    {
        foreach (var effect in handEffects)
        {
            effect.Initialize(transform);
        }
    }

    public void addEffect()
    {
        ScriptableHandEffect tempEffect = null;   
        switch (selectedEffect)
        {
            case HandEffectType.Grab:
                tempEffect = ScriptableObject.CreateInstance<GrabEffectHold>();
                break;
            case HandEffectType.ColorHover:
                tempEffect = ScriptableObject.CreateInstance<GrabEffectHover>();
                break;
            case HandEffectType.Haptics:
                tempEffect = ScriptableObject.CreateInstance<GrabEffectHaptics>();
                break;
            case HandEffectType.Disappear:
                tempEffect = ScriptableObject.CreateInstance<GrabEffectDisappear>();
                break;
        }

        if (tempEffect != null)
        {
            tempEffect.Initialize(transform);
            handEffects.Add(tempEffect);
            Debug.Log($"{selectedEffect} effect added.");
        }
        
    }

    public override bool OnGrab(Grab controller)
    {
        foreach (var effect in handEffects)
        {
            if (effect.OnGrab(controller))
                return true;
        }
        return false;
    }
    public override bool OnRelease(Grab controller)
    {
        foreach (var effect in handEffects)
        {
            if (effect.OnRelease(controller))
            {
                foreach(var effect2 in handEffects)
                {
                    if (effect2.OnDisappear(controller))
                    {
                        effect2.OnDisappear(controller);
                        break;
                    }
                }
                return true;
            }
        }
        return false;
    }

    public override bool OnHover(Grab controller)
    {
        foreach (var effect in handEffects)
        {
            if (effect.OnHover(controller))
                return true;
        }
        return false;
    }

    public override bool OnRemove(Grab controller)
    {
        foreach (var effect in handEffects)
        {
            if (effect.OnRemove(controller))
                return true;
        }
        return false;
    }

    public override bool OnHaptics(Grab controller) 
    {
        foreach(var effect in handEffects)
        {
            if (effect.OnHaptics(controller))
                return true;
        }
        return false;
    }
}