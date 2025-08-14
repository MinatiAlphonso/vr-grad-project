using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class MoveSelection : MonoBehaviour
{
    // the movement code
    [SerializeField]
    private Navigation navigation;

    // user input events
    [SerializeField]
    private InputActionProperty searchTeleport;
    [SerializeField]
    private InputActionProperty confirmTeleport;
    [SerializeField]
    private InputActionProperty walk;
    [SerializeField]
    private InputActionProperty strafe;
    [SerializeField]
    private InputActionProperty turn;

    // max number of segments in the parabola
    [SerializeField]
    private int maxSegments = 30;

    // how much outward push
    [SerializeField]
    private float force = 10;

    [SerializeField]
    private float lineSize = 0.1f;

    [SerializeField]
    private Color legalTeleportColor = Color.green;
    [SerializeField]
    private Color illegalTeleportColor = Color.magenta;

    // reference to the parabola line
    private LineRenderer line;

    // how close are the points, make smaller for smoother curves
    [SerializeField]
    private float stepSize = 0.1f;

    // current points in the parabola
    private Vector3[] linePoints = new Vector3[2];

    // player rig parameters
    [SerializeField]
    private GameObject head;
    [SerializeField]
    private GameObject rightHand;

    [SerializeField]
    private GameObject decal;

    [SerializeField]
    private InputActionProperty vrWalk;
    [SerializeField]
    private InputActionProperty vrTurn;

    private bool canMove = true;
    private float resetDis = 0.3f;
    private float activateDis = 0.8f;

    private bool canTurn = true;

    void Start()
    {
        vrWalk.action.performed += OnVrMove;
        vrTurn.action.performed += OnVrTurn;

        searchTeleport.action.performed += OnSearching;
        confirmTeleport.action.performed += OnConfirmTeleport;
        walk.action.performed += OnConfirmWalk;
        strafe.action.performed += OnConfirmStrafe;
        turn.action.performed += OnConfirmTurn;

        // create the line
        line = gameObject.AddComponent<LineRenderer>();
        line.numCornerVertices = 2;
        line.numCapVertices = 2;
        line.material = new Material(Shader.Find("Sprites/Default"));
        linePoints = new Vector3[maxSegments];

        // apply the user’s preferences
        line.positionCount = maxSegments;
        line.startWidth = lineSize;
        line.endWidth = lineSize;
        line.startColor = legalTeleportColor;
        line.endColor = legalTeleportColor;

        // hide for now
        line.enabled = false;

        if (decal != null)
        {
            decal = Instantiate(decal);
            decal.SetActive(false);
        }
    }

    private void OnVrMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        // last frame the touch was not near an edge, and now it is 
        if (canMove && input.magnitude > activateDis)
        {
            // 1) figure out our direction in left-handed system 
            float theta = Mathf.Atan2(input.x, input.y);

            // 2) rotate forward around up by theta 
            Vector3 forward = new Vector3(head.transform.forward.x, 0, head.transform.forward.z);
            forward.Normalize();
            Vector3 dir = Quaternion.AngleAxis(Mathf.Rad2Deg * theta, Vector3.up) * forward;

            // 3) apply step 
            navigation.OnConfirmStep(dir);
            canMove = false;
        }
        else
        {
            // the touch is near the center, allow a new event 
            if (input.magnitude < resetDis)
                canMove = true;
        }
    }

    private void OnVrTurn(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        Debug.Log(input);

        if (canTurn && Mathf.Abs(input.x) > activateDis)
        {
            navigation.OnConfirmTurn(input.x);
            canTurn = false;
        }
        else
        {
            if (Mathf.Abs(input.x) < resetDis)
                canTurn = true;
        }
    }
    public void OnSearching(InputAction.CallbackContext context)
    {
        if (!navigation.IsMoving)
            line.enabled = true;
    }

    private void OnConfirmTurn(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();
        navigation.OnConfirmTurn(input);
    }

    private void OnConfirmWalk(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();

        // figure out our direction
        Vector3 forward = new Vector3(head.transform.forward.x, 0, head.transform.forward.z);
        forward.Normalize();
        navigation.OnConfirmStep(forward * input);
    }


    private void OnConfirmStrafe(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();

        // figure out our direction
        Vector3 right = new Vector3(head.transform.right.x, 0, head.transform.right.z);
        right.Normalize();
        navigation.OnConfirmStep(right * input);
    }


    public void OnConfirmTeleport(InputAction.CallbackContext context)
    {
        if (decal != null)
            decal.SetActive(false);

        line.enabled = false;
        RaycastHit hit = UpdateLine();
        navigation.OnConfirmTeleport(hit);
    }

    public void Update()
    {
        if (line.enabled)
        {
            RaycastHit hit = UpdateLine();

            // update feedback
            if (navigation.IsLegalToMove(hit))
            {
                line.startColor = legalTeleportColor;
                line.endColor = legalTeleportColor;

                if (decal != null)
                {
                    decal.SetActive(true);
                    // place at point, just a little above the plane
                    decal.transform.position = hit.point + hit.normal * 0.01f;
                    decal.transform.rotation = Quaternion.LookRotation(-hit.normal);
                }

            }
            else
            {
                if (decal != null)
                {
                    // turn off
                    decal.SetActive(false);
                }

                line.startColor = illegalTeleportColor;
                line.endColor = illegalTeleportColor;
            }

        }
    }

    private RaycastHit UpdateLine()
    {
        // hit information for this frame 
        RaycastHit parabolaIntersection = new RaycastHit();

        // find starting point based on the object this is on
        Vector3 currentPoint = rightHand.transform.position;

        // calculate initial a and v based on force
        Vector3 a = Physics.gravity;
        Vector3 v = rightHand.transform.forward * force;

        // do Euler steps
        line.positionCount = maxSegments;
        for (int i = 0; i < maxSegments; i++)
        {
            linePoints[i] = currentPoint;
            v = v + a * stepSize;
            currentPoint = currentPoint + v * stepSize;

            // ready to calculate hit point
            if (i > 0)
            {
                // figure out ray, and check
                Ray r = new Ray(linePoints[i - 1], linePoints[i] - linePoints[i - 1]);
                float distance = (linePoints[i] - linePoints[i - 1]).magnitude;
                if (Physics.Raycast(r, out parabolaIntersection, distance))
                {
                    // stop on first hit
                    linePoints[i] = parabolaIntersection.point;
                    line.positionCount = i + 1;
                    break;
                }
            }
        }

        // give the line render this position this frame
        line.SetPositions(linePoints);

        return parabolaIntersection;
    }

}