using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/BiteSuccess")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(
	name: "BiteSuccess",
	message: "[Agent] has bitten [biteable] with player [player]",
	category: "Fish Detection",
	id: "df45ea1cd83dce5a147d1f4f33118f61"
)]
// fish controller, biteable, cursorcontroller
public sealed partial class BiteSuccess : EventChannel<GameObject, Biteable, GameObject> { }
