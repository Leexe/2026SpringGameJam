using UnityEngine;
using UnityEngine.UI;

public class CombatHealthUI : MonoBehaviour
{
	[Header("Debug")]
	[field: SerializeField]
	public HealthController AttachedHealthController { get; private set; }

	[field: SerializeField]
	public Transform AttachedTransform { get; private set; }

	[Header("References")]
	[SerializeField]
	private RectTransform _ownTransform; // yikes

	[SerializeField]
	private RectTransform _parentCanvasTransform;

	[SerializeField]
	private Image _barFill;

	private void OnEnable()
	{
		if (AttachedHealthController != null && AttachedTransform != null)
		{
			AttachTo(AttachedTransform, AttachedHealthController);
		}
	}

	private void OnDisable()
	{
		Detach();
	}

	private void Update()
	{
		if (Camera.main != null && AttachedTransform != null)
		{
			Vector2 viewportPosition = Camera.main.WorldToViewportPoint(AttachedTransform.position);
			Vector2 screenPos = new(
				(viewportPosition.x * _parentCanvasTransform.sizeDelta.x) - (_parentCanvasTransform.sizeDelta.x * 0.5f),
				(viewportPosition.y * _parentCanvasTransform.sizeDelta.y) - (_parentCanvasTransform.sizeDelta.y * 0.5f)
			);

			_ownTransform.anchoredPosition = screenPos;
		}
	}

	public void AttachTo(Transform transform, HealthController healthController)
	{
		if (healthController == null || transform == null)
		{
			return;
		}

		Detach();

		AttachedHealthController = healthController;
		AttachedHealthController.OnDamage.AddListener(UpdateHealthVisual);
		AttachedHealthController.OnHeal.AddListener(UpdateHealthVisual);
		AttachedHealthController.OnRegen.AddListener(UpdateHealthVisual);
		AttachedHealthController.OnRevive.AddListener(UpdateHealthVisual);

		UpdateHealthVisual(healthController.GetHealth, healthController.GetMaxHealth);

		AttachedTransform = transform;
	}

	public void Detach()
	{
		if (AttachedHealthController != null)
		{
			AttachedHealthController.OnDamage.RemoveListener(UpdateHealthVisual);
			AttachedHealthController.OnHeal.RemoveListener(UpdateHealthVisual);
			AttachedHealthController.OnRegen.RemoveListener(UpdateHealthVisual);
			AttachedHealthController.OnRevive.RemoveListener(UpdateHealthVisual);
			AttachedHealthController = null;
		}
	}

	private void UpdateHealthVisual(float currentHealth, float maxHealth)
	{
		if (_barFill == null)
		{
			return;
		}

		float normalizedHealth = maxHealth > 0 ? currentHealth / maxHealth : 0f;
		_barFill.fillAmount = 1f - normalizedHealth;
	}
}
