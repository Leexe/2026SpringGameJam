using UnityEngine;

public class AudioVisualizerUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Material _material;

	[Header("Bar Data")]
	[SerializeField]
	private int _bars = 64;

	[SerializeField]
	private float _tickRate = 0.3f;

	private static readonly int Frequency = Shader.PropertyToID("_Frequency");
	private float[] _frequencyPeaks;
	private int[] _bucketSizes;
	private float[] _zeroArray;
	private float _tickTimer;

	private void OnEnable()
	{
		_zeroArray = new float[_bars];
	}

	private void OnDisable()
	{
		UpdateUI(_zeroArray);
	}

	private void Update()
	{
		_tickTimer += Time.deltaTime;
		if (_tickTimer >= _tickRate)
		{
			if (GetNormalizedPeaks())
			{
				UpdateUI(_frequencyPeaks);
			}

			_tickTimer -= _tickRate;
		}
	}

	private bool GetNormalizedPeaks()
	{
		bool status = AudioManager.Instance.GetFrequencyPeaks(
			_bars,
			ref _frequencyPeaks,
			ref _bucketSizes,
			AudioManager.AudioBusType.Music,
			1f,
			0.05f,
			0.7f
		);

		if (!status)
		{
			return false;
		}

		return true;
	}

	private void UpdateUI(float[] frequencyPeaks)
	{
		_material.SetFloatArray(Frequency, frequencyPeaks);
	}
}
