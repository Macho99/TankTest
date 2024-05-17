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
}