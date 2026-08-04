namespace Olallieberry;

/// <summary>
/// Overrides vanilla debug warps by removing any existing spawn points with the same location.
/// </summary>
public class CustomSpawnPoint : SpawnPoint
{
	private void Start()
	{
		foreach (var spawn in Locator.GetPlayerBody().GetComponent<PlayerSpawner>()._spawnList)
		{
			if (spawn != this && spawn.GetSpawnLocation() == _spawnLocation && 
				spawn._isShipSpawn == _isShipSpawn)
			{
				spawn.SetSpawnLocation(SpawnLocation.None);
			}
		}
	}
}