using UnityEngine;

namespace Olallieberry.TimeZones;

[RequireComponent(typeof(OWRigidbody))]
public class TimeZoneRigidbody : TimeZoneObject
{
	private OWRigidbody _rigidbody;
	private OWRigidbody _attachedBody;
	private Vector3 _initialPosition;
	private Quaternion _initialRotation;
	private Vector3 _initialScale;

	protected override void Awake()
	{
		base.Awake();
		_rigidbody = this.GetRequiredComponent<OWRigidbody>();
		_attachedBody = _timeZone.GetAttachedOWRigidbody();
	}

	private void Start()
	{
		_rigidbody.Suspend(_attachedBody);
		transform.parent = _timeZone.transform;
		_initialPosition = transform.localPosition;
		_initialRotation = transform.localRotation;
		_initialScale = transform.localScale;
	}

	protected override void OnZoneActivated(TimeZone zone)
	{
		_rigidbody.Unsuspend(false);
		_rigidbody.SetVelocity(_attachedBody.GetVelocity());
	}
	
	protected override void OnZoneDeactivated(TimeZone zone)
	{
		_rigidbody.Suspend(_attachedBody);
		transform.parent = _timeZone.transform;
		
		transform.localPosition = _initialPosition;
		transform.localRotation = _initialRotation;
		transform.localScale = _initialScale;
	}
}