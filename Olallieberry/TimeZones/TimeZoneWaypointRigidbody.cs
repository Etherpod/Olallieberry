using UnityEngine;

namespace Olallieberry.TimeZones;

/// <summary>
/// An <see cref="OWRigidbody"/> that moves between a series of waypoints.
/// </summary>
public class TimeZoneWaypointRigidbody : TimeZoneKinematicRigidbody
{
	public enum PathMode
	{
		/// <summary>
		/// Stops after reaching the final waypoint.
		/// </summary>
		Once,

		/// <summary>
		/// Returns to the first waypoint after reaching the final waypoint.
		/// </summary>
		Loop,

		/// <summary>
		/// Reverses direction after reaching either end of the path.
		/// </summary>
		PingPong
	}

	public enum RotationMode
	{
		/// <summary>
		/// Keeps the rotation of the starting waypoint.
		/// </summary>
		KeepStartingRotation,

		/// <summary>
		/// Gradually rotates to match each target waypoint.
		/// </summary>
		MatchWaypoints
	}

	public enum StartDirection
	{
		Forward,
		Backward
	}

	/// <summary>
	/// The waypoints the rigidbody moves between.
	/// </summary>
	[Header("Waypoints")]
	[Tooltip("The waypoints the rigidbody moves between.")]
	public Transform[] waypoints = [];

	/// <summary>
	/// Movement speed in units per second.
	/// </summary>
	[Header("Movement")]
	[Tooltip("Movement speed in units per second.")]
	[Min(0f)]
	public float moveSpeed = 2f;

	/// <summary>
	/// Time to wait after reaching each waypoint.
	/// </summary>
	[Tooltip("Time to wait after reaching each waypoint.")]
	[Min(0f)]
	public float waitTime = 0f;

	/// <summary>
	/// Controls how strongly the rigidbody slows near a waypoint.
	/// </summary>
	[Tooltip("Controls how strongly the rigidbody slows near a waypoint.")]
	[Min(0f)]
	public float deceleration = 5f;

	/// <summary>
	/// How the rigidbody rotates while following the path.
	/// </summary>
	[Header("Rotation")]
	[Tooltip("How the rigidbody rotates while following the path.")]
	public RotationMode rotationMode =
		RotationMode.KeepStartingRotation;

	/// <summary>
	/// Rotation speed in degrees per second.
	/// </summary>
	[Tooltip("Rotation speed in degrees per second.")]
	[Min(0f)]
	public float rotationSpeed = 45f;

	/// <summary>
	/// What happens when the rigidbody reaches the end of the path.
	/// </summary>
	[Header("Path")]
	[Tooltip("What happens when the rigidbody reaches the end of the path.")]
	public PathMode pathMode = PathMode.Loop;

	/// <summary>
	/// Waypoint used when the rigidbody initializes or resets.
	/// </summary>
	[Header("Startup")]
	[Tooltip("Waypoint used when the rigidbody initializes or resets.")]
	public int startingWaypoint;

	/// <summary>
	/// Direction used when beginning or resetting the path.
	/// </summary>
	[Tooltip("Direction used when beginning or resetting the path.")]
	public StartDirection startDirection = StartDirection.Forward;

	/// <summary>
	/// Whether movement begins when the <see cref="TimeZone"/> activates.
	/// </summary>
	[Tooltip("Whether movement begins when the TimeZone activates.")]
	public bool startMoving = true;

	/// <summary>
	/// Distance at which a waypoint is considered reached.
	/// </summary>
	public static readonly float positionTolerance = 0.001f;

	private int _index;
	private int _direction;

	private bool _active;
	private bool _isMoving;
	private bool _stopAtTarget;

	private float _waitTimer;
	private Vector3 _holdPosition;
	private Quaternion _holdRotation;
	private Quaternion _startingRotation;

	protected override void Awake()
	{
		base.Awake();

		EnsureWaypointExists();
		ResetPathState();
	}

	protected override void Start()
	{
		base.Start();

		MoveToWaypoint(_index);
	}

	protected override void OnValidate()
	{
		base.OnValidate();

		moveSpeed = Mathf.Max(0f, moveSpeed);
		waitTime = Mathf.Max(0f, waitTime);
		deceleration = Mathf.Max(0f, deceleration);
		rotationSpeed = Mathf.Max(0f, rotationSpeed);

		if (HasWaypoints())
		{
			startingWaypoint = Mathf.Clamp(
				startingWaypoint,
				0,
				waypoints.Length - 1);
		}
	}

	public override void StartFromInitialState()
	{
		base.StartFromInitialState();

		ResetPathState();

		_active = true;
		_isMoving = startMoving;

		MoveToWaypoint(_index);
	}

	public override void ResetToInitialState()
	{
		_active = false;
		_isMoving = false;

		ResetPathState();

		base.ResetToInitialState();

		MoveToWaypoint(_index);
	}

	/// <summary>
	/// Updates the platform each physics frame.
	/// The component remains enabled even while the platform is stopped.
	/// </summary>
	public void FixedUpdate()
	{
		if (!_active || !HasWaypoints())
			return;

		if (!_isMoving)
		{
			HoldPosition();
			HoldRotation();
			return;
		}

		if (_waitTimer > 0f)
		{
			HoldPosition();
			HoldRotation();

			_waitTimer = Mathf.Max(
				0f,
				_waitTimer - Time.fixedDeltaTime);

			return;
		}

		Transform target = waypoints[_index];

		if (target == null)
		{
			AdvanceIndex();
			return;
		}

		MoveTowardsWaypoint(target, Time.fixedDeltaTime);
		RotateTowards(target, Time.fixedDeltaTime);
	}

	/// <summary>
	/// Updates the platform's movement toward the current waypoint.
	/// </summary>
	private void MoveTowardsWaypoint(
		Transform target,
		float deltaTime
	)
	{
		Vector3 currentPosition = _rigidbody.GetPosition();
		Vector3 toTarget = target.position - currentPosition;
		float distance = toTarget.magnitude;

		if (distance <= positionTolerance)
		{
			ReachWaypoint(target);
			return;
		}

		float desiredSpeed =
			deceleration * Mathf.Sqrt(distance);

		float distanceLimitedSpeed = deltaTime > 0f
			? distance / deltaTime
			: 0f;

		desiredSpeed = Mathf.Min(
			desiredSpeed,
			moveSpeed,
			distanceLimitedSpeed);

		Vector3 relativeVelocity =
			toTarget.normalized * desiredSpeed;

		_rigidbody.SetVelocity(
			GetParentPointVelocity(currentPosition)
			+ relativeVelocity);
	}

	/// <summary>
	/// Updates the platform's rotation toward the current waypoint.
	/// </summary>
	private void RotateTowards(
		Transform target,
		float deltaTime
	)
	{
		if (deltaTime <= 0f)
			return;

		Quaternion targetRotation = rotationMode switch
		{
			RotationMode.MatchWaypoints => target.rotation,
			_ => _startingRotation
		};

		Quaternion currentRotation = _rigidbody.GetRotation();

		Quaternion nextRotation = Quaternion.RotateTowards(
			currentRotation,
			targetRotation,
			rotationSpeed * deltaTime);

		Quaternion rotationDelta =
			nextRotation * Quaternion.Inverse(currentRotation);

		rotationDelta.ToAngleAxis(
			out float angle,
			out Vector3 axis);

		if (angle > 180f)
			angle -= 360f;

		Vector3 angularVelocity =
			axis.sqrMagnitude > 0f
				? axis.normalized
				  * angle
				  * Mathf.Deg2Rad
				  / deltaTime
				: Vector3.zero;

		if (_attachedBody != null)
		{
			angularVelocity +=
				_attachedBody.GetAngularVelocity();
		}

		_rigidbody.SetAngularVelocity(angularVelocity);
	}

	/// <summary>
	/// Handles arriving at the current waypoint.
	/// </summary>
	private void ReachWaypoint(Transform waypoint)
	{
		_holdPosition = waypoint.position;
		_holdRotation = rotationMode switch
		{
			RotationMode.MatchWaypoints => waypoint.rotation,
			_ => _startingRotation
		};

		HoldPosition();
		HoldRotation();

		if (_stopAtTarget || IsFinished())
		{
			FinishStopping();
			return;
		}

		_waitTimer = waitTime;
		AdvanceIndex();
	}

	private bool IsFinished()
	{
		if (pathMode != PathMode.Once)
			return false;

		return _direction > 0
			? _index >= waypoints.Length - 1
			: _index <= 0;
	}

	/// <summary>
	/// Advances to the next waypoint based on the current movement mode.
	/// </summary>
	private void AdvanceIndex()
	{
		if (waypoints.Length <= 1)
			return;

		_index += _direction;

		switch (pathMode)
		{
			case PathMode.Once:
				_index = Mathf.Clamp(
					_index,
					0,
					waypoints.Length - 1);
				break;

			case PathMode.Loop:
				if (_index >= waypoints.Length)
					_index = 0;
				else if (_index < 0)
					_index = waypoints.Length - 1;
				break;

			case PathMode.PingPong:
				if (_index >= waypoints.Length)
				{
					_direction = -1;
					_index = waypoints.Length - 2;
				}
				else if (_index < 0)
				{
					_direction = 1;
					_index = 1;
				}

				break;
		}
	}

	/// <summary>
	/// Keeps the platform at its last stopped or reached position.
	/// </summary>
	private void HoldPosition()
	{
		_rigidbody.MoveToPosition(_holdPosition);
		_rigidbody.SetVelocity(
			GetParentPointVelocity(_holdPosition));
	}

	/// <summary>
	/// Keeps the platform at its last stopped or reached rotation.
	/// </summary>
	private void HoldRotation()
	{
		_rigidbody.MoveToRotation(_holdRotation);
		_rigidbody.SetAngularVelocity(Vector3.zero);
	}

	/// <summary>
	/// Immediately moves the platform to the specified waypoint.
	/// </summary>
	private void MoveToWaypoint(int waypointIndex)
	{
		if (!HasWaypoints())
			return;

		waypointIndex = Mathf.Clamp(
			waypointIndex,
			0,
			waypoints.Length - 1);

		Transform waypoint = waypoints[waypointIndex];

		if (waypoint == null)
			return;

		_holdPosition = waypoint.position;
		_holdRotation = waypointIndex == startingWaypoint
			? _startingRotation
			: rotationMode switch
			{
				RotationMode.MatchWaypoints => waypoint.rotation,
				_ => _startingRotation
			};

		if (_active) _rigidbody.Suspend();

		HoldPosition();
		HoldRotation();

		if (_active) _rigidbody.Unsuspend(false);
	}

	private void ResetPathState()
	{
		if (!HasWaypoints())
			return;

		_index = Mathf.Clamp(
			startingWaypoint,
			0,
			waypoints.Length - 1);

		_direction = startDirection switch
		{
			StartDirection.Backward => -1,
			_ => 1
		};

		_stopAtTarget = false;
		_waitTimer = 0f;

		Transform waypoint = waypoints[_index];

		if (waypoint != null)
		{
			_holdPosition = waypoint.position;
			_startingRotation = waypoint.rotation;
			_holdRotation = _startingRotation;
		}
		else
		{
			_holdPosition = _rigidbody.GetPosition();
			_startingRotation = transform.rotation;
			_holdRotation = _startingRotation;
		}
	}

	/// <summary>
	/// Creates a default waypoint if none exist.
	/// </summary>
	private void EnsureWaypointExists()
	{
		if (HasWaypoints())
			return;

		GameObject waypointObject =
			new($"{name}_Waypoint");

		waypointObject.transform.SetParent(
			_rigidbody.GetOrigParent());

		waypointObject.transform.SetPositionAndRotation(
			transform.position,
			transform.rotation);

		waypoints = [waypointObject.transform];
	}

	private bool HasWaypoints()
	{
		return waypoints != null && waypoints.Length > 0;
	}

	/// <summary>
	/// Starts or resumes movement toward the current target waypoint.
	/// </summary>
	public void StartMoving()
	{
		_stopAtTarget = false;
		_isMoving = true;
	}

	/// <summary>
	/// Finishes moving to the current target waypoint, then stops there.
	/// If the platform is already waiting at a waypoint, it stops immediately.
	/// </summary>
	public void StopMoving()
	{
		if (!_isMoving)
			return;

		if (_waitTimer > 0f)
		{
			FinishStopping();
			return;
		}

		_stopAtTarget = true;
	}

	/// <summary>
	/// Toggles platform movement.
	/// </summary>
	public void ToggleMoving()
	{
		if (_isMoving)
			StopMoving();
		else
			StartMoving();
	}

	/// <summary>
	/// Changes the current target waypoint.
	/// </summary>
	public void SetWaypoint(
		int waypointIndex,
		bool snapToWaypoint = false
	)
	{
		if (!HasWaypoints())
			return;

		_index = Mathf.Clamp(
			waypointIndex,
			0,
			waypoints.Length - 1);

		_waitTimer = 0f;
		_stopAtTarget = false;

		if (snapToWaypoint)
			MoveToWaypoint(_index);
	}

	/// <summary>
	/// Resets the platform to its configured starting waypoint.
	/// </summary>
	public void ResetToStart()
	{
		if (!HasWaypoints())
			return;

		ResetPathState();
		MoveToWaypoint(_index);
	}

	/// <summary>
	/// Completes a stop
	/// </summary>
	private void FinishStopping()
	{
		_isMoving = false;
		_stopAtTarget = false;
		_waitTimer = 0f;

		HoldPosition();
		HoldRotation();
	}

	public void OnDrawGizmosSelected()
	{
		if (!HasWaypoints())
			return;

		for (int i = 0; i < waypoints.Length; i++)
		{
			Transform waypoint = waypoints[i];

			if (waypoint == null)
				continue;

			Gizmos.color = i == startingWaypoint
				? Color.green
				: Color.cyan;

			Gizmos.DrawWireSphere(
				waypoint.position,
				0.15f);

			Gizmos.DrawLine(
				waypoint.position,
				waypoint.position + waypoint.forward * 0.5f);

			if (i < waypoints.Length - 1)
			{
				Transform next = waypoints[i + 1];

				if (next != null)
				{
					Gizmos.color = Color.white;
					Gizmos.DrawLine(
						waypoint.position,
						next.position);
				}
			}
		}

		if (pathMode == PathMode.Loop && waypoints.Length > 1)
		{
			Transform first = waypoints[0];
			Transform last = waypoints[waypoints.Length - 1];

			if (first != null && last != null)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawLine(
					last.position,
					first.position);
			}
		}
	}
}