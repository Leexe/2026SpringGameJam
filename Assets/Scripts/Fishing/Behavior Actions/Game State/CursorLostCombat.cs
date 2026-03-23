using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Cursor Lost Combat")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(
	name: "Cursor Lost Combat",
	message: "[Cursor] lost while fighting [fish]",
	category: "Fish Combat",
	id: "6ff05e759f8d9dbdaca625aa0062a8b9"
)]
// cursor, fish
public sealed partial class CursorLostCombat : EventChannel<GameObject, GameObject> { }
