using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric
{
    [HarmonyPatch(typeof(BiomeDef), "CommonalityOfAnimal")]
    public static class AnimalSpawnPatch
    {
        static bool Prefix(PawnKindDef animalDef, List<BiomeAnimalRecord> ___wildAnimals, ref float __result)
        {
            if(BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                if(___wildAnimals?.Any(d => Util.IsPrehistoric(d.animal)) == true)
                {
                    if (animalDef.modContentPack?.Name != "Biomes! Prehistoric")
                    {
                        __result = 0f;
                        return false;
                    }
                }
            }

            return true;
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

            Log.Message("[Biomes! Prehistoric] Evaluating manhunter pack");
            if (!BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                Log.Message("[Biomes! Prehistoric] DINO world is off. Running vanilla manhunter pack");
                return true;
            }

            else
            {
                Log.Message(String.Format("[Biomes! Prehistoric] DINO world is ON. Points: {0}", points));
                IEnumerable<PawnKindDef> source = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.RaceProps.Animal && k.canArriveManhunter && (tile == -1 || Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)));
                source = source.Where(k => Util.IsPrehistoric(k));
                if (source.Any())
                {
                    if (source.TryRandomElementByWeight((PawnKindDef a) => ManhunterPackIncidentUtility.ManhunterAnimalWeight(a, points), out animalKind))
                    {
                        Log.Message("[Biomes! Prehistoric] Branch A: selected " + animalKind?.defName);
                        __result = true;
                        return false;
                    }
                    else if (points > source.Min((PawnKindDef a) => a.combatPower) * 2f)
                    {
                        animalKind = source.MaxBy((PawnKindDef a) => a.combatPower);
                        Log.Message("[Biomes! Prehistoric] Branch B: selected " + animalKind?.defName);
                        __result = true;
                        return false;
                    }
                }

                Log.Message("[Biomes! Prehistoric] Branch C");
                animalKind = null;
                __result = false;
                return false;
            }
            
        }
    }



    /// <summary>
    /// patch replaces pack animals in AI trader caravans with dinos
    /// </summary>
    [HarmonyPatch(typeof(PawnGroupKindWorker_Trader), "GenerateCarriers")]
    public static class TraderPackAnimalPatch
    {
        static void Postfix(ref List<Pawn> outPawns)
        {
            if (BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                foreach(Pawn pawn in outPawns)
                {
                    switch (pawn.kindDef.defName)
                    {
                        case "Muffalo":
                            pawn.kindDef = PawnKindDef.Named("BMT_Pachyrhinosaurus");
                            break;
                        case "Bison":
                            pawn.kindDef = PawnKindDef.Named("BMT_Apatosaurus");
                            break;
                        case "Dromedary":
                            pawn.kindDef = PawnKindDef.Named("BMT_Parasaur");
                            break;
                        case "Yak":
                            pawn.kindDef = PawnKindDef.Named("BMT_Pachyrhinosaurus");
                            break;
                        case "Horse":
                            pawn.kindDef = PawnKindDef.Named("BMT_Gallimimus");
                            break;
                        case "Donkey":
                            pawn.kindDef = PawnKindDef.Named("BMT_Gallimimus");
                            break;
                        case "Elephant":
                            pawn.kindDef = PawnKindDef.Named("BMT_Brachiosaurus");
                            break;
                        case "Alpaca":
                            pawn.kindDef = PawnKindDef.Named("BMT_Iguanodon");
                            break;
                        //default:
                        //    pawn.kindDef = PawnKindDef.Named("BMT_Brachiosaurus");
                        //    break;

                    }

                }
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
            if (!BiomesPrehistoricMod.mod.settings.dinoOnly)
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
            if (!BiomesPrehistoricMod.mod.settings.dinoOnly)
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
            if (!BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                return true;
            }

            __result = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => Util.IsPrehistoric(k) && k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)).TryRandomElementByWeight((PawnKindDef x) => Mathf.Lerp(0.2f, 1f, x.RaceProps.wildness), out animalKind);
            
            return false;
        }
    }



   //bocks thrumbo event
   [HarmonyPatch(typeof(IncidentWorker_ThrumboPasses), "CanFireNowSub")]
   public static class ThrumboPatch
    {
        static bool Prefix(ref bool __result)
        {
            if (!BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }


    //bocks alphabeaver event
    [HarmonyPatch("IncidentWorker_Alphabeavers", "CanFireNowSub")]
    public static class AlphabeaverPatch
    {
        static bool Prefix(ref bool __result)
        {
            if (!BiomesPrehistoricMod.mod.settings.dinoOnly)
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
            if (!BiomesPrehistoricMod.mod.settings.dinoOnly)
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



    // Uncomment this patch for Dino World to apply to plants

    [HarmonyPatch(typeof(BiomeDef), "CommonalityOfPlant")]
    public static class PlantSpawnPatch
    {
        static bool Prefix(PawnKindDef plantDef, List<BiomeAnimalRecord> ___wildPlants, ref float __result)
        {
            if (BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                if (___wildPlants?.Any(d => Util.IsPrehistoric(d.animal)) == true)
                {
                    if (Util.IsPrehistoric(plantDef))
                    {
                        __result = 0f;
                        return false;
                    }
                }
            }

            return true;
        }
    }
    


    // this exists in case we change the definition of "prehistoric" later
    public static class Util
    {
        public static bool IsPrehistoric(Def thing)
        {
            if(thing.modContentPack?.Name == "Biomes! Prehistoric")
            {
                return true;
            }
            return false;

        }
    }

}
