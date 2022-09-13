using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            
            BiomesPrehistoricMod.UpdateMainMenuSongDef();
        }

        public static bool IsLargeDino(ThingDef def)
        {
            return def.race?.baseBodySize >= 3f;
        }
    }
}
