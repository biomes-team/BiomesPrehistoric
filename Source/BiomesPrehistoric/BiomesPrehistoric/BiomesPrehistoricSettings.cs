using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BiomesPrehistoric
{
    public class BiomesPrehistoricSettings : ModSettings
    {
        private static Vector2 scrollPosition = Vector2.zero;
        public Dictionary<string, bool> pawnSpawnStates = new Dictionary<string, bool>();
        public bool flagVanillaAnimals = true;
        private List<string> pawnKeys;
        private List<bool> boolValues;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, bool>(ref this.pawnSpawnStates, "pawnSpawnStates", LookMode.Value, LookMode.Value, ref this.pawnKeys, ref this.boolValues);
            Scribe_Values.Look<bool>(ref this.flagVanillaAnimals, "flagVanillaAnimals", true, true);
        }

        public void DoWindowContents(Rect inRect)
        {
            List<string> list = this.pawnSpawnStates.Keys.ToList<string>().OrderByDescending<string, string>((Func<string, string>)(x => x)).ToList<string>();
            Listing_Standard listingStandard = new Listing_Standard();
            Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect = new Rect(0.0f, 0.0f, inRect.width - 30f, (float)((list.Count / 2 + 2) * 24));
            ref Vector2 local = ref BiomesPrehistoricSettings.scrollPosition;
            Rect viewRect = rect;
            Widgets.BeginScrollView(outRect, ref local, viewRect);
            listingStandard.ColumnWidth = rect.width / 2.2f;
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled((string)"allowVanillaAnimals".Translate(), ref this.flagVanillaAnimals);
            for (int index = list.Count - 1; index >= 0; --index)
            {
                if (index == list.Count / 2)
                    listingStandard.NewColumn();
                bool pawnSpawnState = this.pawnSpawnStates[list[index]];
                if (DefDatabase<PawnKindDef>.GetNamedSilentFail(list[index]) == null)
                {
                    this.pawnSpawnStates.Remove(list[index]);
                }
                else
                {
                    listingStandard.CheckboxLabeled((string)"BMT_DisableAnimal".Translate((NamedArgument)PawnKindDef.Named(list[index]).LabelCap), ref pawnSpawnState);
                    this.pawnSpawnStates[list[index]] = pawnSpawnState;
                }
            }
            listingStandard.End();
            Widgets.EndScrollView();
            this.Write();
        }
    }
}
