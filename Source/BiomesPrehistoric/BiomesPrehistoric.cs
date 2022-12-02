using HarmonyLib;
using Verse;

namespace BiomesPrehistoric
{
    [StaticConstructorOnStartup]
    static class BiomesPrehistoric
    {
        static BiomesPrehistoric()
        {
            Harmony harmony = new Harmony("rimworld.biomesprehistoric");

            harmony.PatchAll();
            Log.Message("Biomes! Prehistoric initialized");
            
        //    BiomesPrehistoricMod.UpdateMainMenuSongDef();
        }

        public static bool IsLargeDino(ThingDef def)
        {
            return def.race?.baseBodySize >= 3f;
        }
    }
}
