using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamFire : MonoBehaviour
{
	[SerializeField] Image aimImg;

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			print("누름");
			aimImg.color = Color.red;
			Fire();
		}
		if (Input.GetMouseButtonUp(0))
		{
			print("뗌");
			aimImg.color = Color.white;
		}
	}

	private void Fire()
	{
		if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100f, 
			LayerMask.GetMask("Monster")))
		{
			ZombieHitBox zombieHit = hit.collider.GetComponent<ZombieHitBox>();
			if (zombieHit == null)
			{
				Debug.LogError("좀비 HitBox가 없는 Monster Layer가 있습니다");
				return;
			}

			zombieHit.ApplyDamage(-transform.forward, 1f, 1);
		}
	}
}
