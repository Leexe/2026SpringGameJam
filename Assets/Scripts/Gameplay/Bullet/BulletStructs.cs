using UnityEngine;

public enum BulletBehavior
{
	Linear = 0,
	SineWave = 1,
	Following = 2,
	Steering = 3,
	Homing = 4,
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
	public float MaxLifeTime;

	// Fade Out
	public bool IsFading;
	public float FadeTimer;
	public float FadeDuration;

	// Behavior Specific Parameters
	public bool RotateTowardsDirection;
	public float HitRadius;
	public BulletBehavior Behavior;
	public float Amplitude;
	public float Frequency;
	public float TrackingStrength;
	public float MaxSteerForce;

	// Homing Parameters
	public float Heading;
	public float AngularVelocity;
	public float MaxTurnRate;
	public float TurnAcceleration;
}
