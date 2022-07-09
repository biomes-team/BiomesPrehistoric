using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace BiomesPrehistoric
{
    [HarmonyPatch(typeof(WildAnimalSpawner))]
    [HarmonyPatch("SpawnRandomWildAnimalAt")]
    public static class PrehistoricSpawnControl
    {
        private static IEnumerable<CodeInstruction> Transpiler(
          IEnumerable<CodeInstruction> instructions,
          ILGenerator ilg)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label label = ilg.DefineLabel();
            int i = 0;
            foreach (CodeInstruction codeInstruction in codes)
            {
                if (codeInstruction.opcode == OpCodes.Stloc_0)
                {
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, (object)typeof(PrehistoricSpawnControl).GetMethod("DetectPrehistpricOptions"));
                    yield return new CodeInstruction(OpCodes.Brfalse, (object)label);
                    yield return new CodeInstruction(OpCodes.Ret);
                }
                else
                    yield return codeInstruction;
                ++i;
            }
        }

        public static bool DetectPrehistpricOptions(PawnKindDef theCreature) => theCreature != null && (!BiomesPrehistoricMod.settings.flagVanillaAnimals && DefDatabase<ToggleableSpawnDef>.AllDefsListForReading.Where<ToggleableSpawnDef>((Func<ToggleableSpawnDef, bool>)(k => k.defName == "BMT_VanillaAnimalToggles")).RandomElement<ToggleableSpawnDef>().toggleablePawns.Contains(theCreature.defName) || BiomesPrehistoricMod.settings.pawnSpawnStates != null && BiomesPrehistoricMod.settings.pawnSpawnStates.Keys.Contains<string>(theCreature.defName) && BiomesPrehistoricMod.settings.pawnSpawnStates[theCreature.defName]);
    }
}