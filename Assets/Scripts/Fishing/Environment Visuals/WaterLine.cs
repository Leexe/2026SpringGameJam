using UnityEngine;

public class WaterLine : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private LineRenderer _lineRenderer;

	[SerializeField]
	private MeshRenderer _meshRenderer;

	[SerializeField]
	private MeshRenderer _meshRendererBg;

	[SerializeField]
	private MeshFilter _meshFilter;

	[SerializeField]
	private MeshFilter _meshFilterBg;

	[Header("References (optional)")]
	[Tooltip("Reference to the tracked hook rigidbody. If not provided, this script will find it based on Hook Name")]
	[SerializeField]
	private Rigidbody2D _hookRb;

	[Header("Geometry")]
	[SerializeField]
	private int _segmentCount = 64;

	[SerializeField]
	private float _width = 10f;

	[SerializeField]
	private float _baseHeight;

	[Header("Fill Mesh")]
	[SerializeField]
	private bool _enableFillMesh = true;

	[SerializeField]
	private float _fillBottomY = -500f;

	[Header("Spring settings")]
	[SerializeField]
	private float _tension = 0.025f;

	[SerializeField]
	private float _damping = 0.025f;

	[SerializeField]
	private float _spread = 0.05f;

	[Range(1, 16)]
	[SerializeField]
	private int _propagationPasses = 8;

	[Header("Ambient Waves")]
	[SerializeField]
	private bool _enableAmbientWaves = true;

	[Range(0f, 1f)]
	[SerializeField]
	private float _waveAmplitude = 0.1f;

	[Range(0.1f, 5f)]
	[SerializeField]
	private float _waveFrequency = 0.5f;

	[Range(0.1f, 5f)]
	[SerializeField]
	private float _waveSpeed = 1f;

	[Header("Hook interaction")]
	[SerializeField]
	private string _hookName = "Hook";

	[SerializeField]
	private float _splashForce = 3f;

	[SerializeField]
	private float _minSplashInterval = 0.1f;

	[SerializeField]
	private ParticleSystem _splashParticles;

	private const float YRange = 3f;

	private Mesh _fillMesh;

	private Vector3[] _fillVertices;
	private Vector2[] _fillUVs;
	private int[] _fillTriangles;

	private float[] _waveHeights;
	private float[] _xPositions;
	private float[] _yPositions;
	private float[] _velocities;
	private float[] _leftDeltas;
	private float[] _rightDeltas;

	private bool _hookInWater;
	private float _lastSplashTime;

	private Texture2D _heightTex; // info for the shader
	private Color32[] _heightTexColors; // for filling in the heightTex

	private float _dtAccumulator = 0f;

	private const float MAX_VELOCITY = 100f;
	private const float MAX_DISPLACEMENT = 50f;
	private const float SIMULATION_INTERVAL = 0.01f;

	// pad the mesh height, so pixellization isn't cut off
	// we could actually use a giant rectangular mesh instead of making its shape match the waves,
	// but it's fine for now
	private const float MESH_PAD = 1f;

	private float _waveTime;

	private void Awake()
	{
		_lineRenderer.positionCount = _segmentCount;

		_waveHeights = new float[_segmentCount];
		_xPositions = new float[_segmentCount];
		_yPositions = new float[_segmentCount];
		_velocities = new float[_segmentCount];
		_leftDeltas = new float[_segmentCount];
		_rightDeltas = new float[_segmentCount];

		float leftX = transform.position.x - (_width * 0.5f);
		float dx = _width / (_segmentCount - 1);

		for (int i = 0; i < _segmentCount; i++)
		{
			float x = leftX + (dx * i);
			_xPositions[i] = x;
			_yPositions[i] = _baseHeight;
			_velocities[i] = 0f;
		}

		if (_enableFillMesh)
		{
			_fillMesh = new Mesh { name = "WaterFillMesh" };
			_fillMesh.MarkDynamic();
			_meshFilter.sharedMesh = _fillMesh;
			_meshFilterBg.sharedMesh = _fillMesh;

			BuildFillMeshStaticData(_fillMesh);
			UpdateFillMesh(_fillMesh);
		}

		FindHookIfNeeded();
		InitShaderHeightTexture();
		UpdateLineRenderer();
	}

	private void InitShaderHeightTexture()
	{
		_heightTex = new Texture2D(_segmentCount, 1, TextureFormat.RFloat, false) { wrapMode = TextureWrapMode.Clamp };
		_heightTexColors = new Color32[_segmentCount];

		_lineRenderer.material.SetTexture("_Water_Heights", _heightTex);
		_lineRenderer.material.SetFloat("_Start_X", transform.position.x - (_width * 0.5f));
		_lineRenderer.material.SetFloat("_Width", _width);
		_lineRenderer.material.SetVector("_Y_Range", new(-YRange, YRange));

		foreach (MeshRenderer mr in new MeshRenderer[] { _meshRenderer, _meshRendererBg })
		{
			mr.material.SetTexture("_Water_Heights", _heightTex);
			mr.material.SetFloat("_Start_X", transform.position.x - (_width * 0.5f));
			mr.material.SetFloat("_Width", _width);
			mr.material.SetVector("_Y_Range", new(-YRange, YRange));
		}
	}

	private void BuildFillMeshStaticData(Mesh mesh)
	{
		int n = _segmentCount;

		_fillVertices = new Vector3[n * 2];
		_fillUVs = new Vector2[n * 2];
		_fillTriangles = new int[(n - 1) * 6];

		for (int i = 0; i < n; i++)
		{
			float u = n == 1 ? 0f : (float)i / (n - 1);

			_fillUVs[i] = new Vector2(u, 1f);
			_fillUVs[i + n] = new Vector2(u, 0f);
		}

		int t = 0;
		for (int i = 0; i < n - 1; i++)
		{
			int top0 = i;
			int top1 = i + 1;
			int bot0 = i + n;
			int bot1 = i + 1 + n;

			_fillTriangles[t++] = top0;
			_fillTriangles[t++] = top1;
			_fillTriangles[t++] = bot0;

			_fillTriangles[t++] = bot0;
			_fillTriangles[t++] = top1;
			_fillTriangles[t++] = bot1;
		}

		_fillMesh.vertices = _fillVertices;
		_fillMesh.uv = _fillUVs;
		_fillMesh.triangles = _fillTriangles;
	}

	private void Update()
	{
		_waveTime += Time.deltaTime * _waveSpeed;

		_dtAccumulator += Time.deltaTime;
		_dtAccumulator = Mathf.Min(_dtAccumulator, 0.1f); // cap the number of updates

		while (_dtAccumulator > SIMULATION_INTERVAL)
		{
			_dtAccumulator -= SIMULATION_INTERVAL;
			SimulateWater(SIMULATION_INTERVAL);
		}

		HandleHookInteraction();
		UpdateLineRenderer();

		if (_enableFillMesh)
		{
			UpdateFillMesh(_fillMesh);
		}
	}

	private void UpdateFillMesh(Mesh mesh)
	{
		int n = _segmentCount;

		for (int i = 0; i < n; i++)
		{
			_fillVertices[i] = new Vector3(_xPositions[i], _waveHeights[i] + _yPositions[i] + MESH_PAD, 0f);
			_fillVertices[i + n] = new Vector3(_xPositions[i], _fillBottomY, 0f);
		}

		mesh.vertices = _fillVertices;
		mesh.RecalculateBounds();
	}

	private void FindHookIfNeeded()
	{
		if (_hookRb != null)
		{
			return;
		}

		if (!string.IsNullOrEmpty(_hookName))
		{
			var named = GameObject.Find(_hookName);
			if (named != null)
			{
				_hookRb = named.GetComponent<Rigidbody2D>();
			}
		}
	}

	private void SimulateWater(float dt)
	{
		if (_enableAmbientWaves)
		{
			for (int i = 0; i < _segmentCount; i++)
			{
				float normalizedX = (float)i / (_segmentCount - 1);
				float wave1 = Mathf.Sin(_waveTime + (normalizedX * Mathf.PI * 2f * _waveFrequency)) * _waveAmplitude;
				float wave2 =
					Mathf.Sin((_waveTime * 1.3f) + (normalizedX * Mathf.PI * 3f * _waveFrequency))
					* _waveAmplitude
					* 0.5f;
				_waveHeights[i] = wave1 + wave2;
			}
		}

		for (int i = 0; i < _segmentCount; i++)
		{
			float displacement = _yPositions[i] - _baseHeight;
			displacement = Mathf.Clamp(displacement, -MAX_DISPLACEMENT, MAX_DISPLACEMENT);

			_velocities[i] -= _tension * displacement * dt;
			_velocities[i] *= Mathf.Pow(1f - _damping, dt);
			_velocities[i] = Mathf.Clamp(_velocities[i], -MAX_VELOCITY, MAX_VELOCITY);
		}

		for (int pass = 0; pass < _propagationPasses; pass++)
		{
			for (int i = 0; i < _segmentCount; i++)
			{
				_leftDeltas[i] = 0f;
				_rightDeltas[i] = 0f;
			}

			for (int i = 0; i < _segmentCount; i++)
			{
				if (i > 0)
				{
					float diff = _yPositions[i] - _yPositions[i - 1];
					_leftDeltas[i - 1] = _spread * diff;
				}

				if (i < _segmentCount - 1)
				{
					float diff = _yPositions[i] - _yPositions[i + 1];
					_rightDeltas[i + 1] = _spread * diff;
				}
			}

			for (int i = 0; i < _segmentCount; i++)
			{
				_velocities[i] += _leftDeltas[i] + _rightDeltas[i];
				_velocities[i] = Mathf.Clamp(_velocities[i], -MAX_VELOCITY, MAX_VELOCITY);
			}
		}

		for (int i = 0; i < _segmentCount; i++)
		{
			_yPositions[i] += _velocities[i] * dt;
			_yPositions[i] = Mathf.Clamp(
				_yPositions[i],
				_baseHeight - MAX_DISPLACEMENT,
				_baseHeight + MAX_DISPLACEMENT
			);

			if (float.IsNaN(_yPositions[i]) || float.IsInfinity(_yPositions[i]))
			{
				_yPositions[i] = _baseHeight;
				_velocities[i] = 0f;
			}
		}
	}

	private void HandleHookInteraction()
	{
		if (_hookRb == null)
		{
			FindHookIfNeeded();
			if (_hookRb == null)
			{
				return;
			}
		}

		float hookY = _hookRb.transform.position.y;
		bool nowInWater = hookY <= _baseHeight;

		if (!_hookInWater && nowInWater)
		{
			float strength = _splashForce;

			float vy = _hookRb.linearVelocity.y;
			strength = Mathf.Clamp(-vy * 0.3f, 1f, 10f);

			if (Time.time - _lastSplashTime >= _minSplashInterval)
			{
				Splash(_hookRb.transform.position, strength);
				SpawnSplashParticles(_hookRb.transform.position);
				_lastSplashTime = Time.time;
			}
		}

		_hookInWater = nowInWater;
	}

	private void UpdateLineRenderer()
	{
		for (int i = 0; i < _segmentCount; i++)
		{
			float height = _waveHeights[i] + _yPositions[i];
			_lineRenderer.SetPosition(i, new Vector3(_xPositions[i], height, 0f));

			_heightTexColors[i] = new Color(Mathf.InverseLerp(-YRange, YRange, height), 0, 0, 0);
		}
		_heightTex.SetPixels32(_heightTexColors);
		_heightTex.Apply();
	}

	private void Splash(Vector3 worldPos, float velocityAmount)
	{
		int closestIndex = 0;
		float minDist = float.MaxValue;

		for (int i = 0; i < _segmentCount; i++)
		{
			float dist = Mathf.Abs(_xPositions[i] - worldPos.x);
			if (dist < minDist)
			{
				minDist = dist;
				closestIndex = i;
			}
		}

		_velocities[closestIndex] -= Mathf.Clamp(velocityAmount, 0f, 20f);
	}

	public Vector3 GetWorldPosition(int index)
	{
		if (index < 0 || index >= _segmentCount)
		{
			return Vector3.zero;
		}
		return new Vector3(_xPositions[index], _yPositions[index], 0f);
	}

	public void SetColor(Color color)
	{
		Shader.SetGlobalColor("_Reelin_Water_Color", color);
	}

	private void SpawnSplashParticles(Vector3 worldPos)
	{
		if (_splashParticles == null)
		{
			return;
		}

		var spawnPos = new Vector3(worldPos.x, _baseHeight, -1f);
		var rotation = Quaternion.Euler(-90f, 0f, 0f);

		ParticleSystem instance = Instantiate(_splashParticles, spawnPos, rotation);
		instance.Play();
		Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
	}
}
