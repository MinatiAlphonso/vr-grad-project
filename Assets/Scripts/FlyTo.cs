using UnityEngine;

public class FlyTo : MonoBehaviour
{
    //offset between tracker and trackee to avoid overlap
    [SerializeField]
    private Vector3 offset = new Vector3(0, 1, 0);

    //how far from linear progress
    [SerializeField]
    private float sharpness = 5f;

    //how long is the animation
    [SerializeField]
    private float duration = 5f;

    //how long into the follow animation
    private float elapsedTime = 0.0f;

    //item that should follow
    [SerializeField]
    private GameObject flyToItem;

    //how far away should be ignored
    [SerializeField]
    private float maxDistance = 1;

    //buffer period before pulling
    [SerializeField]
    private float delaySnapTime = 1;

    //how long since the follow object was last out of range
    private float delayElapsedTime = 0.0f;

    //starting point of the annimation
    private Vector3 start;

    //starting rotation of the animation
    private Quaternion startR;

    //find item by tag that should be followed. Only used if item is none
    [SerializeField]
    private string findTag;

    public void Start()
    {
        elapsedTime = duration;

        if (flyToItem == null)
            flyToItem = GameObject.FindGameObjectWithTag(findTag);

        //sanity check, and warning, for missing object to follow
        if (flyToItem == null)
        {
            Debug.LogWarning("No item to fly to in FlyTo script");
        }

    }

    public void Update()
    {
        // no object to follow, short circuit
        if (flyToItem == null)
        {
            return;
        }

        float distance = (flyToItem.transform.position - transform.position + offset).magnitude;

        // in range, and not currently animating, reset delay timer
        if (distance < maxDistance && elapsedTime >= duration)
        {
            delayElapsedTime = 0;
        }
        //out of range, and not currently animating, start the delay timer
        else if (distance > maxDistance && elapsedTime >= duration)
        {
            delayElapsedTime += Time.deltaTime;

            //out of range, and delay timer elapsed, start snap
            if (delayElapsedTime > delaySnapTime)
            {
                start = transform.position;
                startR = transform.rotation;

                elapsedTime = 0.0f;
            }
        }
        //snap triggered, running animation
        else if (elapsedTime < duration)
        {
            Vector3 target = flyToItem.transform.position + offset;

            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / duration;
            float progress = Progress(percent);

            transform.position = Vector3.Lerp(start, target, progress);
            transform.rotation = Quaternion.Lerp(startR, flyToItem.transform.rotation, progress);
        }


    }
    private float Progress(float percentTime)
    {
        return Mathf.Pow(percentTime, 1 / sharpness);
    }

}
