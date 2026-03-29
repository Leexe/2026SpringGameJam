using System.Diagnostics.CodeAnalysis;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "Odin.OdinUnknownGroupingPath")]
public class FMODEvents : MonoSingleton<FMODEvents>
{
	#region Music

	[field: SerializeField]
	[field: FoldoutGroup("Music", true)]
	public EventReference Song_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music", true)]
	public EventReference Menu_Bgm { get; private set; }

	#endregion

	#region Ambience

	[field: SerializeField]
	[field: FoldoutGroup("Ambience", expanded: true)]
	public EventReference Ambience_Amb { get; private set; }

	#endregion

	#region SFX

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference FadeIn_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference FadeOut_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference Explosion_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference ButtonHover_Sfx { get; private set; }

	#endregion

	#region Looping SFX

	[field: SerializeField]
	[field: FoldoutGroup("Loop SFX", true)]
	public EventReference Warning_LoopSFX { get; private set; }

	#endregion
}
