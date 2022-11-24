using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BiomesPrehistoric
{

	internal struct PrehistoricPackAnimalData
	{
		public float totalWeight;
		public List<PawnGenOption> options;
	}

	/// <summary>
	/// Make prehistoric animals available as pack animals for all factions with caravans.
	/// </summary>
	public static class PrehistoricPackAnimals
	{
		// Keeps a backup of the unmodified list of pack animals for a specific faction.
		private static readonly Dictionary<ushort, List<PawnGenOption>> FactionPackAnimalsBackup =
			new Dictionary<ushort, List<PawnGenOption>>();

		private static PrehistoricPackAnimalData _data;

		private static void SetPrehistoricPackAnimalData()
		{
			if (_data.options != null && _data.options.Count > 0)
			{
				return;
			}

			_data.totalWeight = 0.0f;
			_data.options = new List<PawnGenOption>();
			foreach (var pawnKindDef in DefDatabase<PawnKindDef>.AllDefs)
			{
				if (!pawnKindDef.RaceProps.packAnimal || !Util.IsPrehistoric(pawnKindDef))
				{
					continue;
				}

				var selectionWeight = 1.0f / pawnKindDef.RaceProps.baseBodySize;
				_data.totalWeight += selectionWeight;
				_data.options.Add(new PawnGenOption());
				_data.options.Last().kind = pawnKindDef;
				_data.options.Last().selectionWeight = selectionWeight;
			}
		}

		public static void Patch()
		{
			SetPrehistoricPackAnimalData();

			foreach (var factionDef in DefDatabase<FactionDef>.AllDefs)
			{
				if (factionDef.pawnGroupMakers == null)
				{
					continue;
				}

				foreach (var pawnGroupMaker in factionDef.pawnGroupMakers)
				{
					if (pawnGroupMaker.kindDef == PawnGroupKindDefOf.Trader)
					{
						PatchCarriers(factionDef, pawnGroupMaker);
					}
				}
			}
		}

		private static void PatchCarriers(FactionDef factionDef, PawnGroupMaker traderGroupMaker)
		{
			if (!FactionPackAnimalsBackup.ContainsKey(factionDef.shortHash))
			{
				FactionPackAnimalsBackup[factionDef.shortHash] = new List<PawnGenOption>(traderGroupMaker.carriers);
			}

			if (BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoWorld)
			{
				// When dino world is enabled, the list is just replaced with the available prehistoric animals.
				traderGroupMaker.carriers = _data.options;
				return;
			}

			// When prehistoric and regular animals are both present in the same game, the prehistoric animal commonality
			// setting is taken into account to generate the new values.
			traderGroupMaker.carriers = new List<PawnGenOption>(FactionPackAnimalsBackup[factionDef.shortHash]);

			var regularWeight = traderGroupMaker.carriers.Sum(option => option.selectionWeight);

			var prehistoricAdjustment =
				regularWeight / _data.totalWeight * BiomesPrehistoricMod.mod.settings.animalCommonality / 100.0f;

			foreach (var prehistoricOption in _data.options)
			{
				traderGroupMaker.carriers.Add(new PawnGenOption());
				traderGroupMaker.carriers.Last().kind = prehistoricOption.kind;
				traderGroupMaker.carriers.Last().selectionWeight = prehistoricOption.selectionWeight * prehistoricAdjustment;
			}
		}
	}
}