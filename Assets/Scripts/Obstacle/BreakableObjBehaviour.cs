using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjBehaviour : NetworkBehaviour
{
	public enum BreakType { None, Explosion, AddForce }

	public struct BreakData : INetworkStruct
	{
		public int idx;
		public Vector3 position;
		public float force;
	}

	const int NETWORK_ARR_LENGTH = 100;

	bool isFirstRender = true;

	[SerializeField] List<BreakableObstacle> objList;

	int[] visualArr = new int[NETWORK_ARR_LENGTH];
	int visualBreakCnt;

	[Networked, Capacity(NETWORK_ARR_LENGTH), OnChangedRender(nameof(ChangeRender))]
	public NetworkArray<int> NetworkArr => default;
	[Networked, Capacity(10)]
	public NetworkArray<BreakData> BreakNetworkArr => default;
	[Networked] public int BreakCnt { get; private set; }

	public override void Spawned()
	{
		base.Spawned();
		visualBreakCnt = BreakCnt;
		ChangeRender();
	}

	private void ChangeRender()
	{
		for (int i = 0; i < NETWORK_ARR_LENGTH; i++)
		{
			if (visualArr[i] != NetworkArr[i])
			{
				ChangeApply(i, visualArr[i], NetworkArr[i], isFirstRender);
				visualArr[i] = NetworkArr[i];
			}
		}
		CheckBreakArr();
		isFirstRender = false;
	}

	private void CheckBreakArr()
	{
		for (; visualBreakCnt < BreakCnt; visualBreakCnt++)
		{
			BreakData breakData = BreakNetworkArr[visualBreakCnt % BreakNetworkArr.Length];
			//print(objList[breakData.idx].gameObject.name);
			objList[breakData.idx].BreakEffect(breakData);
		}
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
				objList[idx].OwnerTryBreak(isFirstRender);
			}

			visual >>= 1;
			network >>= 1;
		}
	}

	public void BreakRequest(BreakData breakData)
	{
		BreakNetworkArr.Set(BreakCnt++ % BreakNetworkArr.Length, breakData);
		BreakRequest(breakData.idx);
	}

	public void BreakRequest(int objIdx)
	{
		int arrIdx = objIdx / 32;
		int arrDigit = objIdx % 32;
		int prevValue = NetworkArr[arrIdx];
		int newValue = prevValue | (1 << arrDigit);
		NetworkArr.Set(arrIdx, newValue);
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