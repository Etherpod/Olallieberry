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
		new Harmony("Olallieberry.BorrowedTomorrows").PatchAll(Assembly.GetExecutingAssembly());
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

			foreach (var line in planet.GetComponentsInChildren<NomaiTextLine>(true))
			{
				line.gameObject.AddComponent<CircleTextLine>();
			}

			foreach (var fluidVolume in planet.GetComponentsInChildren<FluidVolume>(true))
			{
				fluidVolume.ResetAttachedBody(); // some fluid volumes don't have their attached body set correctly, so we reset it to fix that
			}
		}
	}

	public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
	{
		if (newScene != OWScene.SolarSystem) return;
		ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
	}
}

public static class OlallieberryExtensions
{
	public static void Log(this string message)
	{
		Olallieberry.Instance?.ModHelper?.Console?.WriteLine(message);
	}
}