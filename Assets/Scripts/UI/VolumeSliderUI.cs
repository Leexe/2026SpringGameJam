using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderUI : MonoBehaviour
{
	[Header("Sound Sliders")]
	[SerializeField]
	private Slider _masterVolumeSlider;

	[SerializeField]
	private Slider _gameVolumeSlider;

	[SerializeField]
	private Slider _musicVolumeSlider;

	[SerializeField]
	private Slider _ambienceVolumeSlider;

	private void OnEnable()
	{
		// Get Sound Settings
		_masterVolumeSlider.value = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Master);
		_gameVolumeSlider.value = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Game);
		_musicVolumeSlider.value = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Music);
		_ambienceVolumeSlider.value = AudioManager.Instance.GetVolume(AudioManager.AudioBusType.Ambience);
	}

	public void ChangeMasterVolume()
	{
		// if (!AudioManager.Instance.InstanceIsPlaying(_masterTestInstance)) {
		// 	_masterTestInstance.start();
		// }
		AudioManager.Instance.SetVolume(AudioManager.AudioBusType.Master, _masterVolumeSlider.value);
	}

	public void ChangeGameVolume()
	{
		// if (!AudioManager.Instance.InstanceIsPlaying(_gameTestInstance)) {
		// 	_gameTestInstance.start();
		// }
		AudioManager.Instance.SetVolume(AudioManager.AudioBusType.Game, _gameVolumeSlider.value);
	}

	public void ChangeMusicVolume()
	{
		// if (!AudioManager.Instance.InstanceIsPlaying(_musicTestInstance)) {
		// 	_musicTestInstance.start();
		// }
		AudioManager.Instance.SetVolume(AudioManager.AudioBusType.Music, _musicVolumeSlider.value);
	}

	public void ChangeAmbienceVolume()
	{
		// if (!AudioManager.Instance.InstanceIsPlaying(_musicTestInstance)) {
		// 	_musicTestInstance.start();
		// }
		AudioManager.Instance.SetVolume(AudioManager.AudioBusType.Ambience, _ambienceVolumeSlider.value);
	}
}
