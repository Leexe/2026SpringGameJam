using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
	[Header("References")]
	[SerializeField]
	private PlayerController _player;

	[SerializeField]
	private BulletManager _bulletManager;

	[SerializeField]
	private TMP_Text _debugTimeDisplay;

	[SerializeField]
	private PlayableDirector _enemyDirector;

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
	public UnityEvent OnPlayerDeath;

	[HideInInspector]
	public UnityEvent OnDeathAnimationFinish;

	[HideInInspector]
	public UnityEvent OnFadeInFinish;

	[HideInInspector]
	public UnityEvent OnFadeOutFinish;

	/** Fields **/

	// a negative value means game has not started yet
	public float GameTime { get; private set; } = -1f;

	private int _phase = 0;
	public bool CanPause { get; set; } = true;
	private bool _isPaused;

	/** Unity Messages **/

	private void OnEnable()
	{
		OnFadeInFinish.AddListener(EnablePause);
		OnPlayerDeath.AddListener(DisablePause);

		_bulletManager.OnPlayerCollision += _player.DieFromHit;
		_player.OnDie += TriggerPlayerDie;
		_player.OnDeathAnimationFinished += PlayTransitionOut;

		InputManager.Instance.OnEscapePerformed.AddListener(TogglePause);
	}

	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnDie -= TriggerPlayerDie;
			_player.OnDeathAnimationFinished -= PlayTransitionOut;
		}

		if (_bulletManager != null)
		{
			_bulletManager.OnPlayerCollision -= _player.DieFromHit;
		}

		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnEscapePerformed.RemoveListener(TogglePause);
		}
	}

	private void Start()
	{
		InitPreSequence();
	}

	private void FixedUpdate()
	{
		if (GameTime >= 0f)
		{
			GameTime += Time.fixedDeltaTime;

			if (_phase < _maxPhase && _player.RepairSecondsLeft == 0f && !_player.IsBuffering)
			{
				_phase++;

				if (_phase >= _maxPhase)
				{
					OnGameWin?.Invoke();
				}
				else
				{
					_player.StartBuffer(_timeBetweenPhases);
				}
			}
		}
		else
		{
			// pre game
			if (_player.RepairSecondsLeft == 0f)
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

		_player.Respawn();
		if (BulletManager.Instance != null)
		{
			BulletManager.Instance.ClearAllBullets();
		}

		InitPreSequence();
		OnGameRestart?.Invoke();
	}

	/** Private Methods **/

	// player must repair to start game. this sets that up
	private void InitPreSequence()
	{
		_player.DisableInstability = true;
		_player.ResetRepairProgress(3f, 10f);
	}

	// this starts the game
	private void BeginSequence()
	{
		GameTime = 0f;
		_player.DisableInstability = false;
		_player.StartBuffer(_timeBetweenPhases);

		StartDirector();

		OnGameStart?.Invoke();
	}

	private void TriggerPlayerDie()
	{
		OnPlayerDeath?.Invoke();

		if (_enemyDirector != null)
		{
			StopDirector();
		}
	}

	private void EnablePause()
	{
		CanPause = true;
	}

	private void DisablePause()
	{
		CanPause = false;
	}

	private void StopDirector()
	{
		_enemyDirector.Stop();
	}

	private void StartDirector()
	{
		_enemyDirector.time = 0;
		_enemyDirector.Play();
	}

	private void PlayTransitionOut()
	{
		OnDeathAnimationFinish?.Invoke();
	}

	private void TogglePause()
	{
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
		}
	}

	private void ResumeGame()
	{
		OnGameResume?.Invoke();
		Time.timeScale = 1f;
		_isPaused = false;
	}
}
