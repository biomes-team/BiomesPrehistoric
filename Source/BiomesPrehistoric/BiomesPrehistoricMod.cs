using System;
using System.Collections.Generic;
using BiomesPrehistoric.SettingsUI;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric
{
	public class BiomesPrehistoricMod : Mod
	{
		private const int MinCommonality = 5;
		private const int MaxCommonality = 500;

		/// <summary>
		/// Space taken by the title of the mod in the mod settings window.
		/// </summary>
		private const float TitleHeight = GenUI.ListSpacing;

		/// <summary>
		/// Height of the tabs part of the settings window.
		/// </summary>
		private const float TabsHeight = GenUI.ListSpacing;

		/// <summary>
		/// The names of these labels must match the format used in the BMT_{}Tab translatable strings.
		/// </summary>
		public enum SettingsWindowTab
		{
			General,
			PrehistoricAnimals,
		}

		/// <summary>
		/// Current tab shown in the UI.
		/// </summary>
		private static SettingsWindowTab _tab = SettingsWindowTab.General;

		private static readonly PrehistoricAnimalsTabUI PrehistoricAnimalsTabUI = new PrehistoricAnimalsTabUI();

		private static Vector2 _scrollPos = Vector2.zero;

		public static ModContentPack ModContentPack;

		public BiomesPrehistoricMod(ModContentPack content) : base(content)
		{
			GetSettings<PrehistoricSettings>();
			ModContentPack = content;
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

		private List<TabRecord> Tabs()
		{
			Type enumType = typeof(SettingsWindowTab);
			List<TabRecord> tabs = new List<TabRecord>();
			foreach (SettingsWindowTab tab in Enum.GetValues(enumType))
			{
				string tabStr = Enum.GetName(enumType, tab);
				tabs.Add(new TabRecord($"BMT_{tabStr}Tab".Translate(), () =>
				{
					_tab = tab;
					WriteSettings();
				}, _tab == tab));
			}

			return tabs;
		}

		private void DoTabContents(Rect inRect)
		{
			switch (_tab)
			{
				case SettingsWindowTab.General:
					GeneralTabUI(inRect);
					break;
				case SettingsWindowTab.PrehistoricAnimals:
					PrehistoricAnimalsTabUI.Contents(inRect);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Rect settingsArea = inRect.BottomPartPixels(inRect.height - TitleHeight);
			Rect tabArea = settingsArea.TopPartPixels(TabsHeight);

			Widgets.DrawMenuSection(settingsArea);
			TabDrawer.DrawTabs(tabArea, Tabs());

			DoTabContents(settingsArea.ContractedBy(15.0f));
			
			DrawBottomButtons(inRect);
			base.DoSettingsWindowContents(inRect);
		}

		public void GeneralTabUI(Rect inRect)
		{
			var rect = new Rect(0.0f, 0.0f, inRect.width, 800f);
			rect.xMax *= 0.95f;

			var mainListing = new Listing_Standard();
			mainListing.Begin(rect);
			GUI.EndGroup();
			Widgets.BeginScrollView(inRect, ref _scrollPos, rect);

			mainListing.Label("BMT_PrehistoricSettingsDesc".Translate());

			//Dino and vanilla
			SpawnPicker(mainListing, PrehistoricSettings.Values.spawnOption == SpawnOption.DinoAndVanilla,
				delegate { PrehistoricSettings.Values.spawnOption = SpawnOption.DinoAndVanilla; }, "BMT_DinoAndVanillaLabel",
				"BMT_DinoAndVanillaDesc", "DinoAndVanilla");

			// Dino and plants
			SpawnPicker(mainListing, PrehistoricSettings.Values.spawnOption == SpawnOption.DinoAndPlant,
				delegate { PrehistoricSettings.Values.spawnOption = SpawnOption.DinoAndPlant; }, "BMT_DinoAndPlantLabel",
				"BMT_DinoAndPlantDesc", "DinoAndPlant");

			// DINO WORLD!!!
			SpawnPicker(mainListing, PrehistoricSettings.Values.spawnOption == SpawnOption.DinoWorld,
				delegate { PrehistoricSettings.Values.spawnOption = SpawnOption.DinoWorld; }, "BMT_DinoWorldLabel",
				"BMT_DinoWorldDesc",
				"DinoWorld");

			if (PrehistoricSettings.Values.spawnOption != SpawnOption.DinoWorld)
			{
				mainListing.Label("BMT_PrehistoricAnimalCommonality".Translate(PrehistoricSettings.Values.animalCommonality),
					-1,
					"BMT_PrehistoricAnimalCommonalityTooltip".Translate());
				mainListing.verticalSpacing = 1f;
				PrehistoricSettings.Values.animalCommonality =
					(int)mainListing.Slider(PrehistoricSettings.Values.animalCommonality, MinCommonality, MaxCommonality);
				if (PrehistoricSettings.Values.spawnOption == SpawnOption.DinoAndPlant)
				{
					mainListing.Label("BMT_PrehistoricPlantCommonality".Translate(PrehistoricSettings.Values.plantCommonality),
						-1,
						"BMT_PrehistoricPlantCommonalityTooltip".Translate());
					PrehistoricSettings.Values.plantCommonality =
						(int)mainListing.Slider(PrehistoricSettings.Values.plantCommonality, MinCommonality, MaxCommonality);
				}
			}

			//mainListing.Gap();
			//mainListing.CheckboxLabeled("BMT_MainMenuMusicEnabled".Translate(), ref PrehistoricSettings.Values.mainMenuMusic);

			Widgets.EndScrollView();


		}

		/*
		public static void UpdateMainMenuSongDef()
		{
		    var def = DefDatabase<SongDef>.GetNamedSilentFail(PrehistoricSettings.Values.mainMenuMusic ? "EntrySongPrehistoric" : "EntrySong");
		    if (def != null) SongDefOf.EntrySong = def;
		}
		*/

		private void DrawBottomButtons(Rect inRect)
		{
			float resetX = inRect.width - Window.CloseButSize.x;
			// Dialog_ModSettings leaves a margin of Window.CloseButSize.y at the bottom for the close button.
			// Then, there are three pixels between the top border of the close button and the rest of this window.
			float resetY = inRect.height + Window.CloseButSize.y + 3;
			Rect resetButtonArea = new Rect(resetX, resetY, Window.CloseButSize.x, Window.CloseButSize.y);

			if (Widgets.ButtonText(resetButtonArea, "BMT_ResetSettingsLabel".Translate()))
			{
				PrehistoricSettings.Reset();
			}

			TooltipHandler.TipRegion(resetButtonArea, "BMT_ResetSettingsHover".Translate());
		}

		public static void SpawnPicker(Listing_Standard listing, bool active, Action action, string label, string desc,
			string iconPath)
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

			string texPath = $"BMT_Prehistoric/UI/{iconPath}";
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