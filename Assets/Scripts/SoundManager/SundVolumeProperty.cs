using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SoundBarAttribute))]
public class SundVolumeProperty : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		property.floatValue =  EditorGUI.Slider(position, property.floatValue, 0f, 1f);
		EditorGUI.ProgressBar(new Rect(position.x, position.y, position.width - 53f, position.height), property.floatValue, property.name);
	}
}
#endif

public class SoundBarAttribute : PropertyAttribute
{
}
