namespace Olallieberry;

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