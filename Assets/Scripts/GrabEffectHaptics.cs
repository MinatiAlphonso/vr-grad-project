using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

[System.Serializable]
public class GrabEffectHaptics : ScriptableHandEffect
{
    private Transform myTransform;

    [SerializeField]
    private uint channel = 0;
    [SerializeField]
    private float amplitude = 1;
    [SerializeField]
    private float duration = 0.01f;

    public override HandEffectType EffectType => HandEffectType.Haptics;
    public override void Initialize(Transform t) { myTransform = t; }
    public override bool OnHaptics(Grab controller)
    {
        //sanity check that the hand can grab something still
        if (controller.InHand != null)
            return false;
        if (controller.GetNearestGrabbable() == myTransform.gameObject)
        {
            InputDevice device = controller.SourceDevice;
            device.SendHapticImpulse(channel, amplitude, duration);
        }
        return true; 
    }
    public override bool OnGrab(Grab controller) {  return false; }
    public override bool OnRelease(Grab controller) { return false;}
    public override bool OnHover(Grab controller) { return false; }
    public override bool OnRemove(Grab controller) { return false;}
    public override bool OnDisappear(Grab controller) { return false; }
}
