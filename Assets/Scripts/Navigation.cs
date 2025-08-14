using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Navigation : MonoBehaviour
{
    [SerializeField]
    private GameObject feet;
    [SerializeField]
    private GameObject head;

    // flag to disallow doubling up of teleportation commands
    private bool isMoving = false;
    public bool IsMoving { get => isMoving; }

    // where to try to move 
    private Vector3 movementTarget;

    // super speed parameters
    [SerializeField]
    private float teleportTime = 0.1f;
    private float startTime = 0;
    private Vector3 startLoc = Vector3.zero;

    [SerializeField]
    private int maxSteepness = 20;

    [SerializeField]
    [Tooltip("if left at zero, the head's collider's distance will be used.")]
    private float minDistanceFromWall = 0;

    [SerializeField]
    private float stepDistance = 0.2f;

    [SerializeField]
    private int correctionIterations = 8;

    [SerializeField]
    private float turnSpeed = 20f;

    [SerializeField]
    public float fallBuffer = 0.05f;

    private GameObject lastPlatform;
    private Vector3 lastPlatformLoc;

    void Start()
    {
        // sanity check. Nothing closer than the head
        if (minDistanceFromWall < head.GetComponent<SphereCollider>().radius)
            minDistanceFromWall = head.GetComponent<SphereCollider>().radius;

    }

    private List<XRNodeState> nodeStates = new List<XRNodeState>();
    public void Update()
    {
        if (isMoving)
        {
            OnMoving();
        }
        else
        {
            InputTracking.GetNodeStates(nodeStates);
            XRNodeState headState = nodeStates.FirstOrDefault(node => node.nodeType == XRNode.Head);
            if (headState.nodeType == XRNode.LeftEye || headState.tracked)
            {
                //Debug.Log(headState.nodeType);
                //ApplyPhysics();
            }
        }
    }

    private float v = 0;
    private void ApplyPhysics()
    {
        float playerHeight = head.transform.position.y - feet.transform.position.y;

        RaycastHit hit;

        Physics.Raycast(head.transform.position, Vector3.down, out hit, playerHeight + fallBuffer);

        if (hit.collider == null)
        {
            // nothing underneath, apply gravity
            v += Physics.gravity.y * Time.deltaTime;
            feet.transform.position = new Vector3(feet.transform.position.x,
                feet.transform.position.y + v * Time.deltaTime,
                feet.transform.position.z);
        }
        else
        {
            // apply platform update 
            // if near the player height, pin to the floor 
            TeleportTo(hit.point);
            v = 0;

            // platform moved, nudge player along 
            Vector3 platformMovement = lastPlatformLoc - hit.collider.transform.position;
            if (lastPlatform == hit.collider.gameObject)
            {
                feet.transform.position = feet.transform.position - platformMovement;
            }

            lastPlatform = hit.collider.gameObject;
            lastPlatformLoc = lastPlatform.transform.position;
        }
    }

    public void OnConfirmTeleport(RaycastHit target)
    {
        // sanity check. This movement must be legal at this time
        if (IsLegalToMove(target))
        {
            movementTarget = target.point;
            OnInit();
        }

    }

    private void OnInit()
    {
        isMoving = true;
        startTime = 0;

        // find starting ground height
        RaycastHit hitGround;
        if (Physics.Raycast(head.transform.position, Vector3.down, out hitGround))
        {
            startLoc = hitGround.point;
        }
    }

    private void OnMoving()
    {
        // move to next position
        startTime += Time.deltaTime;
        Vector3 target = Vector3.Lerp(startLoc, movementTarget, startTime / teleportTime);

        // offset between play area and head location, same as before
        Vector3 difference = feet.transform.position - head.transform.position;
        difference.y = 0;

        // fix up ground height if going over bumps
        RaycastHit groundHit;
        float playerHeight = head.transform.position.y - feet.transform.position.y;
        if (Physics.Raycast(head.transform.position, Vector3.down, out groundHit))
        {
            // found a bump, change starting height to this height to smooth the effect
            if (groundHit.distance < playerHeight)
            {
                startLoc.y = groundHit.point.y;
            }
        }

        // final position, same as before
        feet.transform.position = target + difference;


        // check to see if movement has ended, and do any cleanup
        if (startTime > teleportTime)
            OnMoveEnd();

    }

    private void OnMoveEnd()
    {
        isMoving = false;
        TeleportTo(movementTarget);
    }

    public bool IsLegalToMove(RaycastHit hit)
    {
        // currently moving, ignore input
        if (isMoving)
        {
            return false;
        }

        // nothing detected, disallow jumping off cliffs
        if (hit.collider == null)
        {
            return false;
        }
        else
        {
            if (!hit.collider.CompareTag("TeleportPath"))
            {
                return false;
            }
            // check how flat the interaction plane is.
            float groundAngle = Mathf.Abs(Vector3.Angle(Vector3.up, hit.normal));

            // if too steep, this location is NOT OK!
            if (groundAngle > maxSteepness)
            {
                return false;
            }

            float distance = GetClosestPointForHeadAt(hit.point, minDistanceFromWall);
            if (distance < minDistanceFromWall)
            {
                return false;
            }

        }

        return true;

    }

    private void TeleportTo(Vector3 target)
    {
        // offset between play area and head location
        Vector3 difference = feet.transform.position - head.transform.position;

        // ignore changes in y right now, to keep the head at the same height!
        difference.y = 0;

        // final position
        feet.transform.position = target + difference;
    }

    private float GetClosestPointForHeadAt(Vector3 location, float maxCheckDistance)
    {
        // assume no hit
        float distance = float.PositiveInfinity;

        // (1) check initial head area
        RaycastHit hitGround;
        float playerHeight = head.transform.position.y - feet.transform.position.y;
        Vector3 newHeadLoc = new Vector3(location.x, location.y + playerHeight, location.z);
        Collider[] hitObjs = Physics.OverlapSphere(newHeadLoc, minDistanceFromWall, ~MakeLayerMask(new int[] { 2 }));
        foreach (Collider hit in hitObjs)
        {
            Vector3 hitPoint = hit.ClosestPoint(newHeadLoc);
            float distanceToHead = (hitPoint - newHeadLoc).magnitude;
            //Debug.Log("Immediate Hit: " + hit.name);
            return distanceToHead;
        }

        // (2) shoot out a sphere from the head to the max distance
        // check along “thick ray”, check all layers BUT the ignore raycast layer
        // only works if the hit object is past the original sphere position
        float rayLength = maxCheckDistance;
        if (Physics.SphereCast(newHeadLoc, minDistanceFromWall, Vector3.down,
                               out hitGround, rayLength, ~MakeLayerMask(new int[] { 2 })))
        {
            //Debug.Log("Hit: " + hitGround.collider.name);

            // shorter distance?
            if (hitGround.distance < distance)
                distance = hitGround.distance;
        }


        return distance;
    }

    // (3) helper function to convert the list of layers into a
    // bit mask needed for physics casts
    private int MakeLayerMask(int[] list)
    {
        int mask = 0;
        foreach (int layer in list)
        {
            int bitPosition = 1 << layer;
            mask |= bitPosition;
        }
        return mask;
    }

    public void OnDrawGizmos()
    {
        // show only during run by checking if a variable is set
        if (head != null)
        {
            Vector3 right = new Vector3(head.transform.right.x, 0, head.transform.right.z);
            right.Normalize();
            Vector3 forward = new Vector3(head.transform.forward.x, 0, head.transform.forward.z);
            forward.Normalize();

            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(head.transform.position + forward, new Vector3(.1f, .1f, .1f));

            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawCube(head.transform.position + right, new Vector3(.1f, .1f, .1f));

            Gizmos.color = new Color(0, 0, 0, 0.5f);
            Gizmos.DrawSphere(feet.transform.position, 0.3f);
        }

    }

    public void OnConfirmStep(Vector3 dir)
    {
        movementTarget = AttemptStep(dir);
        OnInit();
    }

    private Vector3 AttemptStep(Vector3 step)
    {
        // (1A) where would our teleport point be
        Vector3 newLoc = head.transform.position + step * stepDistance;
        RaycastHit hit;
        Physics.Raycast(newLoc, Vector3.down, out hit);

        // (1B) if legal to teleport, teleport!
        if (IsLegalToMove(hit))
        {
            return hit.point;
        }
        else
        {
            //(3) binary search for legal teleport in between
            Physics.Raycast(head.transform.position, Vector3.down, out hit);

            Vector3 correctedLoc = hit.point;
            float magnitude = stepDistance / 2;
            for (int i = 1; i < correctionIterations; i++)
            {
                // determine no teleport location
                newLoc = head.transform.position + step * magnitude;
                Physics.Raycast(newLoc, Vector3.down, out hit);

                // if legal to teleport, try to move forward by half again
                if (IsLegalToMove(hit))
                {
                    magnitude = magnitude + stepDistance / Mathf.Pow(2, i + 1);
                    correctedLoc = hit.point;
                }
                // if not legal to teleport, try to move backwards by half again
                else
                {
                    magnitude = magnitude - stepDistance / Mathf.Pow(2, i + 1);
                }
            }
            return correctedLoc;

        }
    }
    public void OnConfirmTurn(float dir)
    {
        float direction = -dir * turnSpeed;

        // shift to "origin"
        Vector3 point = feet.transform.position - head.transform.position;

        // rotate around "origin" to find the new location
        float theta = direction * Mathf.Deg2Rad;
        Vector3 newPoint2 = new Vector3(point.x * Mathf.Cos(theta) - point.z * Mathf.Sin(theta),
            point.y,
            point.z * Mathf.Cos(theta) + point.x * Mathf.Sin(theta));

        // shift back
        Vector3 point2 = head.transform.position + newPoint2;
        feet.transform.position = point2;


        // apply rotate in reverse to place head in the same spot
        feet.transform.Rotate(new Vector3(0, -direction, 0));

    }

}
