using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternSpawner : MonoBehaviour
{
	[System.Serializable]
	public class PatternPhase
	{
		public BulletPattern pattern;
		public Transform origin; //optional for patterns that need specific origin
		public bool trackPlayer = false;
		public bool simueltaneous = false;
		public float initialDelay = 0f;
		public float interval = 2f;
		public int repeatCount = 5;
	}

	[SerializeField]
	private List<PatternPhase> phases = new List<PatternPhase>();

	[SerializeField]
	private Transform playerTransform;

	[SerializeField]
	private bool autoStart = true;

	[SerializeField]
	private float delayBetweenPhases = 1f;

	[SerializeField]
	private bool loopPhases = true;

	private int currentPhaseIndex = 0;
	private bool isRunning = false;
	private List<Coroutine> activeCoroutines = new List<Coroutine>();

	private void Start()
	{
		if (playerTransform == null)
		{
			var player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
			{
				playerTransform = player.transform;
			}
		}

		if (autoStart)
		{
			StartSpawning();
		}
	}

	public void StartSpawning()
	{
		if (!isRunning)
		{
			isRunning = true;
			StartCoroutine(RunPhases());
		}
	}

	public void StopSpawning()
	{
		isRunning = false;
		StopAllCoroutines();
	}

	private IEnumerator RunPhases()
	{
		while (isRunning)
		{
			if (phases.Count == 0)
			{
				yield break;
			}

			List<PatternPhase> activePhases = new List<PatternPhase>();
			activePhases.Add(phases[currentPhaseIndex]);

			int nextIndex = currentPhaseIndex + 1;
			while (nextIndex < phases.Count && phases[nextIndex].simueltaneous)
			{
				activePhases.Add(phases[nextIndex]);
				nextIndex++;
			}

			List<Coroutine> phaseCoroutines = new List<Coroutine>();
			foreach (var phase in activePhases)
			{
				Coroutine phaseCoroutine = StartCoroutine(RunSinglePhase(phase));
				phaseCoroutines.Add(phaseCoroutine);
				activeCoroutines.Add(phaseCoroutine);
			}

			//Wait for all phase coroutines to complete
			foreach (var coroutine in phaseCoroutines)
			{
				yield return coroutine;
			}

			currentPhaseIndex = nextIndex;
			if (currentPhaseIndex >= phases.Count)
			{
				if (loopPhases)
				{
					currentPhaseIndex = 0;
				}
				else
				{
					isRunning = false;
					yield break;
				}
			}

			if (delayBetweenPhases > 0f && isRunning)
			{
				yield return new WaitForSeconds(delayBetweenPhases);
			}
		}
	}

	private IEnumerator RunSinglePhase(PatternPhase phase)
	{
		// If this is a Laser, pass the origin transform
		var laser = phase.pattern as Laser;
		if (laser != null)
		{
			laser.SetOriginTransform(phase.origin);
		}

		int count = 0;
		while (isRunning && (phase.repeatCount < 0 || count < phase.repeatCount))
		{
			if (count == 0 && phase.initialDelay > 0f)
			{
				yield return new WaitForSeconds(phase.initialDelay);
			}
			Vector2? playerPos =
				phase.trackPlayer && playerTransform != null ? playerTransform.position : (Vector2?)null;

			phase.pattern.Fire(phase.origin != null ? phase.origin.position : transform.position, playerPos);

			count++;

			if (count < phase.repeatCount || phase.repeatCount < 0)
			{
				yield return new WaitForSeconds(phase.interval);
			}
		}
	}

	//Manually fire a specific pattern
	public void FirePattern(int phaseIndex)
	{
		if (phaseIndex >= 0 && phaseIndex < phases.Count)
		{
			PatternPhase phase = phases[phaseIndex];
			Vector3? playerPos =
				phase.trackPlayer && playerTransform != null ? playerTransform.position : (Vector3?)null;

			phase.pattern.Fire(transform.position, playerPos);
		}
	}
}
