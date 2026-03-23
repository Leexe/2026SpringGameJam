using System;
using UnityEngine;

// TODO: make HookVisualsController listen to events? instead of exposing public
// methods for external classes to call.
public class HookVisualsController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private ParticleSystem _flash;

	[SerializeField]
	private ParticleSystem _absorb;

	[SerializeField]
	private ParticleSystem _pulse;

	[Header("Parameters")]
	// note: per-stage params are unused. right now we just use the visuals
	// from stage 1, as the depth display when charging provides enough info
	// and the extra color is obnoxious
	[SerializeField]
	private FlashParams[] _flashParams;

	public void Flash(int stage)
	{
		if (stage >= _flashParams.Length)
		{
			stage = _flashParams.Length - 1;
		}

		ParticleSystem.MainModule psMain = _flash.main;
		psMain.startColor = _flashParams[stage].Color;
		psMain.startSpeed = new ParticleSystem.MinMaxCurve(_flashParams[stage].Speed * 0.8f, _flashParams[stage].Speed);

		ParticleSystem.EmissionModule psEmission = _flash.emission;
		psEmission.rateOverTime = _flashParams[stage].Emission;
	}

	public void StopFlash()
	{
		ParticleSystem.EmissionModule psEmission = _flash.emission;
		psEmission.rateOverTime = 0f;
	}

	public void Absorb()
	{
		ParticleSystem.EmissionModule psEmission = _absorb.emission;
		psEmission.rateOverTime = 20f;
	}

	public void StopAbsorb()
	{
		ParticleSystem.EmissionModule psEmission = _absorb.emission;
		psEmission.rateOverTime = 0f;
	}

	public void Pulse(int stage)
	{
		if (stage >= _flashParams.Length)
		{
			stage = _flashParams.Length - 1;
		}

		ParticleSystem.MainModule psMain = _pulse.main;
		psMain.startColor = _flashParams[stage].Color;
		_pulse.Play();
	}

	//

	[Serializable]
	private struct FlashParams
	{
		public Color Color;
		public float Emission;
		public float Speed;
	}
}
