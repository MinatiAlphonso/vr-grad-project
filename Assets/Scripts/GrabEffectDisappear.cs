using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class GrabEffectDisappear : ScriptableHandEffect
{
    private Transform myTransform;
    public override HandEffectType EffectType => HandEffectType.Disappear;
    public override void Initialize(Transform t) { myTransform = t; }
    public override bool OnDisappear(Grab controller)
    {
        myTransform.gameObject.SetActive(false);
        return true;
    }
    public override bool OnGrab(Grab controller) { return false; }
    public override bool OnRelease(Grab controller) { return false; }
    public override bool OnHover(Grab controller) { return false; }
    public override bool OnRemove(Grab controller) { return false; }
    public override bool OnHaptics(Grab controller) { return false; }
}