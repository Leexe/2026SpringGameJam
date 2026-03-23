using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/FishNoticedBiteable")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "FishNoticedBiteable", message: "[Fish] noticed biteable [biteable]", category: "Events", id: "93d1b0a55f4a9286c2fd1789f7a4ddd4")]
public sealed partial class FishNoticedBiteable : EventChannel<GameObject, Biteable> { }

