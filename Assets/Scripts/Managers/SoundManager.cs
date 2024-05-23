using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;


public enum AudioGroup { BGM, SFX, Zombie, Vehicle, Size }

public class SoundManager : MonoBehaviour
{
	const string sourcePath = "FX/SFX/OneShotAudioSource";
	const string audioMixerPath = "FX/SFX/AudioMixer";

	AudioMixer mixer;
	[SerializeField] AudioMixerGroup[] mixerGroups = new AudioMixerGroup[(int) AudioGroup.Size];

	private void Awake()
	{
		mixer = GameManager.Resource.Load<AudioMixer>(audioMixerPath);
		for(int i =0;i<(int)AudioGroup.Size; i++)
		{
			mixerGroups[i] = mixer.FindMatchingGroups(((AudioGroup)i).ToString())[0];
		}
	}

	public AudioSource PlayOneShot(AudioClip clip, AudioGroup group, Transform target, bool setParent = true)
	{
		AudioSource source = GameManager.Resource.Instantiate<AudioSource>(sourcePath, target.position, target.rotation, true);
		if(setParent == true)
		{
			source.transform.parent = target;
		}

		source.outputAudioMixerGroup = mixerGroups[(int)group];
		source.GetComponent<FXAutoOff>().SetOffTime(clip.length);
		source.clip = clip;
		source.Play();
		return source;
	}
}