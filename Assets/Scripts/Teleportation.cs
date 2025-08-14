using UnityEngine;
using UnityEngine.InputSystem;

public class Teleportation : MonoBehaviour
{
    private UnityEngine.XR.InputDevice sourceDevice;
    public UnityEngine.XR.InputDevice SourceDevice { get => sourceDevice; }

    [SerializeField]
    private InputActionProperty teleportAction;

    [SerializeField]
    public LineRenderer line;

    private Vector3[] points = new Vector3[2];

    [SerializeField]
    private GameObject hand;

    [SerializeField]
    private GameObject head;
    [SerializeField]
    private GameObject feet;

    //hit information for this frame 
    private RaycastHit hit;

    private Color noHitColor = Color.magenta;
    private Color hitColor = Color.green;


    public void ShowLine(bool on)
    {
        line.enabled = on;
        hand.SetActive(!on);
    }


    // Start is called before the first frame update
    void Start()
    {
        if (teleportAction == null)
        {
            Debug.LogWarning("Need the teleport action to work. Teleporting is disabled.");
            return;
        }
        //ShowLine(false);
        teleportAction.action.performed += OnTeleport;
    }

    // Update is called once per frame
    void Update()
    {

        //point at hand
        points[0] = gameObject.transform.position;

        //point 4 units (meters) forward from hand
        points[1] = gameObject.transform.position + gameObject.transform.forward * 4;

        //give the line render this positions this frame
        line.SetPositions(points);

        // change line color if teleportation is legal
        if (HaveCollision())
        {
            line.startColor = hitColor;
            line.endColor = hitColor;

        }
        else
        {
            line.startColor = noHitColor;
            line.endColor = noHitColor;
        }

    }

    public void OnTeleport(InputAction.CallbackContext context)
    {
        if (hit.collider != null)
        {
            //should teleport?
            if (hit.collider.CompareTag("TeleportPath"))
            {
                //offset between play area and head location
                Vector3 difference = feet.transform.position - head.transform.position;

                //ignore changes in y right now, to keep the head at the same height!
                difference.y = 0;

                //final position
                feet.transform.position = hit.point + difference;

            }
        }
    }
    public bool HaveCollision()
    {
        //shoot out a ray that aligns to the current points
        Vector3 origin = points[0];
        Vector3 direction = points[1] - origin;
        float distance = direction.magnitude;
        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            if (hit.collider.CompareTag("TeleportPath"))
            {
                return true;
            }
        }

        return false;
    }
}
