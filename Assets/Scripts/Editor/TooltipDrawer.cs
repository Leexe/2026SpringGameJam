using UnityEditor;
using UnityEngine;

// Adds tooltips to ShaderLab properties via [Tooltip(Your Text)]
public class TooltipDrawer : MaterialPropertyDrawer
{
	private readonly string _tooltip;

	public TooltipDrawer(string tooltip)
	{
		_tooltip = tooltip;
	}

	public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
	{
		return MaterialEditor.GetDefaultPropertyHeight(prop);
	}

	public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
	{
		// 1. Draw the native property exactly how Unity normally would (handles any type: Color, Vector, Float, Textures)
		editor.DefaultShaderProperty(position, prop, label);

		// 2. Overlay a completely invisible label that only contains the tooltip text!
		// We constrain the width to the name label so the tooltip doesn't trigger when hovering over the value field itself.
		Rect tooltipRect = position;
		tooltipRect.width = EditorGUIUtility.labelWidth;
		GUI.Label(tooltipRect, new GUIContent("", _tooltip));
	}
}
