using System;
using System.Collections.Generic;
using System.Linq;
using BiomesPrehistoric.Defs;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using RimWorld.Planet;

namespace BiomesPrehistoric
{
    [HarmonyPatch(typeof(BiomeDef), "CommonalityOfAnimal")]
    public static class AnimalSpawnPatch
    {
        static bool Prefix(PawnKindDef animalDef, List<BiomeAnimalRecord> ___wildAnimals, ref float __result)
        {
            if (PrehistoricSettings.Values.spawnOption == SpawnOption.DinoWorld)
            {
                if(___wildAnimals?.Any(d => PrehistoricStatus.IsPrehistoric(d.animal)) == true)
                {
                    if (!PrehistoricStatus.IsPrehistoric(animalDef))
                    {
                        __result = 0f;
                        return false;
                    }
                }
            }

            return true;
        }

        static void Postfix(PawnKindDef animalDef, ref float __result)
        {
            if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld && PrehistoricStatus.IsPrehistoric(animalDef))
            {
                __result *= PrehistoricSettings.Values.animalCommonality;
                __result /= 100.0f;
            }
        }
    }

    [HarmonyPatch(typeof(BiomeDef), nameof(BiomeDef.IsPackAnimalAllowed))]
    internal static class TraderPackAnimalPatch
    {
        private static readonly Dictionary<ushort, HashSet<PawnKindDef>> PrehPackByBiome =
            new Dictionary<ushort, HashSet<PawnKindDef>>();

        static void InitCache(BiomeDef def)
        {
            if (PrehPackByBiome.ContainsKey(def.shortHash))
            {
                return;
            }

            var prehistoricAnimalsInBiome = new HashSet<PawnKindDef>();
            foreach (var animal in def.AllWildAnimals)
            {
                if (animal.RaceProps != null && animal.RaceProps.packAnimal && PrehistoricStatus.IsPrehistoric(animal))
                {
                    prehistoricAnimalsInBiome.Add(animal);
                }
            }

            PrehPackByBiome[def.shortHash] = prehistoricAnimalsInBiome;
        }

        static void Postfix(ref BiomeDef __instance, ref bool __result, ThingDef pawn)
        {
            InitCache(__instance);

            var isPrehistoric = PrehistoricStatus.IsPrehistoric(pawn);
            if (PrehistoricSettings.Values.spawnOption == SpawnOption.DinoWorld && !isPrehistoric)
            {
                __result = false;
            }
            else
            {
                var prehistoricPackAnimals = PrehPackByBiome[__instance.shortHash];

                __result = __result
                           // Assume that biomes without prehistoric pack animals are arctic biomes. In that case,
                           // allow the Borealopelta as a fallback.
                           || prehistoricPackAnimals.Count == 0 && pawn.defName == "BMT_Borealopelta"
                           // Any prehistoric pack animal that can spawn in the biome is allowed in caravans.
                           || prehistoricPackAnimals.Any(pawnKindDef => pawnKindDef.race == pawn);
            }
        }
    }

    /// <summary>
    /// starting pets
    /// </summary>
    [HarmonyPatch(typeof(ScenPart_StartingAnimal), "PossibleAnimals")]
    public static class StartingPetPatch
    {
        static void Postfix(ref IEnumerable<PawnKindDef> __result)
        {
            if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
            {
                return;
            }
            __result = __result.Where(PrehistoricStatus.IsPrehistoric);
        }
    }


    /// <summary>
    /// herd migration event
    /// </summary>
    [HarmonyPatch(typeof(IncidentWorker_HerdMigration), "TryFindAnimalKind")]
    public static class HerdMigrationPatch
    {
        static bool Prefix(int tile, ref PawnKindDef animalKind, ref bool __result)
        {
            if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
            {
                return true;
            }

            __result = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => PrehistoricStatus.IsPrehistoric(k) && k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)).TryRandomElementByWeight((PawnKindDef x) => Mathf.Lerp(0.2f, 1f, x.RaceProps.wildness), out animalKind);
            
            return false;
        }
    }

    //blocks thrumbo event
    [HarmonyPatch(typeof(IncidentWorker_ThrumboPasses), "CanFireNowSub")]
    public static class ThrumboPatch
    {
        static bool Prefix(ref bool __result)
        {
            if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    //blocks alphabeaver event
    [HarmonyPatch("IncidentWorker_Alphabeavers", "CanFireNowSub")]
    public static class AlphabeaverPatch
    {
        static bool Prefix(ref bool __result)
        {
            if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    /// <summary>
    /// trader stock animals
    /// </summary>
    [HarmonyPatch(typeof(StockGenerator_Animals), "SelectionChance")]
    public static class TraderStockAnimalsPatch
    {
        static bool Prefix(PawnKindDef k, ref float __result)
        {
            if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
            {
                return true;
            }

            if (PrehistoricStatus.IsPrehistoric(k))
            {
                return true;
            }

            __result = 0f;
            return false;
        }
    }


    
    // Plant spawn options
    
    [HarmonyPatch(typeof(WildPlantSpawner), "CalculatePlantsWhichCanGrowAt")]
    public static class PlantSpawnerPatch
    {
        static void Postfix(List<ThingDef> outPlants)
        {
            if (PrehistoricSettings.Values.spawnOption == SpawnOption.DinoWorld)
            {
                outPlants.RemoveAll(p => !PrehistoricStatus.IsPrehistoric(p));
            }
            else if(PrehistoricSettings.Values.spawnOption == SpawnOption.DinoAndVanilla)
            {
                outPlants.RemoveAll(PrehistoricStatus.IsPrehistoric);
            }
        }
    }
 
    [HarmonyPatch(typeof(BiomeDef), "CommonalityOfPlant")]
    public static class PlantCommonalityPatch
    {
        static void Postfix(ThingDef plantDef, List<BiomePlantRecord> ___wildPlants, ref float __result)
        {
            if (PrehistoricSettings.Values.spawnOption == SpawnOption.DinoAndPlant && PrehistoricStatus.IsPrehistoric(plantDef))
            {
                __result *= PrehistoricSettings.Values.plantCommonality;
                __result /= 100.0f;
            }
        }
    }

    /// <summary>
    /// foraged food
    /// </summary>
    [HarmonyPatch(typeof(Caravan_ForageTracker), "Forage")]
    public static class CaravanForagePatch
    {
        static bool Prefix(ref Caravan ___caravan)
        {
            if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
            {
                return true;
            }

            ThingDef foragedFood = ThingDef.Named("BMT_RawPineNuts");

            if (foragedFood != null)
            {
                int a = GenMath.RoundRandom(ForagedFoodPerDayCalculator.GetForagedFoodCountPerInterval(___caravan));
                int b = Mathf.FloorToInt((___caravan.MassCapacity - ___caravan.MassUsage) / foragedFood.GetStatValueAbstract(StatDefOf.Mass));
                a = Mathf.Min(a, b);
                while (a > 0)
                {
                    Thing thing = ThingMaker.MakeThing(foragedFood);
                    thing.stackCount = Mathf.Min(a, foragedFood.stackLimit);
                    a -= thing.stackCount;
                    CaravanInventoryUtility.GiveThing(___caravan, thing);
                }
            }
            
            return false;
        }
    }
}
