using System;
using System.Reflection;
using UnityEngine;

namespace Olallieberry;

/// <summary>
/// Overrides vanilla debug warps by removing any existing spawn points with the same location.
/// </summary>
public class CustomSpawnPoint : SpawnPoint
{
	private bool _default = true;

	public void Start()
	{
		GlobalMessenger.AddListener("WakeUp", Spawn);

		foreach (var spawn in Locator.GetPlayerBody().GetComponent<PlayerSpawner>()._spawnList)
		{
			if (spawn != this && spawn.GetSpawnLocation() == _spawnLocation && 
				spawn._isShipSpawn == _isShipSpawn)
			{
				spawn.SetSpawnLocation(SpawnLocation.None);
			}
		}
	}

	public void OnDestroy()
	{
		GlobalMessenger.RemoveListener("WakeUp", Spawn);
	}

	public void Spawn()
	{
		if (_default)
		{
			Olallieberry.Instance.ModHelper.Events.Unity.FireInNUpdates(() =>
			{
				if (!_isShipSpawn)
				{
					GameObject.FindObjectOfType<PlayerSpawner>().DebugWarp(this);
					SuitUp();
				}
				else
				{
					var body = Locator.GetShipBody();
					var ship = body.gameObject;
					var pos = transform.position;

					foreach (var landingPadSensor in ship.GetComponentsInChildren<LandingPadSensor>())
					{
						landingPadSensor._contactBody = null;
					}

					body.WarpToPositionRotation(pos, transform.rotation);

					var spawnVelocity = _attachedBody.GetVelocity();
					var spawnAngularVelocity = _attachedBody.GetPointTangentialVelocity(pos);
					var velocity = spawnVelocity + spawnAngularVelocity;

					body.SetVelocity(velocity);
				}
			}, 4);
		}
	}

	public static void SuitUp()
	{
		if (!Locator.GetPlayerController()._isWearingSuit)
		{
			Locator.GetPlayerSuit().SuitUp(false, true, true);
		}
	}
}