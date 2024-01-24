using System.Collections.Generic;
using Verse;

namespace BiomesPrehistoric.Defs
{
	public class PrehistoricModDef : Def
	{
		public List<string> packageIds;
		public List<string> packageIds_AlwaysSpawn;

		public static HashSet<string> AllPackageIds = new HashSet<string>();
		public static HashSet<string> AlwaysSpawnPackageIds = new HashSet<string>();

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
			foreach (string id in packageIds_AlwaysSpawn)
			{
				AlwaysSpawnPackageIds.Add(id.ToLower());
			}
		}
	}
}