using System.Collections.Generic;
using UnityEngine;

public class DecorationPlacer : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform _waterAndSkyTransform;

	[SerializeField]
	private Transform _floorTransform;

	[Header("Parameters")]
	[SerializeField]
	private float _depth = 10f;

	private bool _placed = false;

	/** Public Methods **/

	public void Place(List<GameObject> elements)
	{
		if (_placed)
		{
			Debug.LogError("Place() called twice");
			return;
		}
		_placed = true;

		// sea floor
		_floorTransform.position = _depth * Vector2.down;

		// decorations
		foreach (GameObject elementPrefab in elements)
		{
			GameObject obj = Instantiate(elementPrefab, transform);
			if (obj.TryGetComponent(out DecorationElement element))
			{
				Transform parent = element.Slot switch
				{
					DecorationElement.SlotEnum.SeafloorBack => _floorTransform,
					DecorationElement.SlotEnum.SeafloorFront => _floorTransform,
					_ => _waterAndSkyTransform,
				};

				obj.transform.SetParent(parent);
				obj.transform.localPosition = Vector2.zero;
				element.Place(_depth);
			}
			else
			{
				Debug.LogError("Attempted to place a decoration that wasn't a DecorationElement");
				Destroy(obj);
			}
		}
	}
}
