using System.Collections.Generic;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity;
using PrimeTween;
using UnityEngine;

public class TopTextUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private TypewriterComponent _typewriter;

	[SerializeField]
	private VoiceSO _voiceSO;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[Header("Tween Settings")]
	[SerializeField]
	private float _fadeDuration = 1.5f;

	[SerializeField]
	private float _delay = 1.5f;

	[SerializeField]
	private List<string> _sequenceTexts;
	private int _sequenceIndex;

	private Sequence _fadeTween;
	private int _currentSequenceIndex;

	private void OnEnable()
	{
		GameManager.Instance.OnFadeInFinish.AddListener(PlayFirstText);
		GameManager.Instance.OnFadeOutFinish.AddListener(ResetText);
	}

	private void OnDisable()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.OnFadeInFinish.RemoveListener(PlayFirstText);
			GameManager.Instance.OnFadeOutFinish.RemoveListener(ResetText);
		}
	}

	public void ShowNextSequenceText()
	{
		_fadeTween.Stop();
		_canvasGroup.alpha = 1f;
		_typewriter.ShowText("");
		if (_sequenceIndex < _sequenceTexts.Count)
		{
			_typewriter.ShowText(_sequenceTexts[_sequenceIndex++]);
		}
	}

	public void PlayVoice(CharacterData characterData)
	{
		_voiceSO.PlayVoice(characterData.info.character, 0.5f);
	}

	public void FadeOutText()
	{
		_fadeTween.Stop();
		_fadeTween = Sequence.Create().ChainDelay(_delay).Chain(Tween.Alpha(_canvasGroup, 0f, _fadeDuration));
	}

	private void PlayFirstText()
	{
		ShowNextSequenceText();
	}

	private void ResetText()
	{
		_sequenceIndex = 0;
		_typewriter.ShowText("");
	}
}
