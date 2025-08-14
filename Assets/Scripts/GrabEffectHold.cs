using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[System.Serializable]
public class GrabEffectHold : ScriptableHandEffect
{
    private Transform myTransform;
    public override HandEffectType EffectType => HandEffectType.Grab;
    protected Transform ParentOnRelease;
    private SnapRelease releaseScript;

    [field: SerializeField]
    public bool ApplyPhysicsOnRelease { get; set; } = true;

    [SerializeField]
    protected Vector3 rotationOffset;
    [SerializeField]
    protected Vector3 positionOffset;

    public override void Initialize(Transform t)
    {
        myTransform = t;
        ParentOnRelease = t.parent;
    }

    public void SetReleaseFunction(SnapRelease obj)
    {
        releaseScript = obj;
    }

    public override bool OnGrab(Grab controller)
    {
        //break old parenting and positioning
        myTransform.parent = null;

        //parent to controller, and place directly in hand
        myTransform.parent = controller.transform;
        myTransform.localPosition = positionOffset; //new Vector3(0, 0, 0);
        myTransform.localRotation = Quaternion.Euler(rotationOffset);//Quaternion.identity;

        //tell the grabber that their hand now has something
        controller.InHand = myTransform.gameObject;

        myTransform.GetComponent<Rigidbody>().isKinematic = true; // turn off physics

        return true;

    }

    public override bool OnRelease(Grab controller)
    {
        if (controller.InHand != myTransform.gameObject)
            return false;

        ResetController(controller);

        if (releaseScript != null)
        {
            releaseScript.Release(myTransform.gameObject);
        }
        else if (ApplyPhysicsOnRelease)
        {
            myTransform.GetComponent<Rigidbody>().isKinematic = !ApplyPhysicsOnRelease; // turn on physics
            ApplyPhysics(controller);
        }

        return true;
    }
    private void ApplyPhysics(Grab hand)
    {
        //give an average of physics of both hands
        Vector3 ave = Vector3.zero;
        Vector3 aveA = Vector3.zero;

        //get values from first hand
        hand.SourceDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out ave);
        hand.SourceDevice.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out aveA);

        //transfer force...if this is not supported, ave and aveA will be 0, and the object will just drop
        Rigidbody body = myTransform.GetComponent<Rigidbody>();
        body.linearVelocity = ave;
        body.angularVelocity = aveA;

        //get approximate vertical size
        Bounds bounds = myTransform.GetComponent<Renderer>().bounds;
        float height = bounds.extents.y / 2;

        //move out of range of the hand. If speed is too low just drop it
        if (ave.magnitude < 0.1f)
            myTransform.position = hand.transform.position + height * Vector3.down;
        else
            myTransform.position = hand.transform.position + height * ave;

        //estimate force by velocity over time
        float force = ave.magnitude / Time.fixedDeltaTime;
        Vector3 applyForce = force * ave.normalized;
        body.AddForce(applyForce, ForceMode.Force);
    }
    public void ResetController(Grab controller)
    {
        //release from grabber
        myTransform.parent = ParentOnRelease;

        //tell the grabber they are now able to grab another item
        controller.InHand = null;
    }

    public override bool OnHover(Grab controller) { return false; }
    public override bool OnRemove(Grab controller) { return false; }
    public override bool OnHaptics(Grab controller) { return false; }
    public override bool OnDisappear(Grab controller) { return false;}
}