using System.Collections.Generic;
using Verse;

namespace BiomesPrehistoric.Defs
{
	public class PrehistoricModDef : Def
	{
		public List<string> packageIds;

		public static HashSet<string> AllPackageIds = new HashSet<string>();

		public override void ResolveReferences()
		{
			if (packageIds.NullOrEmpty())
			{
				return;
			}

			foreach (string packageId in packageIds)
			{
				AllPackageIds.Add(packageId.ToLower());
			}
		}
	}
}