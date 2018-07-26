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
    [StaticConstructorOnStartup]
    class EnergyBeam : ThingWithComps
    {
        public int durationInTicks;
        public int fadeInTicks;
        private int startTick;
        public Verb_ShootBeam verb;
        public IntVec3 targetSquare;

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow);
        //private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        protected int TicksPassed
        {
            get { return Find.TickManager.TicksGame - this.startTick; }
        }

        protected int TicksLeft
        {
            get { return this.durationInTicks - this.TicksPassed; }
        }

        protected float Length
        {
            get { return this.verb.verbProps.range; }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.durationInTicks, "duration", 0);
            Scribe_Values.Look<int>(ref this.startTick, "startTick", 0);
            Scribe_Values.Look<Verb_ShootBeam>(ref this.verb, "verb", new Verb_ShootBeam()); //TODO: Test loading while a beam exists
        }

        /// <summary>
        /// Renders a texture depicting a beam connecting two points.
        /// </summary>
        public virtual void DrawArc(Vector3 start, Vector3 end, Material mat)
        {
            if ((Mathf.Abs(start.x - end.x) < 0.01f && Mathf.Abs(start.z - end.z) < 0.01f)
                || start == end)
            {
                //Don't bother drawing a very short arc
                return;
            }
            start.y = end.y;
            Vector3 midpoint = (start + end) / 2f;
            float mag = (start - end).MagnitudeHorizontal();
            Quaternion rotate = Quaternion.LookRotation(start - end);
            Vector3 scalar = new Vector3(mag * 0.666f, 1f, mag);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(midpoint, rotate, scalar);
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
        }

        public override void Draw()
        {
            //Assume, for now, that while this object exists it should be drawn
            DrawArc(verb.CasterPawn.DrawPos, targetSquare.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteLow), BeamMat);
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
            //Expand shoot line if beam uses its entire effective range
            if (verb.CanPierceAllInRange)
            {
                //shootLine.
            }

            //Begin displaying
            startTick = Find.TickManager.TicksGame;
            //MoteMaker.MakePowerBeamMote(stop, caster.Map);
            //Find offset for tip of weapon

        }
    }
}
