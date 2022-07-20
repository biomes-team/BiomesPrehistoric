using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
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
                if(___wildAnimals?.Any(d => d.animal.modContentPack?.Name == "Biomes! Prehistoric") == true)
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

            if (!BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                return true;
            }

            else
            {
                IEnumerable<PawnKindDef> source = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.RaceProps.Animal && k.canArriveManhunter && (tile == -1 || Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)));
                source = source.Where(k => k.modContentPack?.Name == "Biomes! Prehistoric");
                if (source.Any())
                {
                    if (source.TryRandomElementByWeight((PawnKindDef a) => ManhunterPackIncidentUtility.ManhunterAnimalWeight(a, points), out animalKind))
                    {
                        __result = true;
                        return false;

                    }
                    else if (points > source.Min((PawnKindDef a) => a.combatPower) * 2f)
                    {
                        animalKind = source.MaxBy((PawnKindDef a) => a.combatPower);
                        __result = true;
                        return false;

                    }
                }

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
                    Log.Message("PAWN KIND: " + pawn.kindDef.defName);

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





    // Uncomment this patch for Dino World to apply to plants

    /*
    [HarmonyPatch(typeof(BiomeDef), "CommonalityOfPlant")]
    public static class PlantSpawnPatch
    {
        static bool Prefix(PawnKindDef plantDef, List<BiomeAnimalRecord> ___wildPlants, ref float __result)
        {
            if (BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                if (___wildPlants?.Any(d => d.animal.modContentPack?.Name == "Biomes! Prehistoric") == true)
                {
                    if (plantDef.modContentPack?.Name != "Biomes! Prehistoric")
                    {
                        __result = 0f;
                        return false;
                    }
                }
            }

            return true;
        }
    }
    */



}
