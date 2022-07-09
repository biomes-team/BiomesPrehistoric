using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric
{
    public class BiomesPrehistoricMod : Mod
    {
        public static BiomesPrehistoricSettings settings;

        public BiomesPrehistoricMod(ModContentPack content)
          : base(content)
        {
            BiomesPrehistoricMod.settings = this.GetSettings<BiomesPrehistoricSettings>();
        }

        public override string SettingsCategory() => "Biomes! Prehistoric";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            ToggleableSpawnDef toggleableSpawnDef = DefDatabase<ToggleableSpawnDef>.AllDefsListForReading.Where<ToggleableSpawnDef>((Func<ToggleableSpawnDef, bool>)(k => k.defName == "BMT_ToggleableAnimals")).RandomElement<ToggleableSpawnDef>();
            if (BiomesPrehistoricMod.settings.pawnSpawnStates == null)
                BiomesPrehistoricMod.settings.pawnSpawnStates = new Dictionary<string, bool>();
            foreach (string toggleablePawn in toggleableSpawnDef.toggleablePawns)
            {
                if (!BiomesPrehistoricMod.settings.pawnSpawnStates.ContainsKey(toggleablePawn) && DefDatabase<ThingDef>.GetNamedSilentFail(toggleablePawn) != null)
                    BiomesPrehistoricMod.settings.pawnSpawnStates[toggleablePawn] = false;
            }
            BiomesPrehistoricMod.settings.DoWindowContents(inRect);
        }
    }
}
