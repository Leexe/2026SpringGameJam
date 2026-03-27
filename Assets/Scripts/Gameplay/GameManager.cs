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
	private CanvasGroup _fader;

	[SerializeField]
	private TMP_Text _debugTimeDisplay;

	/** Events **/

	public UnityEvent OnGamePause;
	public UnityEvent OnGameResume;

	/** Fields **/

	// a negative value means game has not started yet
	public float GameTime { get; private set; } = -1f;
	private bool _canPause = true;
	private bool _isPaused = false;

	/** Unity Messages **/

	private void OnEnable()
	{
		_player.OnDie += HandlePlayerDie;

		InputManager.Instance.OnEscapePerformed.AddListener(TogglePause);
	}

	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnDie -= HandlePlayerDie;
		}

		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnEscapePerformed.RemoveListener(TogglePause);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_fader.alpha = 1f;
	}

	private void Start()
	{
		Sequence.Create().ChainDelay(0.1f).Chain(Tween.Alpha(_fader, endValue: 0f, duration: 1f));
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
	}

	// this starts the game
	private void BeginSequence()
	{
		GameTime = 0f;
		_player.DisableInstability = false;
		_player.ResetRepairProgress();

		// start music, etc...
	}

	private void HandlePlayerDie()
	{
		// for now - fade out and reload scene. more fitting transition TBD
		Sequence
			.Create()
			.ChainDelay(0.5f)
			.Chain(Tween.Alpha(_fader, endValue: 1f, duration: 0.6f))
			.ChainCallback(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
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
