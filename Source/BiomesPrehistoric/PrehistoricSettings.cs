using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric
{
    public enum SpawnOption
    {
        DinoAndVanilla,           // Dinos are added to the game, no plants. Vanilla stuff still spawns
        DinoAndPlant,       // Prehistoric animals and plants, vanilla stuff still spawns
        DinoWorld           // Prehistoric Only
    }
    // this class handles the settings
    public class PrehistoricSettings : ModSettings
    {
        public SpawnOption spawnOption = SpawnOption.DinoAndPlant;

        // this is what saves the setting when the user closes the game
        public override void ExposeData()
        {
            Scribe_Values.Look(ref spawnOption, "spawnOption", SpawnOption.DinoAndPlant);
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
            Listing_Standard mainListing = new Listing_Standard();
            mainListing.Begin(inRect);

            //Dino and vanilla
            SpawnPicker(mainListing, BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoAndVanilla, delegate { BiomesPrehistoricMod.mod.settings.spawnOption = SpawnOption.DinoAndVanilla; }, "BMT_DinoAndVanillaLabel", "BMT_DinoAndVanillaDesc", "DinoAndVanilla");

            // Dino and plants
            SpawnPicker(mainListing, BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoAndPlant, delegate { BiomesPrehistoricMod.mod.settings.spawnOption = SpawnOption.DinoAndPlant; }, "BMT_DinoAndPlantLabel", "BMT_DinoAndPlantDesc", "DinoAndPlant");
            
            // DINO WORLD!!!
            SpawnPicker(mainListing, BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoWorld, delegate { BiomesPrehistoricMod.mod.settings.spawnOption = SpawnOption.DinoWorld; }, "BMT_DinoWorldLabel", "BMT_DinoWorldDesc", "DinoWorld");
            
            mainListing.End();
            base.DoSettingsWindowContents(inRect);

        }

        public static void SpawnPicker(Listing_Standard listing, bool active, Action action, string label, string desc, string iconPath)
        {
            float height = 180f;
            Rect mainRect = new Rect(listing.GetRect(height));
            if (active)
            {
                Widgets.DrawHighlight(mainRect);
            }

            Rect iconRect = mainRect;
            Rect textRect = mainRect;
            iconRect.xMax -= mainRect.width - height;
            textRect.xMin += height + 10f;
            textRect.xMax -= 10f;
            iconRect.xMin = iconRect.xMax - height;

            string texPath = String.Format("BMT_Prehistoric/UI/{0}", iconPath);
            Texture2D icon = ContentFinder<Texture2D>.Get(texPath, true);
            Widgets.DrawTextureFitted(iconRect, icon, height / 216);

            Listing_Standard textListing = new Listing_Standard();
            textListing.Begin(textRect);

            Text.Font = GameFont.Medium;
            textListing.Label(label.Translate());
            Text.Font = GameFont.Small;
            textListing.Label(desc.Translate());
            Rect selectButtonRect = textListing.GetRect(30f).LeftHalf();
            Listing_Standard selectButtonListing = new Listing_Standard();
            selectButtonListing.Begin(selectButtonRect);
            if (selectButtonListing.ButtonText("BMT_Select".Translate()))
            {
                action();
            }
            selectButtonListing.End();
            textListing.End();
        }

    }





}