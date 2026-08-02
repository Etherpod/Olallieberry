using UnityEngine;

namespace Olallieberry.TimeZones;

public class TimeZone : MonoBehaviour
{
	public delegate void TimeZoneEvent(TimeZone zone);

	public event TimeZoneEvent OnZoneActivated;
	public event TimeZoneEvent OnZoneDeactivated;
	
	private OWTriggerVolume _triggerVolume;
	private bool _playerInside;
	private bool _probeInside;

	private void Awake()
	{
		_triggerVolume = this.GetRequiredComponent<OWTriggerVolume>();
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;

		// set in code because I hate having to deal with Unity auto-assigning this layer
		// to every child of the zone
		// I might actually just make an editor script that sets the layer to default every time
		gameObject.layer = LayerMask.NameToLayer("BasicEffectVolume");
	}

	private void OnEntry(GameObject hitObj)
	{
		bool wasEmpty = !_playerInside && !_probeInside;
		
		if (!_playerInside && hitObj.CompareTag("PlayerDetector"))
		{
			_playerInside = true;
		}
		else if (!_probeInside && hitObj.CompareTag("ProbeDetector"))
		{
			_probeInside = true;
		}
		else
		{
			return;
		}

		if ((_playerInside || _probeInside) && wasEmpty)
		{
			OnZoneActivated?.Invoke(this);
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (_playerInside && hitObj.CompareTag("PlayerDetector"))
		{
			_playerInside = false;
		}
		else if (_probeInside && hitObj.CompareTag("ProbeDetector"))
		{
			_probeInside = false;
		}
		else
		{
			return;
		}

		if (!_playerInside && !_probeInside)
		{
			OnZoneDeactivated?.Invoke(this);
		}
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
	}
}