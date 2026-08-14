using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace Olallieberry;

public class Olallieberry : ModBehaviour
{
	public static Olallieberry Instance;
	public INewHorizons NewHorizons;

	public void Awake()
	{
		Instance = this;
		new Harmony("Etherpod.Olallieberry").PatchAll(Assembly.GetExecutingAssembly());
	}

	public void Start()
	{
		NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
		NewHorizons.LoadConfigs(this);
		NewHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);
		
		OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
		LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
	}

	public void OnStarSystemLoaded(string system)
	{
		if (system == "SolarSystem")
		{
			var planet = NewHorizons.GetPlanet("The Ephemeris");

			foreach (var line in planet.GetComponentsInChildren<NomaiTextLine>())
			{
				line.gameObject.AddComponent<CircleTextLine>();
			}
		}
	}

	public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
	{
		if (newScene != OWScene.SolarSystem) return;
		ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
	}
}