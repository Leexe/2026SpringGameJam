using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class WarpDriveUI : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _pieces;

	[Button]
	private void UpdateUI(int phase)
	{
		phase = Mathf.Clamp(phase, 0, _pieces.Count);
		for (int i = 0; i < _pieces.Count; i++)
		{
			_pieces[i].SetActive(i < phase);
		}
	}
}
