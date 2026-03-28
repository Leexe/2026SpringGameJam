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
		AudioManager.Instance.SwitchMusic(FMODEvents.Instance.Song_Bgm);
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
			bool status = AudioManager.Instance.GetFrequencyPeaks(
				20,
				ref _frequencyPeaks,
				ref _bucketSizes,
				AudioManager.AudioBusType.Music,
				scaleFactor: 1.0f
			);
			if (status)
			{
				Debug.Log("Frequency Array: " + string.Join(", ", _frequencyPeaks));
			}
		}
	}
}
