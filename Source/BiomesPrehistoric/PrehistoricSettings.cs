using System.Collections.Generic;
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

		/// <summary>
		/// Stores the prehistoric animal status override for each pawnKindDef.race ThingDef.
		/// This should not be accessed directly for read, use PrehistoricStatus instead.
		/// Write access should be followed by PrehistoricStatus.Update.
		/// Keys stored as defName strings to avoid issues when the modlist of a user changes.
		/// Pawns no longer present are silently ignored, and will be deleted if the settings are saved.
		/// </summary>
		public Dictionary<string, bool> PrehistoricAnimalOverride = new Dictionary<string, bool>();
	}

	public class PrehistoricSettings : ModSettings
	{
		/// <summary>
		/// Single instance of the setting values of this mod. Uses static for performance reasons.
		/// </summary>
		public static SettingValues Values = new SettingValues();

		/// <summary>
		/// Set all settings to their default values.
		/// </summary>
		public static void Reset()
		{
			Values = new SettingValues();
		}

		// this is what saves the settings when the user closes the game
		public override void ExposeData()
		{
			Scribe_Values.Look(ref Values.spawnOption, "spawnOption", SpawnOption.DinoAndPlant);
			Scribe_Values.Look(ref Values.animalCommonality, "animalCommonality", 100, true);
			Scribe_Values.Look(ref Values.plantCommonality, "plantCommonality", 100, true);
			Scribe_Collections.Look(ref Values.PrehistoricAnimalOverride, nameof(Values.PrehistoricAnimalOverride), LookMode.Value,
				LookMode.Value);
			if (Scribe.mode == LoadSaveMode.LoadingVars && Values.PrehistoricAnimalOverride == null)
			{
				// Ensure that this dict is initialized after loading older configuration files.
				Values.PrehistoricAnimalOverride = new Dictionary<string, bool>();
			}
		}
	}
}