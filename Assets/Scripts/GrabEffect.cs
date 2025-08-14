using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for the grab scripts that grabbing should notify
/// </summary>
public abstract class GrabEffect : MonoBehaviour
{
    //[field: SerializeField]
    //public int Priority { get; set; } = 0;

    public void OnTriggerEnter(Collider other)
    {
        Grab g = other.GetComponent<Grab>();
        if (g != null)
        {
            //found an object that triggers grab, register
            Debug.Log(g.name + " is in range");
            g.RegisterGrab(this);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Grab g = other.GetComponent<Grab>();
        if (g != null)
        {
            //left area that allows grab, deregister
            Debug.Log(g.name + " is NOT in range");
            g.UnregisterGrab(this);

        }
    }
    //public virtual HandEffectType Type() { return HandEffectType.None; }
    public virtual bool OnGrab(Grab controller) { return false; }
    public virtual  bool OnRelease(Grab controller) { return false; }
    public virtual void ResetController(Grab controller) { }
    public virtual bool OnHover(Grab controller) { return false; }
    public virtual bool OnRemove(Grab controller) { return false; }
    public virtual bool OnAdd(Grab controller) { return false; }
    public virtual bool OnHaptics(Grab controller) { return false; }
    public virtual bool OnDisappear(Grab controller) { return false; }
}