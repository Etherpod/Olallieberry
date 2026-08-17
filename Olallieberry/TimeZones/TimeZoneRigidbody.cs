using System;
using UnityEngine;

namespace Olallieberry.TimeZones;

/// <summary>
/// A rigidbody that suspends itself and
/// returns to its initial local transform
/// when a <see cref="TimeZone"/> is deactivated.
/// </summary>
[RequireComponent(typeof(OWRigidbody))]
public class TimeZoneRigidbody : TimeZoneObject
{
	protected OWRigidbody _rigidbody;
	protected OWRigidbody _attachedBody;

	protected Vector3 _initialPosition;
	protected Quaternion _initialRotation;
	protected Vector3 _initialScale;

	protected bool _started = false;

	protected override void Awake()
	{
		base.Awake();

		_rigidbody = this.GetRequiredComponent<OWRigidbody>();

		if (_timeZone == null)
		{
			enabled = false;
			return;
		}

		_attachedBody = _timeZone.GetAttachedOWRigidbody();
	}

	protected virtual void Start()
	{
		if (_timeZone == null || _attachedBody == null)
			return;

		ConfigureRigidbody();

		Suspend();

		_initialPosition = transform.localPosition;
		_initialRotation = transform.localRotation;
		_initialScale = transform.localScale;
		_started = false;
	}

	private void FixedUpdate()
	{
		if (!IsActive) return;

		if (_started) return;
		
		StartFromInitialState();
	}

	/// <summary>
	/// Called before the rigidbody is initially suspended.
	/// Override this to configure kinematic simulation or other settings.
	/// </summary>
	protected virtual void ConfigureRigidbody() { }

	protected override void OnZoneActivated(TimeZone zone)
	{
		Unsuspend();
	}

	protected override void OnZoneDeactivated(TimeZone zone)
	{
		Suspend();
	}

	protected override void OnZoneReset(TimeZone zone)
	{
		ResetToInitialState();
	}

	/// <summary>
	/// Suspends the rigidbody.
	/// </summary>
	public void Suspend()
	{
		_rigidbody.Suspend(_attachedBody);
		transform.parent = _timeZone.transform;
	}

	/// <summary>
	/// Returns the parent body's velocity at the supplied position.
	/// </summary>
	protected Vector3 GetParentPointVelocity(Vector3 worldPosition)
	{
		return _attachedBody != null
			? _attachedBody.GetPointVelocity(worldPosition)
			: Vector3.zero;
	}

	/// <summary>
	/// Returns the parent body's velocity at the current position.
	/// </summary>
	protected Vector3 GetParentPointVelocity()
	{
		return GetParentPointVelocity(transform.position);
	}

	/// <summary>
	/// Unsuspends the rigidbody and sets its velocity.
	/// </summary>
	public void Unsuspend()
	{
		_rigidbody.Unsuspend(false);
		_rigidbody.SetVelocity(GetParentPointVelocity());
	}

	public virtual void StartFromInitialState()
	{
		_started = true;
	}

	/// <summary>
	/// Restores the object's initial local transform.
	/// </summary>
	public virtual void ResetToInitialState()
	{
		transform.localPosition = _initialPosition;
		transform.localRotation = _initialRotation;
		transform.localScale = _initialScale;

		_rigidbody.SetVelocity(Vector3.zero);
		_rigidbody.SetAngularVelocity(Vector3.zero);
		
		_started = false;
	}

	/// <summary>
	/// Suspends the rigidbody and restores its initial local transform.
	/// </summary>
	public void UnsuspendAndStart()
	{
		Unsuspend();
		StartFromInitialState();
	}

	/// <summary>
	/// Suspends the rigidbody and restores its initial local transform.
	/// </summary>
	public void SuspendAndReset()
	{
		Suspend();
		ResetToInitialState();
	}
}