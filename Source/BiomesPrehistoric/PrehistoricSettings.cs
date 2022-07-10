using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric
{

    // this class handles the settings
    public class PrehistoricSettings : ModSettings
    {
        public bool dinoOnly = false;

        // this is what saves the setting when the user closes the game
        public override void ExposeData()
        {
            Scribe_Values.Look(ref dinoOnly, "dinoOnly", false);
        }
    }

    // this class handles the interface
    public class BiomesPrehistoricMod : Mod
    {
        public PrehistoricSettings settings;
        public static BiomesPrehistoricMod mod;
        public BiomesPrehistoricMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<PrehistoricSettings>();
            mod = this;
        }

        public override string SettingsCategory()
        {
            return "BMT_BiomesPrehistoric".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("BMT_DinoOnly".Translate(), ref BiomesPrehistoricMod.mod.settings.dinoOnly);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);

        }
    }
}