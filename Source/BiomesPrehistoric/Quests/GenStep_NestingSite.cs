using System.Linq;
using RimWorld;
using Verse;

namespace BiomesPrehistoric
{
    public class GenStep_NestingSite : GenStep_Scatterer
    {
        public override int SeedPart => 931844070;
        
        private const int SpawnRadius = 5;

        protected override bool CanScatterAt(IntVec3 c, Map map) => base.CanScatterAt(c, map) && CanSpawnAt(c, map);

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int scatterCount = 1)
        {
            if (parms.sitePart?.things == null || !parms.sitePart.things.Any) return;

            var toBeSpawned = parms.sitePart.things.ToList();
            foreach (var spawnThing in toBeSpawned.Select(thing => parms.sitePart.things.Take(thing)))
            {
                if (spawnThing != null && CellFinder.TryRandomClosewalkCellNear(loc, map, SpawnRadius, out var result, x => CanSpawnAt(x, map)))
                {
                    GenSpawn.Spawn(spawnThing, result, map);
                }
            }
            
            MapGenerator.rootsToUnfog.Add(loc);
            MapGenerator.SetVar("RectOfInterest", CellRect.CenteredOn(loc, SpawnRadius * 2, SpawnRadius * 2));
        }
        
        private bool CanSpawnAt(IntVec3 c, Map map)
        {
            if (!c.Standable(map))
                return false;
            
            if (map.fertilityGrid.FertilityAt(c) <= 0f)
                return false;
            
            if (c.GetEdifice(map) != null)
                return false;

            return true;
        }
    }
}