using UnityEngine;

public class PositioningUtils
{
	public static Vector2 SwitchSides()
	{
		return Vector2.zero;
	}

	/// <summary>
	/// Returns a random position within bounds, at a distance between distMin and distMax from <c>point</c>.
	/// Falls back to any point in bounds if no valid position found after 50 tries
	/// </summary>
	public static Vector2 PosAwayFromPoint(Rect bounds, Vector2 point, float distMin, float distMax)
	{
		Vector2[] corners = GetCorners(bounds);

		float maxDistToBounds = 0f;
		foreach (Vector2 corner in corners)
		{
			maxDistToBounds = Mathf.Max(maxDistToBounds, Vector2.Distance(point, corner));
		}

		float actualDistMax = Mathf.Min(distMax, maxDistToBounds);

		const int maxAttempts = 50;
		for (int i = 0; i < maxAttempts; i++)
		{
			Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(distMin, actualDistMax);
			Vector2 candidate = point + offset;

			if (bounds.Contains(candidate))
			{
				return candidate;
			}
		}

		// fallback
		return PosWithinBounds(bounds);
	}

	/// <summary>
	/// Returns a random position within bounds, with an offset from <c>point</c> within the specified X and Y ranges.
	/// Falls back to any point in bounds if no valid position found after 50 tries
	/// </summary>
	public static Vector2 PosOffsetFromPoint(Rect bounds, Vector2 point, float xMin, float xMax, float yMin, float yMax)
	{
		float maxXDistToBounds = Mathf.Max(bounds.xMax - point.x, point.x - bounds.xMin);
		float maxYDistToBounds = Mathf.Max(bounds.yMax - point.y, point.y - bounds.yMin);
		float actualXMax = Mathf.Min(maxXDistToBounds, xMax);
		float actualYMax = Mathf.Min(maxYDistToBounds, yMax);

		const int maxAttempts = 50;
		for (int i = 0; i < maxAttempts; i++)
		{
			Vector2 offset = new(
				Random.Range(xMin, actualXMax) * (Random.value > 0.5f ? 1 : -1),
				Random.Range(yMin, actualYMax) * (Random.value > 0.5f ? 1 : -1)
			);
			Vector2 candidate = point + offset;

			if (bounds.Contains(candidate))
			{
				return candidate;
			}
		}

		// fallback
		return PosWithinBounds(bounds);
	}

	/// <summary>
	/// Given <c>from</c> and <c>point</c>, returns a point behind point from the POV of <c>from</c>
	/// </summary>
	public static Vector2 PosBehindTarget(Vector2 from, Vector2 point, float distMin, float distMax)
	{
		Vector2 offset = point - from;
		return point + (offset.normalized * Random.Range(distMin, distMax));
	}

	/// <summary>
	/// Returns a random position within bounds.
	/// </summary>
	public static Vector2 PosWithinBounds(Rect bounds)
	{
		return new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));
	}

	private static Vector2[] GetCorners(Rect bounds)
	{
		return new Vector2[]
		{
			new(bounds.xMin, bounds.yMin),
			new(bounds.xMax, bounds.yMin),
			new(bounds.xMin, bounds.yMax),
			new(bounds.xMax, bounds.yMax),
		};
	}
}
