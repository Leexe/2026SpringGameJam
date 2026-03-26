using UnityEngine;

#region Enums

public enum BulletBehavior
{
	Linear,
	SineWave,
	Tracking,
}

#endregion

#region Structs

[System.Serializable]
public struct Bullet
{
	#region Fields

	public bool IsActive;

	// Target visual and physical definition
	public BulletData Data;

	// Core physics
	public Vector2 Position;
	public Vector2 Velocity;
	public float TimeAlive;

	// Behavior specific parameters
	public BulletBehavior Behavior;
	public float Amplitude;
	public float Frequency;

	#endregion
}

#endregion
