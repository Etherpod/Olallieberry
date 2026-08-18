using System;
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
	
	public event TimeZoneEvent OnZoneExpired;
	
	public event TimeZoneEvent OnZoneReset;
	
	[SerializeField] protected bool presenceControlled = true;
	[SerializeField] protected bool manuallyControlled = false;
	[SerializeField] protected float zoneLifespan = 0f;
	[SerializeField] protected bool deactivateOnReset = true;
	[SerializeField] protected bool resetOnDeactivate = true;
	[SerializeField] protected bool resetOnExpire = true;
	[SerializeField] protected float expiryResetDelay = 0f;

	private bool _playerInside = false;
	private bool _probeInside = false;
	private float _expiryResetTime = -1f;
	protected bool _activated = false;
	protected float _elapsedTime = 0f;

	public bool IsManuallyActive => manuallyControlled && _activated;
	public bool IsPresenceActive => presenceControlled && (_playerInside || _probeInside);
	public bool HasLifespan => 0f < zoneLifespan;
	public bool IsExpired => HasLifespan && zoneLifespan <= _elapsedTime;
	
	/// <summary>
	/// Whether the player or probe is currently inside the time zone.
	/// </summary>
	public bool IsActive => (IsManuallyActive || IsPresenceActive) && !IsExpired;

	public float ElapsedTime => _elapsedTime;

	public void OnValidate()
	{
		_triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
		_triggerVolume.Reset();
	}

	public void ActivateZone(bool wasActive = false)
	{
		if (!IsActive || wasActive) return;
		
		OnZoneActivated?.Invoke(this);
	}

	public void ResetZone(bool ignoreDeactivate = false)
	{
		_elapsedTime = 0f;
		if (!ignoreDeactivate && deactivateOnReset) DeactivateZone(true);
		OnZoneReset?.Invoke(this);
	}

	private void DeactivateZone(bool ignoreReset = false)
	{
		_activated = false;
		OnZoneDeactivated?.Invoke(this);
		if (!ignoreReset && resetOnDeactivate) ResetZone(true);
	}

	private void ExpireZone()
	{
		OnZoneExpired?.Invoke(this);
		DeactivateZone();
		if (!resetOnExpire || _elapsedTime == 0f) return;
		
		if (expiryResetDelay == 0f)
		{
			ResetZone(true);
		}
		else
		{
			_expiryResetTime = Time.time + expiryResetDelay;
		}
	}

	public void Activate()
	{
		if (!manuallyControlled || _activated) return;
		
		var wasActive = IsActive;
		_activated = true;
		ActivateZone(wasActive);
	}

	public void Deactivate()
	{
		if (!manuallyControlled || !_activated) return;
		_activated = false;
		DeactivateZone();
	}

	private void FixedUpdate()
	{
		if (0 < _expiryResetTime)
		{
			if (_elapsedTime == 0f) _expiryResetTime = -1f;
			else if (_expiryResetTime <= Time.time)
			{
				ResetZone(true);
				_expiryResetTime = -1f;
			}
		}
		
		if (!IsActive) return;
		
		_elapsedTime += Time.fixedDeltaTime;
		
		UpdateLife();
	}
	
	private void UpdateLife()
	{
		if (!IsExpired) return;
		
		ExpireZone();
	}

	public override void OnEffectVolumeEnter(GameObject hitObj)
	{
		bool wasActive = IsActive;

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

		ActivateZone(wasActive);
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

		if (!IsActive)
		{
			DeactivateZone();
		}
	}
}