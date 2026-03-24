using Sirenix.OdinInspector;
using UnityEngine;

public class AudioVisualizerTest : MonoBehaviour
{
	[SerializeField]
	private bool _enableDebugMessages;

	private float[] _frequencyPeaks;
	private int[] _bucketSizes;

	private void Update()
	{
		DebugVisualization();
	}

	[Button]
	private void StartMusic()
	{
		// AudioManager.Instance.SwitchMusic(FMODEvents.Instance.TnShi_Bgm);
		// AudioManager.Instance.SwitchMusic(FMODEvents.Instance.Shop_Bgm);
		AudioManager.Instance.SwitchMusic(FMODEvents.Instance.HAND2HAND_Bgm);
	}

	[Button]
	private void StopMusic()
	{
		AudioManager.Instance.StopMusic();
	}

	private void DebugVisualization()
	{
		if (_enableDebugMessages)
		{
			AudioManager.Instance.GetFrequencyPeaks(
				20,
				ref _frequencyPeaks,
				ref _bucketSizes,
				AudioManager.AudioBusType.Music,
				scaleFactor: 1.0f
			);
			Debug.Log("Frequency Array: " + string.Join(", ", _frequencyPeaks));
		}
	}
}
