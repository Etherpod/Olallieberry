using UnityEngine;

namespace Olallieberry.TimeZones;

public abstract class TimeZoneObject : MonoBehaviour
{
	[SerializeField]
	protected TimeZone _timeZone;

	protected virtual void Awake()
	{
		_timeZone.OnZoneActivated += OnZoneActivated;
		_timeZone.OnZoneDeactivated += OnZoneDeactivated;
	}

	protected virtual void OnZoneActivated(TimeZone zone) { }

	protected virtual void OnZoneDeactivated(TimeZone zone) { }

	protected virtual void OnDestroy()
	{
		_timeZone.OnZoneActivated -= OnZoneActivated;
		_timeZone.OnZoneDeactivated -= OnZoneDeactivated;
	}
}