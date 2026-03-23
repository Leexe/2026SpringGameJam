using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Fish Status Change")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Fish Status Change", message: "[Fish] wants status change to [value]", category: "Events", id: "987561b96347a7e22d267877105c4c66")]
public sealed partial class FishStatusChange : EventChannel<GameObject, FishStatus> { }

