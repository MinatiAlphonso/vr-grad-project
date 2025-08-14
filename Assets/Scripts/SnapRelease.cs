using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapRelease : MonoBehaviour
{
    private float startTime = 0;
    private Vector3 startPos = new Vector3();
    private Quaternion startRot = Quaternion.identity;
    public float speed = 0.5f;
    private GameObject item;
    private Vector3 offset;

    public void Update()
    {
        if (item != null)
        {
            float percent = (Time.realtimeSinceStartup - startTime) / speed;
            Vector3 position = Vector3.Slerp(startPos, transform.position + offset, percent);
            Quaternion rot = Quaternion.Slerp(startRot, transform.rotation, percent);

            item.transform.position = position;
            item.transform.rotation = rot;

            //turn off animation for efficiency
            if (percent >= 1)
                item = null;
        }
    }

    public void Release(GameObject obj)
    {
        //parent to the snap area
        obj.transform.parent = transform;

        //save starting info
        startTime = Time.realtimeSinceStartup;
        startPos = obj.transform.position;
        startRot = obj.transform.rotation;
        item = obj;

        //align with area
        obj.transform.rotation = this.transform.rotation;
        obj.transform.position = this.transform.position;
    }

    /// <summary>
    /// register snapping within range
    /// </summary>
    /// <param name="other">What has entered this area</param>
    private void OnTriggerEnter(Collider other)
    {
        //GrabEffectHold hold = other.GetComponent<GrabEffectHold>();
        //if (hold != null)
        //{
        //    hold.SetReleaseFunction(this);
        //}
    }

    /// <summary>
    /// deregister snapping within range
    /// </summary>
    /// <param name="other">What has exited this area</param>
    private void OnTriggerExit(Collider other)
    {
        //GrabEffectHold hold = other.GetComponent<GrabEffectHold>();
        //if (hold != null)
        //{
        //    hold.SetReleaseFunction(null);
        //}
    }
}