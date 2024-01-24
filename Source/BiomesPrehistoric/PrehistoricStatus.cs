using System.Collections.Generic;
using BiomesPrehistoric.Defs;
using Verse;

namespace BiomesPrehistoric
{
	public static class PrehistoricStatus
	{
		/// <summary>
		/// Cache of pawns that are considered prehistoric animals.
		/// </summary>
		private static HashSet<PawnKindDef> _prehistoricPawns;

		/// <summary>
		/// Cache of races that are considered prehistoric animals.
		/// </summary>
		private static HashSet<ThingDef> _prehistoricRaces;

		/// <summary>
		/// Checks if a def comes from a mod that is considered prehistoric. See PrehistoricModDef for details.
		/// </summary>
		/// <param name="def">Definition to check.</param>
		/// <returns>True if def's mod is prehistoric.</returns>
		private static bool FromPrehistoricMod(Def def)
		{
			return PrehistoricModDef.AllPackageIds.Contains(def.modContentPack?.PackageId);
		}
		public static bool AlwaysSpawn(Def def)
		{
			return PrehistoricModDef.AlwaysSpawnPackageIds.Contains(def.modContentPack?.PackageId);
		}

		/// <summary>
		/// Sets up prehistoric animal status values after game load.
		/// Assumes that mod settings have been loaded correctly.
		/// </summary>
		public static void Initialize()
		{
			_prehistoricPawns = new HashSet<PawnKindDef>();
			_prehistoricRaces = new HashSet<ThingDef>();

			List<PawnKindDef> pawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading;
			for (int pawnIndex = 0; pawnIndex < pawnKindDefs.Count; ++pawnIndex)
			{
				Update(pawnKindDefs[pawnIndex]);
			}
		}

		/// <summary>
		/// Return true for animals that can have a prehistoric status.
		/// </summary>
		/// <param name="raceThingDef">Race of animal being checked.</param>
		/// <returns>True for non-dryad animals.</returns>
		public static bool IsAnimal(ThingDef raceThingDef)
		{
			return raceThingDef?.race != null && raceThingDef.race.Animal && !raceThingDef.race.Dryad;
		}

		public static void Update(PawnKindDef pawnKindDef)
		{
			ThingDef raceDef = pawnKindDef.race;
			if (!IsAnimal(raceDef))
			{
				return;
			}

			_prehistoricPawns.Remove(pawnKindDef);
			_prehistoricRaces.Remove(raceDef);

			bool status =
				PrehistoricSettings.Values.PrehistoricAnimalOverride.TryGetValue(raceDef.defName, out bool overrideValue)
					? overrideValue
					: FromPrehistoricMod(pawnKindDef);

			if (status)
			{
				_prehistoricPawns.Add(pawnKindDef);
				_prehistoricRaces.Add(raceDef);
			}
		}

		/// <summary>
		/// Check if a pawn kind definition should be considered a prehistoric animal.
		/// </summary>
		/// <param name="pawnKindDef">Pawn kind to be checked.</param>
		/// <returns>True if it is considered prehistoric.</returns>
		public static bool IsPrehistoric(PawnKindDef pawnKindDef)
		{
			return _prehistoricPawns.Contains(pawnKindDef);
		}

		public static bool IsPrehistoric(ThingDef thingDef)
		{
			return IsAnimal(thingDef) ? _prehistoricRaces.Contains(thingDef) : FromPrehistoricMod(thingDef);
		}
	}
}