using HarmonyLib;
using System.Reflection;
using Verse;

namespace BiomesPrehistoric
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main() => new Harmony("com.biomesprehistoric").PatchAll(Assembly.GetExecutingAssembly());
    }
}
