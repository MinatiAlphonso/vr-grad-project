using UnityEngine;
using UnityEditor;
using static UnityEngine.GraphicsBuffer;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

[CustomEditor(typeof(GrabEffectBuilder))]
public class GrabEffectBuilderEditor : Editor
{
    private bool isDuplicate;
    private HandEffectType duplicateType;
    private Dictionary<ScriptableHandEffect, Editor> effectEditors = new Dictionary<ScriptableHandEffect, Editor>();
    private int priorCount = 0;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GrabEffectBuilder myGrabEffectBuilder = (GrabEffectBuilder)target;
        myGrabEffectBuilder.selectedEffect = (HandEffectType)EditorGUILayout.EnumPopup("Effect Type",myGrabEffectBuilder.selectedEffect);

        List<HandEffectType> listType = new List<HandEffectType>();
        foreach (var effect in myGrabEffectBuilder.listHandEffects)
        {
            if (effect == null) continue;
            listType.Add(effect.EffectType);
        }

        if (GUILayout.Button("Add Effect"))
        {
            isDuplicate = listType.Contains(myGrabEffectBuilder.selectedEffect);
            duplicateType = myGrabEffectBuilder.selectedEffect;

            myGrabEffectBuilder.addEffect();
        }

        if (isDuplicate)
        {
            EditorGUILayout.HelpBox($"Duplicate {duplicateType} effect is added!", MessageType.Warning);
        }

        // something was deleted
        if(priorCount > myGrabEffectBuilder.listHandEffects.Count)
        {
            //remove from dictionary
            var effectsOld = effectEditors.Keys.Where(effect => !myGrabEffectBuilder.listHandEffects.Contains(effect)).ToList();
            foreach(var key in effectsOld)
            {
                effectEditors.Remove(key);
            }
        }

        for (int i = 0; i <myGrabEffectBuilder.listHandEffects.Count; i++)
        {
            var effect = myGrabEffectBuilder.listHandEffects[i];
            if(effect == null) { continue; }

            // create new editor only if not yet made for the grab effect
            if (!effectEditors.ContainsKey(effect))
            {
                effectEditors.Add(effect, CreateEditor(effect as UnityEngine.Object));
            }
            effectEditors[effect].OnInspectorGUI();
        }
        priorCount = myGrabEffectBuilder.listHandEffects.Count;
    }
}