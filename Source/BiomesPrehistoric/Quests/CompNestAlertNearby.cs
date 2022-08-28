using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace BiomesPrehistoric
{
    public class CompNestAlertNearby : ThingComp
    {
        private bool _triggered;
        private bool _enabled;

        public CompProperties_NestAlertNearby Props => (CompProperties_NestAlertNearby) props;

        public void Enable() => _enabled = true;

        public override void CompTick()
        {
            base.CompTick();
            
            if (!_enabled || _triggered || !parent.Spawned)
                return;
            
            if (Find.TickManager.TicksGame % 250 == 0)
                CompTickRare();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();

            if (!_enabled || _triggered || !parent.Spawned)
                return;

            var validator = (Predicate<Thing>) null;
            if (Props.onlyHumanlike)
                validator = t => t is Pawn pawn && pawn.RaceProps.Humanlike;
            
            var thing = (Thing) null;
            
            if (Props.triggerRadius > 0.0)
                thing = GenClosest.ClosestThingReachable(parent.Position, parent.Map, 
                    ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, 
                    TraverseParms.For(TraverseMode.NoPassClosedDoors), Props.triggerRadius, validator);

            if (thing == null)
                return;
            
            var eggContainer = parent.TryGetComp<CompEggContainer>();
            if (eggContainer == null || !eggContainer.innerContainer.Any) return;

            var eggStack = eggContainer.innerContainer[0];
            
            var hatcher = eggStack.TryGetComp<CompHatcher>();
            if (hatcher == null) return;

            var dinoKind = hatcher.Props.hatcherPawn;
            var dinos = parent.Map.mapPawns.AllPawnsSpawned
                .Where(p => p.kindDef == dinoKind && p.Position.InHorDistOf(parent.Position, Props.alertRadius));

            bool foundOne = false;
            foreach (var dino in dinos)
            {
                if (dino.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent))
                {
                    LifeStageUtility.PlayNearestLifestageSound(dino, ls => ls.soundAngry);
                    foundOne = true;
                }
            }

            if (foundOne)
            {
                Messages.Message("BMT_NestAlert".Translate((NamedArgument) thing), (Thing) parent, MessageTypeDefOf.ThreatBig);
                Find.CameraDriver.shaker.DoShake(2f);
            }

            _triggered = true;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _triggered, "triggered");
            Scribe_Values.Look(ref _enabled, "enabled");
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CompProperties_NestAlertNearby : CompProperties
    {
        public bool onlyHumanlike;
        public float triggerRadius;
        public float alertRadius;

        public CompProperties_NestAlertNearby() => compClass = typeof(CompNestAlertNearby);
    }
}