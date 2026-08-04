using UnityEngine;

namespace Olallieberry.TimeZones;

/// <summary>
/// An <see cref="OWRigidbody"/> that can move and rotate while its
/// <see cref="TimeZone"/> is active, then reset when the zone deactivates.
/// </summary>
public class TimeZoneAnimatedRigidbody : TimeZoneKinematicRigidbody
{
	public enum MovementMode
	{
		None,

		/// <summary>
		/// Moves from the initial position to the displacement and back.
		/// </summary>
		PingPong
	}

	public enum RotationMode
	{
		None,

		/// <summary>
		/// Rotates indefinitely at a constant speed.
		/// </summary>
		Continuous,

		/// <summary>
		/// Rotates to the configured angular displacement and back.
		/// </summary>
		PingPong
	}

	public enum OscillationMode
	{
		/// <summary>
		/// Smoothly accelerates and decelerates at each end.
		/// </summary>
		Sine,

		/// <summary>
		/// Moves at a constant rate and immediately reverses at each end.
		/// </summary>
		Linear
	}

	/// <summary>
	/// Starting animation time offset in seconds.
	/// </summary>
	[Header("Timing")]
	[Tooltip("Starting animation time offset in seconds.")]
	public float phaseOffset;

	/// <summary>
	/// Determines how the rigidbody moves from its initial position.
	/// </summary>
	[Header("Movement")]
	[Tooltip("Determines how the rigidbody moves from its initial position.")]
	public MovementMode movementMode;

	/// <summary>
	/// Local-space displacement from the object's initial position.
	/// </summary>
	[Tooltip("Local-space displacement from the object's initial position.")]
	public Vector3 movementDisplacement;

	/// <summary>
	/// Time in seconds for one complete out-and-back movement.
	/// </summary>
	[Tooltip("Time in seconds for one complete out-and-back movement.")]
	[Min(0.01f)]
	public float movementCycleDuration = 2f;

	/// <summary>
	/// Determines how the movement animation progresses through its cycle.
	/// </summary>
	[Tooltip("Determines how the movement animation progresses through its cycle.")]
	public OscillationMode movementOscillation = OscillationMode.Sine;

	/// <summary>
	/// Determines how the rigidbody rotates from its initial rotation.
	/// </summary>
	[Header("Rotation")]
	[Tooltip("Determines how the rigidbody rotates from its initial rotation.")]
	public RotationMode rotationMode;

	/// <summary>
	/// Local-space axis around which the object rotates.
	/// </summary>
	[Tooltip("Local-space axis around which the object rotates.")]
	public Vector3 rotationAxis = Vector3.up;

	/// <summary>
	/// Rotation speed used by <see cref="RotationMode.Continuous"/> mode,
	/// in degrees per second.
	/// </summary>
	[Tooltip("Rotation speed used by Continuous mode, in degrees per second.")]
	public float degreesPerSecond = 90f;

	/// <summary>
	/// Maximum angular displacement used by
	/// <see cref="RotationMode.PingPong"/> mode.
	/// </summary>
	[Tooltip("Maximum angular displacement used by PingPong mode.")]
	public float rotationDisplacement = 90f;

	/// <summary>
	/// Time in seconds for one complete out-and-back rotation.
	/// </summary>
	[Tooltip("Time in seconds for one complete out-and-back rotation.")]
	[Min(0.01f)]
	public float rotationCycleDuration = 2f;

	/// <summary>
	/// Determines how the rotation animation progresses through its cycle.
	/// </summary>
	[Tooltip("Determines how the rotation animation progresses through its cycle.")]
	public OscillationMode rotationOscillation = OscillationMode.Sine;

	private bool _animating;
	private float _elapsedTime;

	/// <summary>
	/// Restores the initial state and starts the animation from
	/// <see cref="phaseOffset"/>.
	/// </summary>
	public override void StartFromInitialState()
	{
		base.StartFromInitialState();

		_elapsedTime = phaseOffset;
		_animating = true;
	}

	/// <summary>
	/// Stops the animation and restores the rigidbody's initial state.
	/// </summary>
	public override void ResetToInitialState()
	{
		_animating = false;
		_elapsedTime = 0f;

		base.ResetToInitialState();
	}

	/// <summary>
	/// Advances the animation and applies the required 
	/// velocities to reach the current target pose.
	/// </summary>
	public void FixedUpdate()
	{
		if (!_animating)
			return;

		float deltaTime = Time.fixedDeltaTime;
		_elapsedTime += deltaTime;

		Vector3 localPosition = CalculateLocalPosition();
		Quaternion localRotation = CalculateLocalRotation();

		Vector3 targetPosition =
			_timeZone.transform.TransformPoint(localPosition);

		Quaternion targetRotation =
			_timeZone.transform.rotation * localRotation;

		MoveTowardsPose(
			targetPosition,
			targetRotation,
			deltaTime);
	}

	/// <summary>
	/// Sets the velocities required to reach a target pose
	/// over the given time step.
	/// </summary>
	private void MoveTowardsPose(
		Vector3 targetPosition,
		Quaternion targetRotation,
		float deltaTime
	)
	{
		if (deltaTime <= 0f)
			return;

		Vector3 currentPosition = _rigidbody.GetPosition();
		Quaternion currentRotation = _rigidbody.GetRotation();

		Vector3 velocity =
			(targetPosition - currentPosition) / deltaTime;

		if (_attachedBody != null)
		{
			velocity +=
				_attachedBody.GetPointVelocity(currentPosition);
		}

		Quaternion rotationDelta =
			targetRotation * Quaternion.Inverse(currentRotation);

		rotationDelta.ToAngleAxis(
			out float angle,
			out Vector3 axis);

		if (angle > 180f)
			angle -= 360f;

		Vector3 angularVelocity =
			axis.sqrMagnitude > 0f
				? axis.normalized *
				  angle *
				  Mathf.Deg2Rad /
				  deltaTime
				: Vector3.zero;

		if (_attachedBody != null)
			angularVelocity += _attachedBody.GetAngularVelocity();

		_rigidbody.SetVelocity(velocity);
		_rigidbody.SetAngularVelocity(angularVelocity);
	}

	/// <summary>
	/// Calculates the rigidbody's current animated position in
	/// <see cref="TimeZone"/> local space.
	/// </summary>
	/// <returns>
	/// The current animation local position.
	/// </returns>
	private Vector3 CalculateLocalPosition()
	{
		if (movementMode == MovementMode.None)
			return _initialPosition;

		float progress = EvaluateOscillation(
			_elapsedTime,
			movementCycleDuration,
			movementOscillation
		);

		return _initialPosition + movementDisplacement * progress;
	}

	/// <summary>
	/// Calculates the rigidbody's current animated rotation in
	/// <see cref="TimeZone"/> local space.
	/// </summary>
	/// <returns>
	/// The current animation local rotation.
	/// </returns>
	private Quaternion CalculateLocalRotation()
	{
		if (rotationMode == RotationMode.None)
			return _initialRotation;

		Vector3 axis = rotationAxis.sqrMagnitude > 0f
			? rotationAxis.normalized
			: Vector3.up;

		float angle;

		switch (rotationMode)
		{
			case RotationMode.Continuous:
				angle = _elapsedTime * degreesPerSecond;
				break;

			case RotationMode.PingPong:
				float progress = EvaluateOscillation(
					_elapsedTime,
					rotationCycleDuration,
					rotationOscillation
				);

				angle = rotationDisplacement * progress;
				break;

			default:
				angle = 0f;
				break;
		}

		return _initialRotation * Quaternion.AngleAxis(angle, axis);
	}

	/// <summary>
	/// Evaluates the normalized progress of an oscillation at a given time.
	/// </summary>
	/// <param name="time">
	/// The elapsed animation time in seconds.
	/// </param>
	/// <param name="cycleDuration">
	/// The duration of one complete out-and-back cycle.
	/// </param>
	/// <param name="mode">
	/// The interpolation mode used by the oscillation.
	/// </param>
	/// <returns>
	/// A normalized value between zero and one.
	/// </returns>
	private static float EvaluateOscillation(
		float time,
		float cycleDuration,
		OscillationMode mode
	)
	{
		cycleDuration = Mathf.Max(cycleDuration, 0.01f);

		float cycle = Mathf.Repeat(time / cycleDuration, 1f);

		return mode switch
		{
			OscillationMode.Linear =>
				1f - Mathf.Abs(cycle * 2f - 1f),

			_ =>
				0.5f -
				0.5f * Mathf.Cos(cycle * Mathf.PI * 2f),
		};
	}

	public void OnDrawGizmosSelected()
	{
		if (movementMode == MovementMode.None)
			return;

		Gizmos.color = Color.cyan;

		Vector3 start = transform.position;
		Vector3 end = transform.parent != null
			? transform.parent.TransformPoint(
				transform.localPosition + movementDisplacement)
			: transform.TransformPoint(movementDisplacement);

		Gizmos.DrawLine(start, end);
		Gizmos.DrawWireSphere(end, 0.15f);
	}
}