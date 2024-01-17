using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BiomesPrehistoric.Patches.PrehistoricWorld
{
	/// <summary>
	/// When the Prehistoric World setting is enabled, only allow prehistoric farm animals to be spawned through the farm animals wander in event.
	/// </summary>
	[HarmonyPatch(typeof(IncidentWorker_FarmAnimalsWanderIn), "TryFindRandomPawnKind")]
	public static class IncidentWorker_FarmAnimalsWanderIn_IncidentWorker_FarmAnimalsWanderIn_Patch
	{
		public static bool Prefix(Map map, ref PawnKindDef kind, ref bool __result)
		{
			if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
			{
				return true;
			}

			List<PawnKindDef> availableFarmAnimals = new List<PawnKindDef>();
			foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading)
			{
				if (pawnKindDef.RaceProps.Animal && PrehistoricStatus.IsPrehistoric(pawnKindDef) && pawnKindDef.RaceProps.wildness < 0.35F &&
				    map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(pawnKindDef.race) &&
				    !pawnKindDef.race.tradeTags.NullOrEmpty() && pawnKindDef.race.tradeTags.Contains("AnimalFarm") &&
				    !pawnKindDef.RaceProps.Dryad)
				{
					availableFarmAnimals.Add(pawnKindDef);
				}
			}

			__result = availableFarmAnimals.TryRandomElementByWeight(SelectionChance, out kind);
			return false;
		}

		public static float SelectionChance(PawnKindDef pawnKind)
		{
			float num = 0.42F - pawnKind.RaceProps.wildness;
			if (PawnUtility.PlayerHasReproductivePair(pawnKind))
			{
				num *= 0.5f;
			}

			return num;
		}
	}
}