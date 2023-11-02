using System;
using HarmonyLib;
using Verse;

namespace BiomesPrehistoric
{
	[StaticConstructorOnStartup]
	static class BiomesPrehistoric
	{
		public const string Name = "Biomes! Prehistoric";
		private static readonly Version Version = typeof(BiomesPrehistoric).Assembly.GetName().Version;
		static BiomesPrehistoric()
		{
			Harmony harmony = new Harmony("rimworld.biomesprehistoric");

			harmony.PatchAll();
			Log("Initialized");

			//    BiomesPrehistoricMod.UpdateMainMenuSongDef();
		}

		public static void Log(string message) => Verse.Log.Message(PrefixMessage(message));
		public static void Error(string message) => Verse.Log.Error(PrefixMessage(message));

		private static string PrefixMessage(string message) => $"[{Name} v{Version}] {message}";

		public static bool IsLargeDino(ThingDef def)
		{
			return def.race?.baseBodySize >= 3f;
		}
	}
}