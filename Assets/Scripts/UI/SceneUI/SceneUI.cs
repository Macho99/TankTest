using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class SceneUI : BaseUI
{
	public override void CloseUI()
	{
		GameManager.UI.CloseSceneUI(this);
	}
}