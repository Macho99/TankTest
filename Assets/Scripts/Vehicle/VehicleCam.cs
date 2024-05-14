using Cinemachine;
using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleCam : TargetFollower
{
	CinemachineVirtualCamera cam;

	private void Awake()
	{
		cam = GetComponentInChildren<CinemachineVirtualCamera>();
	}

	public void CamActive(bool value)
	{
		cam.gameObject.SetActive(value);
	}

	public override void Update()
	{
		//업데이트 제거(수동으로 render()에서 호출)
	}

	private void LateUpdate()
	{
		ManualUpdate();
	}

	private void OnDisable()
	{
		CamActive(true);
	}
}