using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TankBehaviour : VehicleBehaviour
{
	const string statUIPrefabPath = "UI/Vehicle/Tank/TankStatUI";
	
	protected TankBody tankBody;

	protected override void Awake()
	{
		base.Awake();
		tankBody = vehicleBody as TankBody;
		if(tankBody == null)
		{
			Debug.LogError("TankBody 컴포넌트를 붙혀주세요");
		}
	}

	protected override void InstantiateStatUI()
	{
		//base.InstantiateStatUI();
		this.statUI = GameManager.UI.ShowSceneUI<TankStatUI>(statUIPrefabPath);
		base.statUI = this.statUI;
	}
}