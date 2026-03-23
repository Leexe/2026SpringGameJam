using UnityEngine;

[ExecuteAlways]
public class LineController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform _startPosition;

	[SerializeField]
	private Transform _endPosition;

	[SerializeField]
	private LineRenderer _lineRenderer;

	private void Update()
	{
		if (_lineRenderer == null)
		{
			return;
		}

		if (_lineRenderer.positionCount != 2)
		{
			_lineRenderer.positionCount = 2;
		}

		float ownZ = transform.position.z;

		if (_startPosition != null)
		{
			_lineRenderer.SetPosition(0, new(_startPosition.position.x, _startPosition.position.y, ownZ));
		}
		if (_endPosition != null)
		{
			_lineRenderer.SetPosition(1, new(_endPosition.position.x, _endPosition.position.y, ownZ));
		}
	}
}
