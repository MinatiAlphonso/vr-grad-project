using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrabEffectHover : ScriptableHandEffect
{
    private Transform myTransform;
    public override HandEffectType EffectType => HandEffectType.ColorHover;
    private List<Material> materials;

    [SerializeField]
    private Color colorLeft = new Color(0.25f, 0.25f, 0.75f);

    [SerializeField]
    private Color colorRight = new Color(0.75f, 0.25f, 0.25f);

    public override void Initialize(Transform t)
    {
        myTransform = t;
        InitializeMaterials();
    }

    //Gets all the materials from each renderer
    private void InitializeMaterials()
    {
        materials = new List<Material>();
        Renderer[] rendererlist = myTransform.gameObject.GetComponents<Renderer>();
        foreach (Renderer renderer in rendererlist)
        {
            materials.AddRange(new List<Material>(renderer.materials));
        }
    }

    public override bool OnHover(Grab controller)
    {
        //sanity check that the hand can grab something still
        if (controller.InHand != null)
            return false;

        if (controller.GetNearestGrabbable() == myTransform.gameObject)
        {
            Color color = colorRight;
            if (controller.WhichHand == Grab.Hand.LEFT)
            {
                color = colorLeft;
            }

            foreach (Material material in materials)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color);
            }
        }
        else
        {
            OnRemove(controller);
        }

        return true;
    }

    public override bool OnRemove(Grab controller)
    {
        foreach (Material material in materials)
        {
            material.DisableKeyword("_EMISSION");
        }
        return true;
    }

    public override bool OnGrab(Grab controller) 
    {
        OnRemove(controller);
        return false; 
    }
    public override bool OnRelease(Grab controller) { return false; }
    public override bool OnHaptics(Grab controller) { return false; }
    public override bool OnDisappear(Grab controller) { return false; }
}