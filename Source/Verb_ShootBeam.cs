using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace Suffixware.TrueLaserWeapons
{
    class Verb_ShootBeam : Verb_LaunchProjectile
    {
        /// <summary>
        /// Override standard projectile to a super-fast dummy projectile.
        /// </summary>
        private int pulseTicksLeft = 0;

        //Use base.Projectile as a way to define the effect of the beam

        private VerbProperties_ShootBeam beamProps => verbProps as VerbProperties_ShootBeam;
        public int PulseTicks
        {
            get { return beamProps.pulseTime.SecondsToTicks(); }
        }

        public int FlashTicks
        {
            get { return beamProps.flashTime.SecondsToTicks(); }
        }

        public bool CanPierceAllInRange
        {
            get { return beamProps.canPierceAllInRange; }
        }

        public int TicksBetweenBeamUpdate
        {
            get
            {
                return this.verbProps.ticksBetweenBurstShots;
            }
        }

        public override void WarmupComplete()
        {
            this.pulseTicksLeft = this.PulseTicks;
            this.state = VerbState.Bursting;
            //Start with "cooldown" as weapon warms up
            if (this.CasterIsPawn)
            {
                this.CasterPawn.stances.SetStance(
                    new Stance_Cooldown(FlashTicks, this.currentTarget, this)
                    );
            }
            TryCastNextPulseContact();
            StdObservationalUpkeep();
        }

        protected override bool TryCastShot()
        {
            if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
            {
                return false;
            }
            if (Projectile == null)
            {
                return false;
            }
            ShootLine shootLine;
            bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
            if (ownerEquipment != null)
            {
                //Debug this to find if this is a turret or gun or both
                CompChangeableProjectile comp = this.ownerEquipment.GetComp<CompChangeableProjectile>();
                if (comp != null)
                {
                    comp.Notify_ProjectileLaunched();
                }
            }
            //This part reconfigures the participants if a turret is being used
            Thing launcher = this.caster;
            Thing equipment = this.ownerEquipment;
            CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = this.caster;
            }
            //Determine the chance the weapon user screws up
            ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
            if (Rand.Value > shotReport.ChanceToNotGoWild_IgnoringPosture)
            {
                if (DebugViewSettings.drawShooting)
                {
                    MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToWild", -1f);
                }
                //Modify the shoot line to aim for a cell adjacent to the target
                shootLine.ChangeDestToMissWild();
            }
            if (Rand.Value > shotReport.ChanceToNotHitCover)
            {
                if (DebugViewSettings.drawShooting)
                {
                    MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToCover", -1f);
                }
                if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn)
                {
                    Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
                    return true;
                }
            }

            EnergyBeam beam = (EnergyBeam)ThingMaker.MakeThing(ThingDefOfTLW.EnergyBeam);
            beam.verb = this;
            beam.targetSquare = shootLine.Dest;
            GenSpawn.Spawn(beam, shootLine.Source, caster.Map);
            return true;
        }

        public void TryCastNextPulseContact()
        {
            LocalTargetInfo localTargetInfo = this.currentTarget;
            //On successful firing of the beam
            if (this.TryCastShot())
            {
                if (this.verbProps.muzzleFlashScale > 0.01f)
                {
                    MoteMaker.MakeStaticMote(this.caster.Position, this.caster.Map, ThingDefOf.Mote_ShotFlash, this.verbProps.muzzleFlashScale);
                }
                //Normally, a sound would be given for each "bullet," but this is a sustained beam
                if (this.pulseTicksLeft >= this.PulseTicks)
                {
                    if (this.verbProps.soundCast != null)
                    {
                        this.verbProps.soundCast.PlayOneShot(new TargetInfo(this.caster.Position, this.caster.Map, false));
                    }
                    if (this.verbProps.soundCastTail != null)
                    {
                        this.verbProps.soundCastTail.PlayOneShotOnCamera(this.caster.Map);
                    }
                }
                if (this.CasterIsPawn)
                {
                    if (this.CasterPawn.thinker != null)
                    {
                        this.CasterPawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
                    }
                    if (this.CasterPawn.mindState != null)
                    {
                        this.CasterPawn.mindState.lastAttackTargetTick = Find.TickManager.TicksGame;
                        this.CasterPawn.mindState.lastAttackedTarget = localTargetInfo;
                    }
                    if (this.CasterPawn.MentalState != null)
                    {
                        this.CasterPawn.MentalState.Notify_AttackedTarget(localTargetInfo);
                    }
                    if (!this.CasterPawn.Spawned)
                    {
                        return;
                    }
                }
                else
                {
                    this.pulseTicksLeft = 0;
                }

                if (this.pulseTicksLeft > 0)
                {
                    //Prepare next beam contact effect
                    this.ticksToNextBurstShot = this.verbProps.ticksBetweenBurstShots;
                    //Because Verb.VerbTick is not overrideable, we have to rely on ticksToNextBurst shot to measure time.
                    this.pulseTicksLeft -= this.ticksToNextBurstShot;
                    if (this.CasterIsPawn)
                    {
                        this.CasterPawn.stances.SetStance(new Stance_Cooldown(this.verbProps.ticksBetweenBurstShots + 1, this.currentTarget, this));
                    }
                }
                else
                {
                    this.state = VerbState.Idle;
                    if (this.CasterIsPawn)
                    {
                        this.CasterPawn.stances.SetStance(new Stance_Cooldown(this.verbProps.AdjustedCooldownTicks(this, this.CasterPawn, this.ownerEquipment), this.currentTarget, this));
                    }
                    if (this.castCompleteCallback != null)
                    {
                        this.castCompleteCallback();
                    }
                }
            }
        }

        private void StdObservationalUpkeep()
        {
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(
                this.caster,
                (!this.currentTarget.HasThing) ? null : this.currentTarget.Thing,
                (this.ownerEquipment == null) ? null : this.ownerEquipment.def,
                this.Projectile,
                this.PulseTicks > 6)
            );
            if (this.CasterIsPawn && this.currentTarget.HasThing)
            {
                Pawn pawn = this.currentTarget.Thing as Pawn;
                if (pawn != null && pawn.IsColonistPlayerControlled)
                {
                    this.CasterPawn.records.AccumulateStoryEvent(StoryEventDefOf.AttackedPlayer);
                }
            }
            if (base.CasterIsPawn && base.CasterPawn.skills != null)
            {
                float xp = 6f;
                Pawn pawn = this.currentTarget.Thing as Pawn;
                if (pawn != null && pawn.HostileTo(this.caster) && !pawn.Downed)
                {
                    xp = 240f;
                }
                base.CasterPawn.skills.Learn(SkillDefOf.Shooting, xp, false);
            }
        }

        /*protected override bool TryCastShot()
        {
            bool flag = base.TryCastShot();
            if (flag && base.CasterIsPawn)
            {
                base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }
            return flag;
        }*/
    }


}

