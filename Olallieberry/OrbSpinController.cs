using UnityEngine;

namespace Olallieberry;

public class OrbSpinController : MonoBehaviour
{
	[SerializeField] private float spinRate = 6;
	[SerializeField] private Vector3 spinAxis = new Vector3(1, 0, -1); // the gear is diagonal for some reason, so we do this to spin it like a gear

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