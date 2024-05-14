using DistantLands.Cozy;
using DistantLands.Cozy.Data;
using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public enum WeatherType
{
	Sunny, Fog, Rain, Storm, Snow
}

[Serializable]
public struct WeatherData
{
	public WeatherType type;
	public WeatherProfile profile;
	public float strength;
}

[RequireComponent(typeof(CozyWeather))]
public class WeatherManager : NetworkBehaviour
{
	[SerializeField] WeatherData[] weatherDatas;
	[SerializeField] Vector2 weatherChangeTime = Vector2.up * 100f;
	[SerializeField] bool clearOnly;
	[SerializeField] int initHour = 9;
	[SerializeField] int initMinute = 0;
	[SerializeField] float timeSpeed = 5f;
	CozyWeather cozyWeather;
	TickTimer weatherChangeTimer;

	int visualWeatherCnt;
	[Networked, OnChangedRender(nameof(WeatherRender))] public int WeatherCnt { get; private set; }
	[Networked] public int TargetTick { get; private set; }
	[Networked] public int WeatherIdx { get; private set; }

	private void Awake()
	{
		if(GameManager.Weather != null)
		{ 
			Destroy(gameObject);
			return;
		}

		GameManager.Weather = this;
		cozyWeather = GetComponent<CozyWeather>();
		print(timeSpeed);
	}

	private void OnDestroy()
	{
		if(GameManager.Weather == this)
		{
			GameManager.Weather = null;
		}
	}

	private void WeatherRender()
	{
		if(visualWeatherCnt < WeatherCnt)
		{
			float leftTime = (TargetTick - Runner.Tick) / Runner.TickRate;
			if(leftTime < 0)
			{
				cozyWeather.weatherModule.Ecosystem.SetWeather(weatherDatas[WeatherIdx].profile);
			}
			else
			{
				cozyWeather.weatherModule.Ecosystem.SetWeather(weatherDatas[WeatherIdx].profile, leftTime);
			}
		}
	}

	public override void Spawned()
	{
		base.Spawned();
		int curTime = (int)(Runner.RemoteRenderTime * timeSpeed) + initHour * 60 + initMinute;
		cozyWeather.timeModule.SetMinute((curTime % 60));
		cozyWeather.timeModule.SetHour((curTime % 1440) / 60);
		if (IsProxy)
		{
			WeatherRender();
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (weatherChangeTimer.ExpiredOrNotRunning(Runner))
		{
			if (clearOnly)
			{
				cozyWeather.weatherModule.Ecosystem.SetWeather(weatherDatas[0].profile);
				return;
			}

			Random.InitState((int)DateTime.Now.Ticks);
			float changeTime = Random.Range(30f, 50f);
			TargetTick = Runner.Tick + (int)(changeTime * Runner.TickRate);
			WeatherIdx = Random.Range(0, weatherDatas.Length);
			weatherChangeTimer = TickTimer.CreateFromSeconds(Runner, 
				changeTime + Random.Range(weatherChangeTime.x, weatherChangeTime.y));
			WeatherCnt++;
		}
	}
}