using System;
using RimWorld;
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

        // Prehistoric animal commonality. Only active with DinoAndVanilla or DinoAndPlant.
        public int animalCommonality = 100;

        // Prehistoric plant commonality. Only active with DinoAndPlant.
        public int plantCommonality = 100;
        
        // Custom music on main menu enabled
        public bool mainMenuMusic = true;

        // this is what saves the settings when the user closes the game
        public override void ExposeData()
        {
            Scribe_Values.Look(ref spawnOption, "spawnOption", SpawnOption.DinoAndPlant);
            Scribe_Values.Look(ref animalCommonality, "animalCommonality", 100, true);
            Scribe_Values.Look(ref plantCommonality, "plantCommonality", 100, true);
            Scribe_Values.Look(ref mainMenuMusic, "mainMenuMusic", true, true);
        }
    }

    // this class handles the interface
    public class BiomesPrehistoricMod : Mod
    {
        public const int minCommonality = 5;
        public const int maxCommonality = 500;

        public PrehistoricSettings settings;
        public static BiomesPrehistoricMod mod;

        public static ModContentPack ModContentPack;
        
        public BiomesPrehistoricMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<PrehistoricSettings>();
            ModContentPack = content;
            mod = this;
            // Patch factions with prehistoric pack animals after the game is fully loaded.
            LongEventHandler.ExecuteWhenFinished(PrehistoricPackAnimals.Patch);
        }

        public override string SettingsCategory()
        {
            return "BMT_BiomesPrehistoric".Translate();
        }
        
        public override void WriteSettings()
        {
            base.WriteSettings();
            // Patch factions with prehistoric pack animals after the settings are changed.
            PrehistoricPackAnimals.Patch();
        }

        private static Vector2 _scrollPos = Vector2.zero;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var rect = new Rect(0.0f, 0.0f, inRect.width, 800f);
            rect.xMax *= 0.95f;
        
            var mainListing = new Listing_Standard();
            mainListing.Begin(rect);
            GUI.EndGroup();
            Widgets.BeginScrollView(inRect, ref _scrollPos, rect);

            mainListing.Label("BMT_PrehistoricSettingsDesc".Translate());

            //Dino and vanilla
            SpawnPicker(mainListing, BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoAndVanilla, delegate { BiomesPrehistoricMod.mod.settings.spawnOption = SpawnOption.DinoAndVanilla; }, "BMT_DinoAndVanillaLabel", "BMT_DinoAndVanillaDesc", "DinoAndVanilla");

            // Dino and plants
            SpawnPicker(mainListing, BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoAndPlant, delegate { BiomesPrehistoricMod.mod.settings.spawnOption = SpawnOption.DinoAndPlant; }, "BMT_DinoAndPlantLabel", "BMT_DinoAndPlantDesc", "DinoAndPlant");
            
            // DINO WORLD!!!
            SpawnPicker(mainListing, BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoWorld, delegate { BiomesPrehistoricMod.mod.settings.spawnOption = SpawnOption.DinoWorld; }, "BMT_DinoWorldLabel", "BMT_DinoWorldDesc", "DinoWorld");

            if (BiomesPrehistoricMod.mod.settings.spawnOption != SpawnOption.DinoWorld)
            {
                mainListing.Label("BMT_PrehistoricAnimalCommonality".Translate(BiomesPrehistoricMod.mod.settings.animalCommonality), -1, "BMT_PrehistoricAnimalCommonalityTooltip".Translate());
                mainListing.verticalSpacing = 1f;
                BiomesPrehistoricMod.mod.settings.animalCommonality = (int) mainListing.Slider(BiomesPrehistoricMod.mod.settings.animalCommonality, minCommonality, maxCommonality);
                if (BiomesPrehistoricMod.mod.settings.spawnOption == SpawnOption.DinoAndPlant)
                {
                    mainListing.Label("BMT_PrehistoricPlantCommonality".Translate(BiomesPrehistoricMod.mod.settings.plantCommonality), -1, "BMT_PrehistoricPlantCommonalityTooltip".Translate());
                    BiomesPrehistoricMod.mod.settings.plantCommonality = (int) mainListing.Slider(BiomesPrehistoricMod.mod.settings.plantCommonality, minCommonality, maxCommonality);
                }
            }

            mainListing.Gap();
            mainListing.CheckboxLabeled("BMT_MainMenuMusicEnabled".Translate(), ref mod.settings.mainMenuMusic);

            Widgets.EndScrollView();
            base.DoSettingsWindowContents(inRect);

        }

        public static void UpdateMainMenuSongDef()
        {
            var def = DefDatabase<SongDef>.GetNamedSilentFail(mod.settings.mainMenuMusic ? "EntrySongPrehistoric" : "EntrySong");
            if (def != null) SongDefOf.EntrySong = def;
        }

        public static void SpawnPicker(Listing_Standard listing, bool active, Action action, string label, string desc, string iconPath)
        {
            const float height = 140f;
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
            textRect.yMin += 10f;
            iconRect.xMin = iconRect.xMax - height;

            string texPath = String.Format("BMT_Prehistoric/UI/{0}", iconPath);
            Texture2D icon = ContentFinder<Texture2D>.Get(texPath, true);
            Widgets.DrawTextureFitted(iconRect, icon, 0.88f);

            Listing_Standard textListing = new Listing_Standard();
            textListing.Begin(textRect);

            Text.Font = GameFont.Medium;
            textListing.Label(label.Translate());
            Text.Font = GameFont.Small;
            textListing.Label(desc.Translate());
            Rect selectButtonRect = textListing.GetRect(30f).LeftPartPixels(150f);
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