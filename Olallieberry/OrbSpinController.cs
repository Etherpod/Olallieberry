using System;
using UnityEngine;

namespace Olallieberry;

public class OrbSpinController : MonoBehaviour
{
	[SerializeField] private float spinRate;

	private float spinRateMultiplier = 0;

	private void OnEnable()
	{
		spinRateMultiplier = 360 / spinRate;
	}

	private void FixedUpdate()
	{
		if (OWTime.IsPaused()) return;
		
		transform.Rotate(Vector3.up, Time.fixedDeltaTime * spinRateMultiplier, Space.Self);
	}
}