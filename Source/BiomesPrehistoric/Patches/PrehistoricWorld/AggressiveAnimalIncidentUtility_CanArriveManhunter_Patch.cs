using HarmonyLib;
using RimWorld;
using Verse;

namespace BiomesPrehistoric.Patches.PrehistoricWorld
{
	/// <summary>
	/// Filter out any non-prehistoric animal from the list of available manhunter animals.
	/// </summary>
	[HarmonyPatch(typeof(AggressiveAnimalIncidentUtility), "CanArriveManhunter")]
	public static class ManhunterPackIncidentUtility_CanArriveManhunter_Patch
	{
		public static void Postfix(ref bool __result, PawnKindDef kind)
		{
			__result = __result && (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld ||
			                        PrehistoricStatus.IsPrehistoric(kind));
		}
	}
}