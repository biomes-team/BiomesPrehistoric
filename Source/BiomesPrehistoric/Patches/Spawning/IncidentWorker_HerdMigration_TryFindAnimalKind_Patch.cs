using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric.Patches.Spawning
{
	/// <summary>
	/// herd migration event
	/// </summary>
	[HarmonyPatch(typeof(IncidentWorker_HerdMigration), "TryFindAnimalKind")]
	public static class IncidentWorker_HerdMigration_TryFindAnimalKind_Patch
	{
		private static bool Prefix(Map map, ref PawnKindDef animalKind, ref bool __result)
		{
			if (PrehistoricStatus.AlwaysSpawn(animalKind))
			{
				return true;
			}

			if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
			{
				return true;
			}

			__result = DefDatabase<PawnKindDef>.AllDefs
				.Where(k => PrehistoricStatus.IsPrehistoric(k) && k.RaceProps.CanDoHerdMigration &&
				            map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(k.race))
				.TryRandomElementByWeight(x => Mathf.Lerp(0.2F, 1.0F, x.RaceProps.wildness), out animalKind);

			return false;
		}
	}
}