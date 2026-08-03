using UnityEngine;

namespace Olallieberry.TimeZones;

public class TimeZone : EffectVolume
{
	public delegate void TimeZoneEvent(TimeZone zone);

	public event TimeZoneEvent OnZoneActivated;
	public event TimeZoneEvent OnZoneDeactivated;
	
	private bool _playerInside;
	private bool _probeInside;

    public override void OnEffectVolumeEnter(GameObject hitObj)
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

	public override void OnEffectVolumeExit(GameObject hitObj)
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
}