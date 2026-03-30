using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class GameManager : MonoSingleton<GameManager>
{
	[field: Header("References")]
	[field: SerializeField]
	public PlayerController Player { get; private set; }

	[SerializeField]
	private BulletManager _bulletManager;

	[SerializeField]
	private TMP_Text _debugTimeDisplay;

	[SerializeField]
	private PlayableDirector _enemyDirector;

	[SerializeField]
	private TransitionUI _transitionUI;

	[Header("Game Details")]
	[SerializeField]
	private int _maxPhase = 5;

	[SerializeField]
	private float _timeBetweenPhases = 5f;

	/** Events **/

	[HideInInspector]
	public UnityEvent OnGamePause;

	[HideInInspector]
	public UnityEvent OnGameResume;

	[HideInInspector]
	public UnityEvent OnGameStart; // Game starts when the player completes the first phase

	[HideInInspector]
	public UnityEvent OnGameRestart;

	[HideInInspector]
	public UnityEvent OnGameLose;

	[HideInInspector]
	public UnityEvent OnGameWin;

	[HideInInspector]
	public UnityEvent<int> OnIncrementPhase;

	[HideInInspector]
	public UnityEvent OnPlayerDeath;

	[HideInInspector]
	public UnityEvent OnDeathAnimationFinish;

	[HideInInspector]
	public UnityEvent OnFadeInFinish;

	[HideInInspector]
	public UnityEvent OnFadeOutFinish;

	[HideInInspector]
	public UnityEvent OnSceneReady;

	/** Fields **/

	// a negative value means game has not started yet
	public float GameTime { get; private set; } = -1f;

	private int _phase = 0;
	public bool CanPause { get; set; } = true;
	private bool _isPaused;
	private bool _wasDirectorPlaying;

	/** Unity Messages **/

	private void OnEnable()
	{
		OnPlayerDeath.AddListener(DisablePause);

		_bulletManager.OnPlayerCollision += Player.DieFromHit;
		Player.OnDie += TriggerPlayerDie;
		Player.OnDeathAnimationFinished += PlayTransitionOut;

		InputManager.Instance.OnEscapePerformed.AddListener(TogglePause);

		LevelManager.Instance.OnSceneReady.AddListener(TriggerSceneReady);
	}

	private void OnDisable()
	{
		if (Player != null)
		{
			Player.OnDie -= TriggerPlayerDie;
			Player.OnDeathAnimationFinished -= PlayTransitionOut;
		}

		if (_bulletManager != null)
		{
			_bulletManager.OnPlayerCollision -= Player.DieFromHit;
		}

		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnEscapePerformed.RemoveListener(TogglePause);
		}

		if (LevelManager.Instance != null)
		{
			LevelManager.Instance.OnSceneReady.RemoveListener(TriggerSceneReady);
		}
	}

	private void Start()
	{
		CanPause = false;
		CanPause = true;
		StartAmbience();
		InitPreSequence();
		HideCursor();
		FadeIn();
	}

	private void FixedUpdate()
	{
		if (GameTime >= 0f)
		{
			GameTime += Time.fixedDeltaTime;

			if (_phase < _maxPhase && Player.RepairSecondsLeft == 0f && !Player.IsBuffering)
			{
				_phase++;
				OnIncrementPhase?.Invoke(_phase);

				if (_phase >= _maxPhase)
				{
					OnGameWin?.Invoke();
				}
				else
				{
					Player.StartBuffer(_timeBetweenPhases);
				}
			}
		}
		else
		{
			// pre game
			if (Player.RepairSecondsLeft == 0f)
			{
				BeginSequence();
			}
		}

		_debugTimeDisplay.text = $"T: {GameTime:F2}";
	}

	/** Public Methods **/

	public void RestartGame()
	{
		GameTime = -1f;
		_phase = 0;
		if (_isPaused)
		{
			ResumeGame();
		}

		Player.Respawn();
		if (BulletManager.Instance != null)
		{
			BulletManager.Instance.ClearAllBullets();
		}

		_enemyDirector.Play();
		_enemyDirector.time = 0;
		_enemyDirector.Evaluate();
		_enemyDirector.Pause();

		InitPreSequence();
		OnGameRestart?.Invoke();

		CanPause = false;

		FadeIn();
	}

	/** Private Methods **/

	// player must repair to start game. this sets that up
	private void InitPreSequence()
	{
		Player.DisableInstability = true;
		Player.ResetRepairProgress(3f, 10);
	}

	// this starts the game
	private void BeginSequence()
	{
		GameTime = 0f;
		Player.DisableInstability = false;
		Player.StartBuffer(_timeBetweenPhases);
		OnIncrementPhase?.Invoke(_phase);

		StartDirector();
		AudioManager.Instance.StopAmbience();

		OnGameStart?.Invoke();
	}

	private void TriggerPlayerDie(bool isFromHit)
	{
		OnPlayerDeath?.Invoke();

		_transitionUI.PlayTransitionOut(() =>
		{
			OnFadeOutFinish?.Invoke();
			PauseDirector();
			RestartGame();
		});
	}

	/// <summary>
	/// Shows the cursor and unlocks it
	/// </summary>
	public void ShowCursor()
	{
		LevelManager.Instance.ShowCursor();
	}

	/// <summary>
	/// Hides the cursor and locks it to the center of the screen
	/// </summary>
	public void HideCursor()
	{
		LevelManager.Instance.HideCursor();
	}

	private void DisablePause()
	{
		CanPause = false;
	}

	private void StopDirector()
	{
		_enemyDirector.Stop();
	}

	private void PauseDirector()
	{
		_enemyDirector.Pause();
	}

	private void StartDirector()
	{
		_enemyDirector.time = 0;
		_enemyDirector.Play();
	}

	private void StartAmbience()
	{
		AudioManager.Instance.SwitchAmbience(FMODEvents.Instance.Ambience_Amb);
	}

	private void PlayTransitionOut()
	{
		OnDeathAnimationFinish?.Invoke();
	}

	private void TogglePause()
	{
		if (!CanPause)
		{
			return;
		}

		if (_isPaused)
		{
			ResumeGame();
		}
		else
		{
			PauseGame();
		}
	}

	private void PauseGame()
	{
		if (CanPause)
		{
			OnGamePause?.Invoke();
			Time.timeScale = 0f;
			_isPaused = true;

			_wasDirectorPlaying = _enemyDirector.state == PlayState.Playing;
			if (_wasDirectorPlaying)
			{
				_enemyDirector.Pause();
			}
		}
	}

	private void ResumeGame()
	{
		OnGameResume?.Invoke();
		Time.timeScale = 1f;
		_isPaused = false;

		if (_wasDirectorPlaying)
		{
			_enemyDirector.Play();
		}
	}

	private void TriggerSceneReady()
	{
		OnSceneReady?.Invoke();
	}

	private void FadeIn()
	{
		_transitionUI.PlayTransitionIn(() =>
		{
			OnFadeInFinish?.Invoke();
			CanPause = true;
			StartAmbience();
		});
	}
}
