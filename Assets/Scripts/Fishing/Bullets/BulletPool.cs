using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
	[SerializeField]
	private GameObject _bulletPrefab;

	[SerializeField]
	private int _initialPoolSize = 100;

	private readonly Queue<Bullet> _availableBullets = new Queue<Bullet>();
	private readonly List<Bullet> _activeBullets = new List<Bullet>();

	private void Awake()
	{
		for (int i = 0; i < _initialPoolSize; i++)
		{
			CreateNewBullet();
		}
	}

	private Bullet CreateNewBullet()
	{
		GameObject obj = Instantiate(_bulletPrefab, transform);
		obj.SetActive(false);
		Bullet bullet = obj.GetComponent<Bullet>();
		_availableBullets.Enqueue(bullet);
		return bullet;
	}

	public Bullet GetBullet(Vector2 position, Vector2 velocity, float damage = 10f, float lifetime = 5f)
	{
		if (_availableBullets.Count == 0)
		{
			CreateNewBullet();
		}

		Bullet bullet = _availableBullets.Dequeue();
		bullet.transform.position = position;
		bullet.gameObject.SetActive(true);
		bullet.Initialize(this, velocity, damage, lifetime);
		_activeBullets.Add(bullet);

		return bullet;
	}

	public void ReturnBullet(Bullet bullet)
	{
		if (!bullet.gameObject.activeSelf)
		{
			return;
		}

		bullet.gameObject.SetActive(false);
		_activeBullets.Remove(bullet);
		_availableBullets.Enqueue(bullet);
	}

	public void ReturnAllBullets()
	{
		for (int i = _activeBullets.Count - 1; i >= 0; i--)
		{
			ReturnBullet(_activeBullets[i]);
		}
	}

	public void ReturnAllBullets(bool didWin)
	{
		ReturnAllBullets();
	}
}
