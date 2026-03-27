using UnityEngine;

public enum BulletBehavior
{
	Linear = 0,
	SineWave = 1,
	Tracking = 2,
}

[System.Serializable]
public struct Bullet
{
	public bool IsActive;
	public BulletSO SO;

	// Core Physics
	public Vector2 Position;
	public Vector2 Velocity;
	public float Speed;
	public float TimeAlive;

	// Behavior Specific Parameters
	public bool RotateTowardsDirection;
	public float HitRadiusSqr;
	public BulletBehavior Behavior;
	public float Amplitude;
	public float Frequency;
	public float TrackingStrength;
}
