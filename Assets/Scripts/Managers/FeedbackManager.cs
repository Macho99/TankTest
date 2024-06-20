using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
	[SerializeField] MMF_Player camImpulse;
	[SerializeField] MMF_Player clap;

	MMF_CinemachineImpulse impulseFeedback;

	private void Awake()
	{
		impulseFeedback = camImpulse.GetFeedbackOfType<MMF_CinemachineImpulse>();
	}

	public void PlayImpulse(Vector3 position, Vector3 shakeVelocity, float radius, float sustainTime)
	{
		camImpulse.transform.position = position;
		impulseFeedback.Velocity = shakeVelocity;
		impulseFeedback.m_ImpulseDefinition.m_ImpactRadius = radius;
		impulseFeedback.m_ImpulseDefinition.m_DissipationDistance = radius * 2f;
		impulseFeedback.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = sustainTime;

		camImpulse.PlayFeedbacks();
	}

	public void PlayClap(Vector3 position, float radius)
	{
		Vector3 camPos = Camera.main.transform.position;
		if((camPos - position).sqrMagnitude < radius * radius)
		{
			Vector3 viewportPos = Camera.main.WorldToViewportPoint(position);
			bool isVisible = viewportPos.x >= 0 && viewportPos.x <= 1 &&
						 viewportPos.y >= 0 && viewportPos.y <= 1 &&
						 viewportPos.z > 0;
			if(isVisible)
			{
				clap.PlayFeedbacks();
			}
		}
	}
}