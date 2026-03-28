using UnityEditor;
using UnityEngine;

// Adds tooltips to ShaderLab properties via [Tooltip(Text)]
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
		editor.DefaultShaderProperty(position, prop, label);

		Rect tooltipRect = position;
		tooltipRect.width = EditorGUIUtility.labelWidth;
		GUI.Label(tooltipRect, new GUIContent("", _tooltip));
	}
}
