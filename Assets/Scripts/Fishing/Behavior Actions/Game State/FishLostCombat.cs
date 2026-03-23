using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Fish Lost Combat")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(
	name: "Fish Lost Combat",
	message: "[Fish] lost while fighting [cursor]",
	category: "Fish Combat",
	id: "9fe230c4dcad0e6f323331f2a3c3015d"
)]
public sealed partial class FishLostCombat : EventChannel<GameObject, GameObject> { }
