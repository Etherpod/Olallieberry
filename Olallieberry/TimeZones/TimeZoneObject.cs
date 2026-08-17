using UnityEngine;

namespace Olallieberry.TimeZones;

public abstract class TimeZoneObject : MonoBehaviour
{
	[SerializeField]
	protected TimeZone _timeZone;
	
	protected bool IsActive => _timeZone.IsActive;
	protected float ElapsedTime => _timeZone.ElapsedTime;

	protected virtual void OnValidate()
	{
		if (_timeZone == null)
			_timeZone = GetComponentInParent<TimeZone>();
	}

	protected virtual void Awake()
	{
		OnValidate();

		if (_timeZone == null) return;

		_timeZone.OnZoneActivated += OnZoneActivated;
		_timeZone.OnZoneDeactivated += OnZoneDeactivated;
		_timeZone.OnZoneExpired += OnZoneExpired;
		_timeZone.OnZoneReset += OnZoneReset;
	}

	/// <summary>
	/// Called when the time zone is activated.
	/// </summary>
	/// <param name="zone"></param>
	protected virtual void OnZoneActivated(TimeZone zone) { }

	/// <summary>
	/// Called when the time zone is deactivated.
	/// </summary>
	/// <param name="zone"></param>
	protected virtual void OnZoneDeactivated(TimeZone zone) { }
	
	protected virtual void OnZoneExpired(TimeZone zone) { }
	
	protected virtual void OnZoneReset(TimeZone zone) { }

	protected virtual void OnDestroy()
	{
		if (_timeZone == null) return;

		_timeZone.OnZoneActivated -= OnZoneActivated;
		_timeZone.OnZoneDeactivated -= OnZoneDeactivated;
		_timeZone.OnZoneExpired -= OnZoneExpired;
		_timeZone.OnZoneReset -= OnZoneReset;
	}
}