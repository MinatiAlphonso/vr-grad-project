using UnityEngine;

public class Blink : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    private bool onOff = true;
    private float currentTime = 0f;


    void Update()
    {
        //update if it is visable
        currentTime += Time.deltaTime;

        // disable the colliders
        Collider parentCollider = GetComponent<Collider>();
        if (parentCollider != null)
        {
            parentCollider.enabled = onOff;
        }

        // disable the renderer in parents if present
        Renderer parentRenderer = GetComponent<Renderer>();
        if (parentRenderer != null)
        {
            parentRenderer.enabled = onOff;
        }

        // disable the renderer in children if present
        Renderer childRenderer = GetComponentInChildren<Renderer>();
        if (childRenderer != null)
        {
            childRenderer.enabled = onOff;
        }
        //GetComponent<Renderer>().enabled = onOff;
        //GetComponent<Collider>().enabled = onOff;

        // if a timeout occurs, flip visibility
        if (currentTime >= speed)
        {
            currentTime = 0f;
            onOff = !onOff;
        }
    }
}