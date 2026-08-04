using UnityEngine;

namespace Olallieberry.TimeZones;

/// <summary>
/// An effect volume that detects when the player or probe is currently inside of it.
/// Intended to be used with <see cref="TimeZoneObject"/>.
/// </summary>
public class TimeZone : EffectVolume
{
	public delegate void TimeZoneEvent(TimeZone zone);

	/// <summary>
	/// Invoked when a foreign object first enters a zone.
	/// </summary>
	public event TimeZoneEvent OnZoneActivated;
	/// <summary>
	/// Invoked when every foreign object has left a zone.
	/// </summary>
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