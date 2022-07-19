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



    // Uncomment this patch for Dino World to apply to plants

    /*
    [HarmonyPatch(typeof(BiomeDef), "CommonalityOfPlant")]
    public static class PlantSpawnPatch
    {
        static bool Prefix(PawnKindDef plantDef, List<BiomeAnimalRecord> ___wildPlants, ref float __result)
        {
            if (BiomesPrehistoricMod.mod.settings.dinoOnly)
            {
                if (___wildPlants.Any(d => d.animal.modContentPack.Name == "Biomes! Prehistoric"))
                {
                    if (plantDef.modContentPack.Name != "Biomes! Prehistoric")
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
