using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace BiomesPrehistoric
{
    public class SitePartWorker_NestingSite : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(
            SitePart part,
            Slate slate,
            List<Rule> outExtraDescriptionRules,
            Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);

            var tile = slate.Get<int>("siteTile");
            var points = slate.Get<int>("points");

            part.things = new ThingOwner<Thing>(part, false);

            var biome = tile < 0 ? BiomeDefOf.TemperateForest : Find.World?.grid[tile]?.biome ?? BiomeDefOf.TemperateForest;

            var dinoDefs = biome.AllWildAnimals
                .Where(animalDef => animalDef != null && animalDef.modContentPack == BiomesPrehistoricMod.ModContentPack)
                .Where(animalDef => animalDef.race.comps.Any(comp => comp is CompProperties_EggLayer))
                .ToList();

            if (dinoDefs.Count == 0)
            {
                Log.Warning("Failed to get dino type for biome, using any type instead.");
                dinoDefs = DefDatabase<PawnKindDef>.AllDefs
                    .Where(animalDef => animalDef != null && animalDef.modContentPack == BiomesPrehistoricMod.ModContentPack)
                    .Where(animalDef => animalDef.race.comps.Any(comp => comp is CompProperties_EggLayer))
                    .ToList();
            }
            
            var dinoDef = dinoDefs.RandomElement();
            var isLargeDino = BiomesPrehistoric.IsLargeDino(dinoDef.race);
            var nestCount = Math.Min(10, (int)(points / 1000f) + Rand.Range(1, 3));
            var adultCount = Math.Min(20, (int)(points / 1000f) * 2 + Rand.Range(2, 5));
            var nestDef = isLargeDino ? BiomesPrehistoric_DefOf.BMT_LargeNest : BiomesPrehistoric_DefOf.BMT_SmallNest;

            var foundComp = dinoDef.race.comps.FirstOrDefault(comp => comp is CompProperties_EggLayer);
            if (foundComp is CompProperties_EggLayer eggLayer)
            {
                for (int i = 0; i < nestCount; i++)
                {
                    var nest = ThingMaker.MakeThing(nestDef);
                    if (nest == null) continue;

                    var nestAlertNearby = nest.TryGetComp<CompNestAlertNearby>();
                    nestAlertNearby?.Enable();

                    var eggContainer = nest.TryGetComp<CompEggContainer>();
                    if (eggContainer == null) continue;
                    
                    var eggs = ThingMaker.MakeThing(eggLayer.eggFertilizedDef);
                    eggs.stackCount = eggLayer.eggCountRange.RandomInRange;
                    eggContainer.innerContainer.TryAdd(eggs);
                    
                    part.things.TryAdd(nest, false);
                }
                
                for (int i = 0; i < adultCount; i++)
                {
                    var request = new PawnGenerationRequest(dinoDef);
                    request.FixedGender = i < nestCount ? Gender.Female : Gender.Male;
                    var dino = PawnGenerator.GeneratePawn(request);
                    part.things.TryAdd(dino, false);
                }
            }
        }

        public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
        {
            string processedThreatLabel = base.GetPostProcessedThreatLabel(site, sitePart);
            
            if (site.HasWorldObjectTimeout) 
                processedThreatLabel += 
                    " (" + "DurationLeft".Translate((NamedArgument)site.WorldObjectTimeoutTicksLeft.ToStringTicksToPeriod()) + ")";
            
            return processedThreatLabel;
        }
    }
}