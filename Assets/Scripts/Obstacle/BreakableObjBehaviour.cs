using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjBehaviour : NetworkBehaviour
{
	const int NETWORK_ARR_LENGTH = 100;

	bool isFirstRender = true;

	[SerializeField] List<BreakableObstacle> objList;

	private int[] visualArr = new int[NETWORK_ARR_LENGTH];

	[Networked, Capacity(NETWORK_ARR_LENGTH), OnChangedRender(nameof(ChangeRender))]
	public NetworkArray<int> networkArr => default;

	public override void Spawned()
	{
		base.Spawned();
		ChangeRender();
	}

	private void ChangeRender()
	{
		for (int i = 0; i < NETWORK_ARR_LENGTH; i++)
		{
			if (visualArr[i] != networkArr[i])
			{
				ChangeApply(i, visualArr[i], networkArr[i], isFirstRender);
				visualArr[i] = networkArr[i];
			}
		}
		isFirstRender = false;
	}

	private void ChangeApply(int j, int visual, int network, bool isFirstRender)
	{
		int offset = j * 32;
		for(int i = 0; i < 32; i++)
		{
			bool visualBool = (visual & 1) == 1;
			bool networkBool = (network & 1) == 1;

			int idx = offset + i;
			if(visualBool == true && networkBool == false)
			{
				print($"{idx}번째가 local에서는 부서지고, 원격에서는 안부서짐");
			}
			else if(visualBool == false && networkBool == true)
			{
				objList[idx].TryBreak(isFirstRender);
			}

			visual >>= 1;
			network >>= 1;
		}
	}

	public void BreakRequest(int objIdx)
	{
		int arrIdx = objIdx / 32;
		int arrDigit = objIdx % 32;
		int prevValue = networkArr[arrIdx];
		int newValue = prevValue | (1 << arrDigit);
		networkArr.Set(arrIdx, newValue);
	}

	public int RegisterObj(BreakableObstacle obj)
	{
		objList.Add(obj);
		return objList.Count - 1;
	}

	public void ObjReset()
	{
		foreach(var obj in objList)
		{
			obj.idx = -1;
		}

		objList.Clear();
	}
}