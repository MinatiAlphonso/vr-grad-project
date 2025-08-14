using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

public class Grab : MonoBehaviour
{
    private UnityEngine.XR.InputDevice sourceDevice;
    public UnityEngine.XR.InputDevice SourceDevice { get => sourceDevice; }

    public enum Hand { NEITHER, LEFT, RIGHT }

    [ReadOnly]
    [field: SerializeField]
    private Hand whichHand;
    public Hand WhichHand { get { return whichHand; } }

    [field: ReadOnly]
    [field: SerializeField]
    public GameObject InHand { set; get; } = null;

    [SerializeField]
    private float maxDistance = 1f;

    [SerializeField]
    private InputActionProperty grabAction;
    [SerializeField]
    private InputActionProperty releaseAction;

    [ReadOnly]
    [SerializeField]
    protected List<GrabEffect> grabObjects = new List<GrabEffect>();

    // Start is called before the first frame update
    void Start()
    {
        DetermineHand();
        //Debug.Log($"Current Hand: {whichHand}");

        //sanity check, if the action does not exists, do not save it
        if (grabAction == null || releaseAction == null)
        {
            Debug.LogWarning("Need both grab and release actions to work. Grabbing is disabled.");

            //safety catch, if no player input there should be no actions
            return;
        }
        grabAction.action.started += OnGrab;
        releaseAction.action.performed += OnRelease;
    }

    /// <summary>
    /// Helper function to determine the hand
    /// </summary>
    private void DetermineHand()
    {
        whichHand = Hand.NEITHER;

        //if not XR, try other ways
        if (whichHand == Hand.NEITHER)
        {
            TrackedPoseDriver driver = GetComponent<TrackedPoseDriver>();
            if (tag.ToLower().Contains("left")
              || name.ToLower().Contains("left")
              || (driver != null && driver.poseSource == TrackedPoseDriver.TrackedPose.LeftPose))
            {
                whichHand = Hand.LEFT;
            }
            else if (tag.ToLower().Contains("right")
              || name.ToLower().Contains("right")
              || (driver != null && driver.poseSource == TrackedPoseDriver.TrackedPose.RightPose))
            {
                whichHand = Hand.RIGHT;
            }
        }
    }

    /// <summary>
    /// Draw the Debugging Gizmos
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red; 
        if (whichHand == Hand.LEFT)
        {
            Gizmos.color = new Color(0, 0, 1, 0.25f);
        }
        else if (whichHand == Hand.RIGHT)
        {
            Gizmos.color = new Color(1, 0, 0, 0.25f);
        }
        else
        {
            Gizmos.color = new Color(1, 1, 1, 0.25f); // translucent white
        }

        Gizmos.DrawSphere(transform.position, GetComponent<SphereCollider>().radius);
    }

    /// <summary>
    /// Helper function to pull the VR controller from the context
    /// </summary>
    /// <param name="context"></param>
    private void SetController(InputAction.CallbackContext context)
    {
        //get left\right controller that caused the action
        UnityEngine.InputSystem.InputDevice device = context.control.device;
        List<UnityEngine.XR.InputDevice> inputDevices = new List<UnityEngine.XR.InputDevice>();

        if (device.usages.Contains(UnityEngine.InputSystem.CommonUsages.LeftHand))
        {
            InputDevices.GetDevicesWithCharacteristics(
              InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left, inputDevices);
        }
        else if (device.usages.Contains(UnityEngine.InputSystem.CommonUsages.RightHand))
        {
            InputDevices.GetDevicesWithCharacteristics(
              InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right, inputDevices);
        }
        sourceDevice = inputDevices.FirstOrDefault();

    }

    /// <summary>
    /// Grab action occured.
    /// Get list of objects that need to be notified in preference order.
    /// </summary>
    public void OnGrab(InputAction.CallbackContext context)
    {
        SetController(context);

        //sanity check, do not grab if hands are full
        if (InHand == null)
        {
            //check for all the objects that are to be notified, but default, this is just one
            GrabEffect[] list = GetGrabCallbackSet();
            //Array.Sort(list, (firstObj, secondObj) =>
            //{
            //    return firstObj.Priority - secondObj.Priority;
            //});

            foreach (GrabEffect go in list)
            {
                //run effect only if still good
                if (go != null && go.enabled)
                {
                    go.OnGrab(this);
                }
            }
        }
    }

    /// <summary>
    /// Release action occurred. Notify objects who want to know and are in the active list.
    /// It does NOT make the hand open, in cases where releasing should be disallowed
    /// </summary>
    public void OnRelease(InputAction.CallbackContext context)
    {
        //safety check. If a release occurs with nothing in the hand,
        //make sure everything is released
        if (InHand == null)
        {
            ResetHand();
        }

        //check for all the objects that are to be notified, but default, this is just one
        GrabEffect[] list = GetReleaseCallbackSet();

        foreach (GrabEffect go in list)
        {
            go.OnRelease(this);
        }
    }

    public void RegisterGrab(GrabEffect item)
    {
        //disallow duplicates
        if (!grabObjects.Contains(item))
        {
            grabObjects.Add(item);
            item.OnAdd(this);
        }
    }

    public void UnregisterGrab(GrabEffect item)
    {
        if (grabObjects.Remove(item))
        {
            item.OnRemove(this);
        }
    }

    /// <summary>
    /// Helper function to remove elements from the list if they have someone 
    /// been disabled or removed since the last grab/release event
    /// </summary>
    protected virtual void CleanUp()
    {
        List<GrabEffect> toRemove = new List<GrabEffect>();

        //find any grabbable game object that is no longer active or has been destroyed
        foreach (GrabEffect obj in grabObjects)
        {
            if (obj == null
                || !obj.gameObject.activeSelf
                || Vector3.Distance(obj.gameObject.transform.position, transform.position) > maxDistance)
            {
                toRemove.Add(obj);
            }
        }

        //knock item out of hand if too far
        if (InHand != null && Vector3.Distance(InHand.gameObject.transform.position, transform.position) > maxDistance)
        {
            ResetHand();
            InHand = null;
        }

        //remove found items from the list
        foreach (GrabEffect obj in toRemove)
        {
            if (obj == InHand)
            {
                ResetHand(); //item was somehow disabled in the hand
                InHand = null;
            }

            if (grabObjects.Remove(obj))
            {
                obj.OnRemove(this);
            }
        }
    }

    //[SerializeField]
    //private Animator handAnimator;
    //private bool wasGrabbing = false;

    /// <summary>
    /// Run hover update as needed
    /// </summary>
    public void Update()
    {
        CleanUp();

        grabObjects.Sort((firstObj, secondObj) =>
        {
            return DistanceOrder(firstObj, secondObj);
        });

        foreach (GrabEffect g in grabObjects)
        {
            if (g.gameObject != InHand)
            {
                g.OnHover(this);
                g.OnHaptics(this);
            }
        }


    }

    private int DistanceOrder(GrabEffect firstObj, GrabEffect secondObj)
    {
        float distanceFirst = (transform.position - firstObj.transform.position).magnitude;
        float distanceSecond = (transform.position - secondObj.transform.position).magnitude;
        if (distanceFirst > distanceSecond) { return 1; }
        else if (distanceFirst < distanceSecond) { return -1; }
        else { return 0; }
    }

    /// <summary>
    /// Get the closet legal grabbable item
    /// </summary>
    /// <returns>Returns the nearest object with a GrabEffect, 
    /// or null if there are none</returns>
    public GameObject GetNearestGrabbable()
    {

        //only works because the list should be sorted each update
        if (grabObjects.Count > 0 && CanGrab(grabObjects[0].gameObject))
        {
            return grabObjects[0].gameObject;

        }
        return null;
    }

    /// <summary>
    /// Answers if the object can be grabbed in the current context
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>True is the object should allow a grab if choosen</returns>
    public virtual bool CanGrab(GameObject obj)
    {
        //Disallow grab is hand is full
        if (InHand == null)
        {
            //get ray from hand to object
            Vector3 lineTo = obj.transform.position - transform.position;
            Ray r = new Ray(transform.position, lineTo);
            RaycastHit hit;
            int layerMask = 1 << 2; //skip the ignore raycast layer
            Physics.Raycast(r, out hit, lineTo.magnitude, ~layerMask);

            //if no hit, nothing in between, but also ignore self-collisions
            if (hit.collider == null || hit.collider.gameObject == obj.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets a list of objects to be notified of a grab callbacks. 
    /// The default just uses ALL grab effects of the closest object
    /// </summary>
    /// <returns>The set of effects to call</returns>
    protected virtual GrabEffect[] GetGrabCallbackSet()
    {
        GameObject closest = GetNearestGrabbable();

        if (closest == null)
            return new GrabEffect[0];
        else
            return closest.GetComponents<GrabEffect>();
    }

    /// <summary>
    /// Gets a list of objects to be notified of a release callbacks. 
    /// The default just uses ALL grab effects of what is in the hand
    /// </summary>
    /// <returns></returns>
    protected virtual GrabEffect[] GetReleaseCallbackSet()
    {
        if (InHand == null)
            return new GrabEffect[0];
        else
            return InHand.GetComponents<GrabEffect>();
    }

    /// <summary>
    /// Resets the hand to a non-grabbed state, by called reset() on all
    /// the effects
    /// </summary>
    public void ResetHand()
    {
        if (InHand == null)
        {
            Debug.Log("Nothing in hand."); // get rid of error when displaying menu
            return;
        }

        GrabEffect[] effectsInHand = InHand.GetComponents<GrabEffect>();
        foreach (GrabEffect go in effectsInHand)
        {
            go.ResetController(this);
        }
    }
}