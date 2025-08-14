using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IHandEffect))]
public class HandEffectDrawer: PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var effect = property.managedReferenceValue;
        if (effect != null)
        {
            EditorGUI.LabelField(position, effect.GetType().Name);
        }
    }
}
