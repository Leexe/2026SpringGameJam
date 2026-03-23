using UnityEngine;

public class FishCircleVisualsController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private FishCircleController _circle;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[Header("Colors")]
	[SerializeField]
	private Color _growColor = Color.limeGreen;

	[SerializeField]
	private Color _shrinkColor = Color.softRed;

	[Header("Params")]
	[SerializeField]
	private float _baseNoise = 0.03f;

	[SerializeField]
	private float _maxNoise = 0.18f;

	[SerializeField]
	private float _noiseAccel = 3f;

	[SerializeField]
	private float _colorLerpRate = 3f;

	[SerializeField]
	private float _opacityLerpRate = 0.7f;

	//

	private float _noiseRatio; // noise, but 0 to 1
	private float _noiseTime;

	private Color _originalColor;
	private Color _color;
	private float _opacity;

	/** Unity Messages **/

	private void Awake()
	{
		_originalColor = _spriteRenderer.color;
	}

	public void Update()
	{
		_noiseTime += Time.deltaTime;
		_spriteRenderer.material.SetFloat("_time", _noiseTime);

		UpdateSize();
		UpdateNoisiness();
		UpdateColor();
	}

	/** Update Functions **/

	private void UpdateSize()
	{
		float scale = Mathf.Max(1f, 1.2f * _circle.Size);
		_spriteRenderer.transform.localScale = new(scale, scale, 1f);

		_spriteRenderer.material.SetFloat("_radius", 0.5f * _circle.Size);
	}

	private void UpdateNoisiness()
	{
		float targetNoiseRatio = _circle.ChangeRate != 0f ? 1f : 0f;

		_noiseRatio = Mathf.MoveTowards(_noiseRatio, targetNoiseRatio, _noiseAccel * Time.deltaTime);

		// fades targetNoiseRatio down as circle approaches target (slightly hacky)
		float fade = Mathf.Clamp01(Mathf.InverseLerp(0f, 0.25f, Mathf.Abs(_circle.Size - _circle.TargetSize)));
		_noiseRatio = Mathf.Min(_noiseRatio, fade);

		_spriteRenderer.material.SetFloat("_noisiness", Mathf.Lerp(_baseNoise, _maxNoise, _noiseRatio));
	}

	private void UpdateColor()
	{
		Color targetColor = _circle.ChangeRate switch
		{
			> 0f => _growColor,
			< 0f => _shrinkColor,
			_ => _originalColor,
		};

		float targetOpacity = _circle.Enabled ? _originalColor.a : 0f;

		_color = Vector4.MoveTowards(_color, targetColor, _colorLerpRate * Time.deltaTime);
		_opacity = Mathf.MoveTowards(_opacity, targetOpacity, _opacityLerpRate * Time.deltaTime);

		_color.a = _opacity;

		_spriteRenderer.color = _color;
	}
}
