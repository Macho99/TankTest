using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TankBehaviour : VehicleBehaviour
{
	const string statUIPrefabPath = "UI/Vehicle/VehicleStatUI";

	protected override void InstantiateStatUI()
	{
		//base.InstantiateStatUI();
		statUI = GameManager.UI.ShowSceneUI<TankStatUI>(statUIPrefabPath);
	}

	protected override void AddEvents()
	{
		base.AddEvents();
	}

	protected override void RemoveEvents()
	{
		base.RemoveEvents();
	}
}