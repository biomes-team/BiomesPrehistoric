﻿using RimWorld;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric.Incidents
{
	public class IncidentWorker_SaurumboPasses : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
			{
				return false;
			}

			Map map = (Map) parms.target;
			if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
			{
				return false;
			}

			if (!map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(BiomesPrehistoric_DefOf.BMT_Saurumbo))
			{
				return false;
			}

			IntVec3 cell;
			return TryFindEntryCell(map, out cell);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map) parms.target;
			if (!TryFindEntryCell(map, out var cell))
			{
				return false;
			}

			PawnKindDef saurumbo = PawnKindDef.Named("BMT_Saurumbo");
			int value = GenMath.RoundRandom(StorytellerUtility.DefaultThreatPointsNow(map) / saurumbo.combatPower);
			int max = Rand.RangeInclusive(3, 6);
			value = Mathf.Clamp(value, 2, max);
			int num = Rand.RangeInclusive(90000, 150000);
			IntVec3 result = IntVec3.Invalid;
			if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(cell, map, 10f, out result))
			{
				result = IntVec3.Invalid;
			}

			Pawn pawn = null;
			for (int i = 0; i < value; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(cell, map, 10);
				pawn = PawnGenerator.GeneratePawn(saurumbo);
				GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
				pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num;
				if (result.IsValid)
				{
					pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(result, map, 10);
				}
			}

			SendStandardLetter("BMT_LetterLabelSaurumboPasses".Translate(saurumbo.label).CapitalizeFirst(),
				"BMT_LetterSaurumboPasses".Translate(saurumbo.label), LetterDefOf.PositiveEvent, parms, pawn);
			return true;
		}

		private bool TryFindEntryCell(Map map, out IntVec3 cell)
		{
			return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
		}
	}
}