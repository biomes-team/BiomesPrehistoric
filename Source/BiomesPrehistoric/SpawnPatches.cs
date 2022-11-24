using System;
using System.Collections.Generic;
using System.Linq;
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
            if (BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoWorld)
            {
                if(___wildAnimals?.Any(d => Util.IsPrehistoric(d.animal)) == true)
                {
                    if (!Util.IsPrehistoric(animalDef))
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
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld && Util.IsPrehistoric(animalDef))
            {
                __result *= BiomesPrehistoricMod.mod.settings.animalCommonality;
                __result /= 100.0f;
            }
        }
    }


    /// <summary>
    /// prevents manhunter non-dinos when dinoworld is enabled
    /// </summary>
    [HarmonyPatch(typeof(ManhunterPackIncidentUtility), "TryFindManhunterAnimalKind")]
    public static class ManhunterAnimalPatch
    {
        static bool Prefix(float points, int tile, ref PawnKindDef animalKind, ref bool __result)
        {

            // Log.Message("[Biomes! Prehistoric] Evaluating manhunter pack");
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
            {
                // Log.Message("[Biomes! Prehistoric] DINO world is off. Running vanilla manhunter pack");
                return true;
            }

            else
            {
                // Log.Message(String.Format("[Biomes! Prehistoric] DINO world is ON. Points: {0}", points));
                IEnumerable<PawnKindDef> source = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.RaceProps.Animal && k.canArriveManhunter && (tile == -1 || Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)));
                source = source.Where(k => Util.IsPrehistoric(k));
                if (source.Any())
                {
                    if (source.TryRandomElementByWeight((PawnKindDef a) => ManhunterPackIncidentUtility.ManhunterAnimalWeight(a, points), out animalKind))
                    {
                        // Log.Message("[Biomes! Prehistoric] Branch A: selected " + animalKind?.defName);
                        __result = true;
                        return false;
                    }
                    else if (points > source.Min((PawnKindDef a) => a.combatPower) * 2f)
                    {
                        animalKind = source.MaxBy((PawnKindDef a) => a.combatPower);
                        // Log.Message("[Biomes! Prehistoric] Branch B: selected " + animalKind?.defName);
                        __result = true;
                        return false;
                    }
                }

                // Log.Message("[Biomes! Prehistoric] Branch C");
                animalKind = null;
                __result = false;
                return false;
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
                if (animal.RaceProps != null && animal.RaceProps.packAnimal && Util.IsPrehistoric(animal))
                {
                    prehistoricAnimalsInBiome.Add(animal);
                }
            }

            PrehPackByBiome[def.shortHash] = prehistoricAnimalsInBiome;
        }

        static void Postfix(ref BiomeDef __instance, ref bool __result, ThingDef pawn)
        {
            InitCache(__instance);

            var isPrehistoric = Util.IsPrehistoric(pawn);
            if (BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoWorld && !isPrehistoric)
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
    /// farm animals event
    /// </summary>
    [HarmonyPatch(typeof(IncidentWorker_FarmAnimalsWanderIn), "TryFindRandomPawnKind")]
    public static class FarmAnimalsPatch
    {
        static bool Prefix(Map map, ref PawnKindDef kind, ref bool __result)
        {
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
            {
                return true;
            }

            __result = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => Util.IsPrehistoric(x) && x.RaceProps.Animal && x.RaceProps.wildness < 0.35f && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.race) && !x.RaceProps.Dryad).TryRandomElementByWeight((PawnKindDef k) => SelectionChance(k), out kind);
            return false;
        }

        private static float SelectionChance(PawnKindDef pawnKind)
        {
            float num = 0.420000017f - pawnKind.RaceProps.wildness;
            if (PawnUtility.PlayerHasReproductivePair(pawnKind))
            {
                num *= 0.5f;
            }
            return num;
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
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
            {
                return;
            }
            __result = __result.Where(p => Util.IsPrehistoric(p));
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
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
            {
                return true;
            }

            __result = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => Util.IsPrehistoric(k) && k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)).TryRandomElementByWeight((PawnKindDef x) => Mathf.Lerp(0.2f, 1f, x.RaceProps.wildness), out animalKind);
            
            return false;
        }
    }



   //blocks thrumbo event
   [HarmonyPatch(typeof(IncidentWorker_ThrumboPasses), "CanFireNowSub")]
   public static class ThrumboPatch
    {
        static bool Prefix(ref bool __result)
        {
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
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
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
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
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
            {
                return true;
            }

            if (Util.IsPrehistoric(k))
            {
                return true;
            }

            __result = 0f;
            return false;
        }
    }









    // Plant spawn options

    [HarmonyPatch(typeof(BiomeDef), "CommonalityOfPlant")]
    public static class PlantSpawnPatch
    {
        static void Postfix(ThingDef plantDef, List<BiomePlantRecord> ___wildPlants, ref float __result)
        {
            if (BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoWorld)
            {
                if(___wildPlants.Any(p => Util.IsPrehistoric(p.plant)))
                {
                    if(!Util.IsPrehistoric(plantDef))
                    {
                        __result *= 0.000000001f;
                    }
                }
            }
            else if(BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoAndVanilla)
            {
                if(Util.IsPrehistoric(plantDef))
                {
                    __result *= 0.000000001f;
                }
            }
            else if(Util.IsPrehistoric(plantDef))
            {
                __result *= BiomesPrehistoricMod.mod.settings.plantCommonality;
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
            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
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




    // this exists in case we change the definition of "prehistoric" later
    public static class Util
    {
        private static readonly HashSet<string> PrehistoricPackageIds = new HashSet<string>
        {
            "biomesteam.biomesprehistoric",
            "regrowth2.extinctanimals",
            "regrowth.botr.extinctanimalspack",
            "spincrus.dinosauria",
            "spino.megafauna"
        };

        public static bool IsPrehistoric(Def thing)
        {
            return PrehistoricPackageIds.Contains(thing.modContentPack?.PackageId);
        }
    }

}
