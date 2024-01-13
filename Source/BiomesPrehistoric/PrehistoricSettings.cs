using Verse;

namespace BiomesPrehistoric
{
	public enum SpawnOption
	{
		DinoAndVanilla, // Dinos are added to the game, no plants. Vanilla stuff still spawns
		DinoAndPlant, // Prehistoric animals and plants, vanilla stuff still spawns
		DinoWorld // Prehistoric Only
	}

	public class SettingValues
	{
		public SpawnOption spawnOption = SpawnOption.DinoAndPlant;

		// Prehistoric animal commonality. Only active with DinoAndVanilla or DinoAndPlant.
		public int animalCommonality = 100;

		// Prehistoric plant commonality. Only active with DinoAndPlant.
		public int plantCommonality = 100;
	}

	public class PrehistoricSettings : ModSettings
	{
		/// <summary>
		/// Single instance of the setting values of this mod. Uses static for performance reasons.
		/// </summary>
		public static readonly SettingValues Values = new SettingValues();

		// this is what saves the settings when the user closes the game
		public override void ExposeData()
		{
			Scribe_Values.Look(ref Values.spawnOption, "spawnOption", SpawnOption.DinoAndPlant);
			Scribe_Values.Look(ref Values.animalCommonality, "animalCommonality", 100, true);
			Scribe_Values.Look(ref Values.plantCommonality, "plantCommonality", 100, true);
		}
	}
}