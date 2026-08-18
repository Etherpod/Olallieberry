using UnityEngine;

namespace Olallieberry;

public class OrbSpinController : MonoBehaviour
{
	[SerializeField] private float spinRate = 6;
	[SerializeField] private Vector3 spinAxis = Vector3.up;

	private float spinRateMultiplier = 0;

	private void OnEnable()
	{
		spinRateMultiplier = 360 / spinRate;
	}

	private void FixedUpdate()
	{
		if (OWTime.IsPaused()) return;
		
		transform.Rotate(
			spinAxis.normalized,
			Time.fixedDeltaTime * spinRateMultiplier,
			Space.Self
		);
	}
}