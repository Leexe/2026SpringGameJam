using System;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

[CreateAssetMenu(fileName = "Fish", menuName = "Fish/FishSO")]
public class FishSO : ScriptableObject
{
	/** Variables **/

	[field: Title("Basic Info")]
	[field: Tooltip("Unique string id. Primarily used to identify the SO when serializing FishInstances")]
	[field: SerializeField]
	[field: Required]
	public string Id { get; private set; }

	[field: Tooltip("The name of the fish")]
	[field: SerializeField]
	public string Name { get; private set; }

	[field: Tooltip("A short description of the fish")]
	[field: SerializeField]
	[field: TextArea(5, 8)]
	public string Description { get; private set; } = "This is a Fish";

	[field: Tooltip("The sprite of the fish")]
	[field: SerializeField]
	[field: PreviewField(100, ObjectFieldAlignment.Right)]
	public Sprite Sprite { get; private set; }

	[field: Title("Stats")]
	[field: Tooltip("The min and max length of the fish")]
	[field: SerializeField]
	[field: MinMaxSlider(0f, 200f, true)]
	public Vector2 Length { get; private set; } = new Vector2(1f, 2.5f);

	[field: Tooltip("The min and max weight of the fish")]
	[field: SerializeField]
	[field: MinMaxSlider(0f, 10000f, true)]
	public Vector2 Weight { get; private set; } = new Vector2(1f, 10f);

	[field: Tooltip("The min and max sell value of the fish, depending on the normalized length of the fish")]
	[field: SerializeField]
	[field: MinMaxSlider(0f, 10000f, true)]
	public Vector2 SellValue { get; private set; } = new Vector2(50f, 100f);

	[field: Tooltip("The min and max normalized strength of the fish, depending on the normalized length of the fish")]
	[field: SerializeField]
	[field: MinMaxSlider(0f, 1f, true)]
	public Vector2 Strength { get; private set; } = new Vector2(0.4f, 0.6f);

	[field: Tooltip(
		"The min and max normalized aggression of the fish, depending on the normalized length of the fish"
	)]
	[field: SerializeField]
	[field: MinMaxSlider(0f, 1f, true)]
	public Vector2 Aggression { get; private set; } = new Vector2(0.4f, 0.6f);

	[field: Tooltip(
		"The min and max normalized aggression of the fish, depending on the normalized length of the fish"
	)]
	[field: SerializeField]
	[field: Range(0f, 15f)]
	public float TimeToCatch { get; private set; } = 4f;

	[field: Title("Misc.")]
	[field: Tooltip("The behavior graph that the fish should follow during the minigame")]
	[field: SerializeField]
	[field: Required]
	public BehaviorGraph FishAI { get; private set; }

	[field: Tooltip("Depth range where this fish spawns - mvp only. Final ver will have a more sophisticated system.")]
	[field: SerializeField]
	[field: MinMaxSlider(0f, 1000f, true)]
	public Vector2 DepthRange { get; private set; } = new Vector2(0f, 10f);

	[field: Tooltip("How often this fish spawns - mvp only. Final ver will have a more sophisticated system.")]
	[field: SerializeField]
	public int SpawnWeight { get; private set; } = 10;

	[field: Tooltip("The prefab for the fish")]
	[field: SerializeField]
	[field: Required]
	public GameObject Prefab { get; private set; }

	[field: Tooltip("Region ID used for sorting or grouping in the Encyclopedia")]
	[field: SerializeField]
	[field: MinValue(0)]
	public int RegionID { get; private set; } = 0;

	[field: Tooltip("Dex number used to sort fish in the Encyclopedia")]
	[field: SerializeField]
	[field: MinValue(0)]
	public int NumID { get; private set; } = 0;
}

[Serializable]
public class FishInstance
{
	[field: SerializeField]
	public FishSO SO { get; private set; }

	[field: SerializeField]
	public float Length { get; private set; }

	[field: SerializeField]
	public float Weight { get; private set; }

	[field: SerializeField]
	public float SellValue { get; private set; }

	[field: SerializeField]
	public float Strength { get; private set; }

	[field: SerializeField]
	public float Aggression { get; private set; }

	public Sprite Sprite => SO != null ? SO.Sprite : null;
	public string Id => SO != null ? SO.Id : default;

	private FishInstance() { }

	public static FishInstance GenerateFromSO(FishSO so)
	{
		if (so == null)
		{
			return null;
		}

		var inst = new FishInstance { SO = so };

		float t = UnityEngine.Random.value;

		inst.Length = Mathf.Lerp(so.Length.x, so.Length.y, t);
		inst.Weight = Mathf.Lerp(so.Weight.x, so.Weight.y, t);
		inst.SellValue = Mathf.Lerp(so.SellValue.x, so.SellValue.y, t);

		inst.Strength = Mathf.Lerp(so.Strength.x, so.Strength.y, t);
		inst.Aggression = Mathf.Lerp(so.Aggression.x, so.Aggression.y, t);

		return inst;
	}
}
