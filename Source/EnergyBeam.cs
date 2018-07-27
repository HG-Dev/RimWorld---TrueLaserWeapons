using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Suffixware.TrueLaserWeapons
{
    /// <summary>
    /// An displayed energy beam prompted by a Verb_ShootBeam object.
    /// </summary>
    class EnergyBeam : ThingWithComps
    {
        public int durationInTicks;
        public int fadeInTicks;
        private int startTick;
        public Verb_ShootBeam verb;
        public IntVec3 targetSquare;
        public float width = 2;
        public CompBeam beam = null;


        //private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        protected int TicksPassed
        {
            get { return Find.TickManager.TicksGame - this.startTick; }
        }

        protected int TicksLeft
        {
            get { return this.durationInTicks - this.TicksPassed; }
        }

        protected Vector3 Start
        {
            get { return verb.CasterPawn.DrawPos; }
        }

        protected Vector3 Target
        {
            get { return targetSquare.ToVector3ShiftedWithAltitude(AltitudeLayer.Projectile); }
        }

        protected float FullLength
        {
            get { return verb.CanPierceAllInRange ? this.verb.verbProps.range : (Start - Target).MagnitudeHorizontal(); }
        }



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.durationInTicks, "duration", 0);
            Scribe_Values.Look<int>(ref this.startTick, "startTick", 0);
            Scribe_Values.Look<Verb_ShootBeam>(ref this.verb, "verb", new Verb_ShootBeam()); //TODO: Test loading while a beam exists
        }

        public override void Draw()
        {
            base.Comps_PostDraw();
        }

        public override void Tick()
        {
            base.Tick();
            if (this.TicksPassed >= this.durationInTicks)
            {
                verb.TryCastNextPulseContact();
                this.Destroy(DestroyMode.Vanish);
            }
        }

        /// <summary>
        /// Create temporary motes(?)
        /// Damage? Or should damage be applied as an invisible bullet?
        /// Projectile's impact method is protected; reproduce its functionality here
        /// </summary>
        public void Emit()
        {
            Log.Message("Energy beam created");
            //Start timer
            startTick = Find.TickManager.TicksGame;

            //Expand shoot line if beam uses its entire effective range
            if (verb.CanPierceAllInRange)
            {
                //shootLine.
            }
            this.beam = GetComp<CompBeam>();
            beam.start = Start;
            beam.target = Target;
            beam.Rendering = true;
            base.GetComp<CompAffectsSky>().StartFadeInHoldFadeOut(2, durationInTicks - 4, 2, 1f);

            //MoteMaker.MakePowerBeamMote(stop, caster.Map);
            //Find offset for tip of weapon

        }
    }
}
