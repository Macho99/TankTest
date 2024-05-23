using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZombieSoundType { None, Idle, Attack, Trace, Hit, Die, Eat }

[CreateAssetMenu(fileName = "ZombieSoundSO", menuName = "SO/ZombieSound", order = 1)]
public class ZombieSoundSO : ScriptableObject
{
	[SerializeField] AudioClip[] idleClips;
	[SerializeField] AudioClip[] attackClips;
	[SerializeField] AudioClip[] traceClips;
	[SerializeField] AudioClip[] hitClips;
	[SerializeField] AudioClip[] dieClips;
	[SerializeField] AudioClip[] eatClips;

	public AudioClip GetClip(ZombieSoundType type, int cnt, int id)
	{
		AudioClip[] clips;
		switch (type)
		{
			case ZombieSoundType.Idle:
				clips = idleClips;
				break;
			case ZombieSoundType.Attack:
				clips = attackClips;
				break;
			case ZombieSoundType.Trace:
				clips = traceClips;
				break;
			case ZombieSoundType.Hit:
				clips = hitClips;
				break;
			case ZombieSoundType.Die: 
				clips = dieClips;
				break;
			case ZombieSoundType.Eat:
				clips = eatClips;
				break;
			default:
				Debug.LogError($"{type}에 해당하는 클립을 등록하세요");
				return null;
		}

		Random.InitState(cnt * id);
		return clips[Random.Range(0, clips.Length)];
	}
}
