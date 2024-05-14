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
	CozyWeather cozyWeather;
	TickTimer weatherChangeTimer;

	private void Awake()
	{
		if(GameManager.Weather != null)
		{ 
			Destroy(gameObject);
			return;
		}

		GameManager.Weather = this;
		cozyWeather = GetComponent<CozyWeather>();
	}

	private void OnDestroy()
	{
		if(GameManager.Weather== this)
		{
			GameManager.Weather = null;
		}
	}

	public override void Spawned()
	{
		base.Spawned();
		int curTime = (int)Runner.RemoteRenderTime + initHour * 60 + initMinute;
		cozyWeather.timeModule.SetMinute((curTime % 60));
		cozyWeather.timeModule.SetHour((curTime % 1440) / 60);
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
			weatherChangeTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(weatherChangeTime.x, weatherChangeTime.y));
			cozyWeather.weatherModule.Ecosystem.SetWeather(weatherDatas[Random.Range(0, weatherDatas.Length)].profile, 20f);
		}
	}
}