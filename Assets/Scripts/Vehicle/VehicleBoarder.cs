using Fusion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class VehicleBoarder : InteractObject, IAfterSpawned
{
	[SerializeField] Transform seatTrans;
	[SerializeField] TextMeshProUGUI debugText;
	[SerializeField] Transform[] getOnObjectTrans;
	[SerializeField] VehicleBody vehicleBody;

	VehicleBehaviour[] vehicleBehaviours;
	PlayerInteract localPlayer;
	const int MAX_PLAYER = 4;
	Rigidbody rb;

	public VehicleBody VehicleBody { get { return vehicleBody; } }

	// 0번 : 운전석, 1번 : 사수, 2 ~ 3 : 장전수
	Vector3[] lastLocalPositions = new Vector3[MAX_PLAYER];
	NetworkId[] localGetOnPlayers = new NetworkId[MAX_PLAYER];
	[Networked, Capacity(MAX_PLAYER), OnChangedRender(nameof(GetOnRender))]
	public NetworkArray<NetworkId> GetOnPlayers => default;

	protected override void Awake()
	{
		base.Awake();
		rb = GetComponent<Rigidbody>();

		if(getOnObjectTrans.Length != MAX_PLAYER)
		{
			Debug.LogError("GetOnObjectTrans는 Max_Player만큼 설정하세요");
			return;
		}

		vehicleBehaviours = new VehicleBehaviour[getOnObjectTrans.Length];
		for (int i = 0; i < vehicleBehaviours.Length; i++)
		{
			if (getOnObjectTrans[i].TryGetComponent(out VehicleBehaviour behaviour))
			{
				vehicleBehaviours[i] = behaviour;
			}
			else
			{
				Debug.LogError($"{i}번째 GetOnObjectTrans에 VehicleBehaviour가 없습니다");
			}
		}
	}

	private void OnValidate()
	{
		if(vehicleBody == null)
		{
			vehicleBody = GetComponent<VehicleBody>();
		}
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		for(int i = 0; i < vehicleBehaviours.Length; i++)
		{
			//게임 종료시 디스폰
			if (vehicleBehaviours[i] == null || vehicleBehaviours[i].Object == null) return;

			if (HasStateAuthority)
			{
				if (localGetOnPlayers[i].IsValid == true)
				{
					vehicleBehaviours[i].GetOff();
				}
			}
			else
			{
				if (localPlayer == null && localGetOnPlayers[i] == localPlayer.Object.Id)
				{
					vehicleBehaviours[i].GetOff();
				}
			}
		}

		for (int i = 0; i < MAX_PLAYER; i++)
		{
			GetOnPlayers.Set(i, new NetworkId());
		}
		GetOnRender();
	}

	private void GetOnRender()
	{
		for(int i = 0; i < MAX_PLAYER; i++)
		{
			if (GetOnPlayers[i] == localGetOnPlayers[i])
				continue;

			if (GetOnPlayers[i].IsValid == false)
			{
				LocalGetOff(localGetOnPlayers[i]);
				localGetOnPlayers[i] = GetOnPlayers[i];
			}
			else 
			{
				localGetOnPlayers[i] = GetOnPlayers[i];
				LocalGetOn(localGetOnPlayers[i]);
			}
		}
	}

	private void LocalGetOn(NetworkId id)
	{
		PlayerInteract player = Runner.FindObject(id).GetComponent<PlayerInteract>();
		player.transform.localScale = Vector3.one * 0.1f;
		player.Teleport(seatTrans.position);
		player.CollisionEnable(false);
		player.transform.parent = seatTrans;
	}

	private void LocalGetOff(NetworkId id)
	{
		PlayerInteract player = Runner.FindObject(id).GetComponent<PlayerInteract>();
		player.transform.localScale = Vector3.one;
		player.CollisionEnable(true);
		player.transform.parent = null;
	}

	public bool GetOn(PlayerInteract player)
	{
		if (player.HasInputAuthority)
		{
			localPlayer = player;
		}
		int idx = AssignIdx(player);
		if (idx == -1)
		{
			return false;
		}

		vehicleBehaviours[idx].Assign(player);
		GetOnPlayers.Set(idx, player.Object.Id);

		if (HasStateAuthority)
		{
			lastLocalPositions[idx] = transform.InverseTransformPoint(player.transform.position);
			player.KCCActive(false);
		}
		return true;
	}

	public void GetOff(PlayerInteract player)
	{
        if (player == null)
        {
			Debug.LogError($"player가 null 입니다");
			return;
        }

        int idx = FindIdx(player.Object.Id);
		if (HasStateAuthority)
		{
			Vector3 pos = transform.TransformPoint(lastLocalPositions[idx]);
			player.KCCActive(true);
			player.Teleport(pos);
		}
		GetOnPlayers.Set(idx, new NetworkId());
	}

	private int AssignIdx(PlayerInteract player)
	{
		Vector3 playerDir = player.transform.position - transform.position;
		playerDir.y = 0f;
		playerDir.Normalize();

		if(Vector3.Angle(playerDir, transform.forward) < 45f)
		{
			if (GetOnPlayers[0].IsValid == true)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}

		for(int i = 1; i < MAX_PLAYER; i++)
		{
			if (GetOnPlayers[i].IsValid == false)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindIdx(NetworkId id)
	{
		for(int i = 0; i < MAX_PLAYER; i++)
		{
			if(id == GetOnPlayers[i])
			{
				return i;
			}
		}
		return -1;
	}

	public override void Render()
	{
		//StringBuilder sb = new StringBuilder();
		//string localPlayerStr = localPlayer == null ? "None" : localPlayer.Object.Id.ToString();
		//sb.AppendLine($"LocalPlayer: {localPlayerStr}");
		//sb.AppendLine("Local List");
		//for (int i = 0; i < MAX_PLAYER; i++)
		//{
		//	sb.Append($"{localGetOnPlayers[i].Raw} / ");
		//}
		//sb.AppendLine("\nNetwork List");
		//for (int i = 0; i < MAX_PLAYER; i++)
		//{
		//	sb.Append($"{GetOnPlayers[i].Raw} / ");
		//}

		//debugText.text = sb.ToString();
	}

	public void AfterSpawned()
	{
		GetOnRender();
	}

	public override void StartInteraction()
	{
		GetOn(playerInteract);
	}
}