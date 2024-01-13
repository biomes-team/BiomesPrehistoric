using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric.SettingsUI
{
	using AnimalEntry = Tuple<PawnKindDef, Color, Texture>;

	public class PrehistoricAnimalsTabUI
	{
		private const float AnimalEntryHeight = 48F;

		/// <summary>
		/// Current scroll position..
		/// </summary>
		private static Vector2 _scrollPosition;

		/// <summary>
		/// List of all entries.
		/// </summary>
		private List<AnimalEntry> _entries;

		/// <summary>
		/// Can filter animals by label, defName, or packageId.
		/// </summary>
		private string _filterText;

		/// <summary>
		/// Checks if a pawnKindDef should appear in the tab.
		/// </summary>
		/// <param name="pawnKindDef">Pawn kind to check.</param>
		/// <param name="ignoreDuplicates">Used to ignore pawnKindDefs sharing the same pawnKindDef.race.</param>
		/// <returns>True if this should be a valid entry.</returns>
		private static bool IsValidPawnKindDef(PawnKindDef pawnKindDef, HashSet<ThingDef> ignoreDuplicates)
		{
			return
				// Ignore invalid entries.
				pawnKindDef.race?.race != null &&
				// Only animals.
				pawnKindDef.race.race.Animal && !pawnKindDef.race.race.Dryad &&
				// Ignore duplicates.
				!ignoreDuplicates.Contains(pawnKindDef.race);
		}

		/// <summary>
		/// Generate entry data from a pawnKindDef.
		/// </summary>
		/// <param name="pawnKindDef">Pawn kind being shown.</param>
		/// <returns>Data required to show this entry on the settings tab.</returns>
		private static AnimalEntry GetAnimalEntry(PawnKindDef pawnKindDef)
		{
			Color color = Color.white;
			Texture texture = null;
			if (pawnKindDef.lifeStages != null)
			{
				int lastLifeStageIndex = pawnKindDef.lifeStages.Count - 1;
				if (lastLifeStageIndex >= 0)
				{
					PawnKindLifeStage lastLifeStage = pawnKindDef.lifeStages[lastLifeStageIndex];
					// See ThingDef.ResolveIcon()
					Material material = lastLifeStage.bodyGraphicData?.Graphic?.MatEast;
					if (material != null)
					{
						color = material.color;
						texture = material.mainTexture;
					}
				}
			}

			return new AnimalEntry(pawnKindDef, color, texture);
		}

		/// <summary>
		/// Lazy initialization of table entries.
		/// </summary>
		private void InitializeEntries()
		{
			if (_entries != null)
			{
				return;
			}

			_entries = new List<AnimalEntry>();
			HashSet<ThingDef> ignoreDuplicates = new HashSet<ThingDef>();

			List<PawnKindDef> pawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading;
			for (int pawnIndex = 0; pawnIndex < pawnKindDefs.Count; ++pawnIndex)
			{
				PawnKindDef pawnKindDef = pawnKindDefs[pawnIndex];
				try
				{
					if (!IsValidPawnKindDef(pawnKindDef, ignoreDuplicates))
					{
						continue;
					}

					ignoreDuplicates.Add(pawnKindDef.race);
					_entries.Add(GetAnimalEntry(pawnKindDef));
				}
				catch (Exception exception)
				{
					Log.Error($"Prehistoric animal tab settings failed to initialize PawnKindDef {pawnKindDef}:");
					Log.Error($"{exception}");
				}
			}

			_entries.Sort(
				(lhs, rhs) => string.Compare(lhs.Item1.race.LabelCap.ToString(), rhs.Item1.race.LabelCap.ToString(),
					StringComparison.Ordinal));
		}

		/// <summary>
		/// Draw the row for one of the animals.
		/// </summary>
		/// <param name="pawnKindDef">Pawn to draw.</param>
		/// <param name="color">Recolor to use for the texture of this pawn.</param>
		/// <param name="texture">Texture to draw.</param>
		/// <param name="rowRect">Available area.</param>
		private static void DrawAnimalRow(PawnKindDef pawnKindDef, Color color, Texture texture, Rect rowRect)
		{
			// First cell: textures use a square space.
			Rect textureRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height);

			// Last cell: checkbox.
			Rect checkboxRect = new Rect(rowRect.x + rowRect.width - Window.CloseButSize.x, rowRect.y, Window.CloseButSize.x,
				rowRect.height);

			// The remaining width is shared between the last two cells.
			float remainingIntervalStart = textureRect.x + textureRect.width;
			float remainingWidth = checkboxRect.x - remainingIntervalStart;

			// Second cell: label
			Rect labelRect = new Rect(remainingIntervalStart, rowRect.y, 2.0F * remainingWidth / 5.0F, rowRect.height);
			// Third cell: mod
			Rect modRect = new Rect(labelRect.x + labelRect.width, rowRect.y, 3.0F * remainingWidth / 5.0F, rowRect.height);

			if (texture != null)
			{
				GUI.color = color;
				GUI.DrawTexture(textureRect, texture);
				GUI.color = Color.white;
			}

			Widgets.Label(labelRect, pawnKindDef.race.LabelCap);


			Widgets.Label(modRect, ModOf(pawnKindDef));
			bool placeholder = Util.IsPrehistoric(pawnKindDef);
			Vector2 position = new Vector2(checkboxRect.center.x, checkboxRect.center.y - Widgets.CheckboxSize / 2.0F);
			Widgets.Checkbox(position, ref placeholder);

			if (Mouse.IsOver(rowRect))
			{
				GUI.DrawTexture(rowRect, TexUI.HighlightSelectedTex);
			}
		}

		private static string ModOf(PawnKindDef pawnKindDef)
		{
			return pawnKindDef.modContentPack != null
				? pawnKindDef.modContentPack.Name
				: "Unknown".Translate().ToString();
		}

		/// <summary>
		/// Helper function for a case-insensitive comparison of an arbitrary string against the current filter.
		/// </summary>
		/// <param name="str">String to check.</param>
		/// <returns>True if the string is a match.</returns>
		private bool FilterMatch(string str)
		{
			return str.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private List<AnimalEntry> FilterEntries()
		{
			if (_filterText == "")
			{
				return _entries;
			}

			List<AnimalEntry> filteredEntries = new List<AnimalEntry>();
			foreach (AnimalEntry entry in _entries)
			{
				PawnKindDef pawnKindDef = entry.Item1;
				if (FilterMatch(pawnKindDef.race.label) || FilterMatch(pawnKindDef.defName) || FilterMatch(ModOf(pawnKindDef)))
				{
					filteredEntries.Add(entry);
				}
			}

			return filteredEntries;
		}

		/// <summary>
		/// Draw the contents of the tab.
		/// </summary>
		/// <param name="inRect">Available draw area.</param>
		public void Contents(Rect inRect)
		{
			const float animalEntryGap = GenUI.GapTiny;
			TextAnchor anchorBackup = Text.Anchor;

			InitializeEntries();
			Rect topRect =  inRect.TopPartPixels(GenUI.GapWide * 2 + GenUI.GapSmall);
			Rect labelRect = topRect.TopPartPixels(GenUI.GapWide);
			Widgets.Label(labelRect, "BMT_PrehistoricAnimalsTabDescription".Translate());
			Rect filterRect = topRect.BottomPartPixels(GenUI.GapWide);
			_filterText = Widgets.TextField(filterRect, _filterText).ToLower();

			inRect = inRect.BottomPartPixels(inRect.height - topRect.height - GenUI.GapSmall);
			Rect outRect = inRect.ContractedBy(GenUI.GapSmall);

			List<AnimalEntry> filteredEntries = FilterEntries();
			int entryCount = filteredEntries.Count;

			Rect viewRect = new Rect(0, 0, outRect.width - GenUI.ScrollBarWidth,
				(AnimalEntryHeight + animalEntryGap) * entryCount);
			Widgets.BeginScrollView(inRect, ref _scrollPosition, viewRect);

			Listing_Standard listing = new Listing_Standard();
			listing.Begin(viewRect);

			Text.Anchor = TextAnchor.MiddleCenter;

			float minHeightThreshold = _scrollPosition.y - AnimalEntryHeight;
			float maxHeightThreshold = _scrollPosition.y + viewRect.height;
			foreach (var (pawnKindDef, color, texture) in filteredEntries)
			{
				Rect entryRect = listing.GetRect(AnimalEntryHeight);
				float currentHeight = entryRect.y;
				// Entries are only processed if they are inside of the viewable area.
				if (currentHeight > minHeightThreshold && currentHeight < maxHeightThreshold)
				{
					try
					{
						DrawAnimalRow(pawnKindDef, color, texture, entryRect);
					}
					catch (Exception exception)
					{
						string str = $"Pawn movement settings failed to draw row for PawnKindDef {pawnKindDef}: {exception}";
						Log.ErrorOnce(str, str.GetHashCode());
					}
				}

				listing.Gap(animalEntryGap);
			}

			Text.Anchor = anchorBackup;

			listing.End();
			Widgets.EndScrollView();
		}
	}
}