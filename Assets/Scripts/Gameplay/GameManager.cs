using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
	[SerializeField]
	private PlayerController _player;

	[SerializeField]
	private BulletManager _bulletManager;

	[SerializeField]
	private TMP_Text _debugTimeDisplay;

	/** Events **/

	[HideInInspector]
	public UnityEvent OnGamePause;

	[HideInInspector]
	public UnityEvent OnGameResume;

	[HideInInspector]
	public UnityEvent OnGameStart; // Game starts when the player completes the first phase

	[HideInInspector]
	public UnityEvent OnPlayerDeath;

	[HideInInspector]
	public UnityEvent OnDeathAnimationFinish;

	/** Fields **/

	// a negative value means game has not started yet
	public float GameTime { get; private set; } = -1f;

	private int _phase = 0;
	private bool _canPause = true;
	private bool _isPaused;

	/** Unity Messages **/

	private void OnEnable()
	{
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

	/** Private Methods **/

	// player must repair to start game. this sets that up
	private void InitPreSequence()
	{
		_player.DisableInstability = true;
		_player.ResetRepairProgress(3f, 10f);
		AudioManager.Instance.SwitchMusic(FMODEvents.Instance.Song_Bgm);
	}

	// this starts the game
	private void BeginSequence()
	{
		GameTime = 0f;
		_player.DisableInstability = false;
		_player.ResetRepairProgress();

		// start music, etc...
	}

	private void TriggerPlayerDie()
	{
		OnPlayerDeath?.Invoke();
	}

	private void PlayTransitionOut()
	{
		OnDeathAnimationFinish?.Invoke();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
		if (_canPause)
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
