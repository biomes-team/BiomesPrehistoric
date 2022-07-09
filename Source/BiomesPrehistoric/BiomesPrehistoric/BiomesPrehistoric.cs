using UnityEngine;
using Verse;

namespace BiomesPrehistoric
{
    [StaticConstructorOnStartup]
    public static class BiomesPrehistoric
    {
        public static bool Settings_Button(this Listing_Standard ls, string label, Rect rect)
        {
            int num = Widgets.ButtonText(rect, label) ? 1 : 0;
            ls.Gap(2f);
            return num != 0;
        }
    }
}
