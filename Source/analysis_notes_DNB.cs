using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Suffixware
{
    class analysis_notes_DNB : Verb_LaunchProjectile
    {
        // Token: 0x060057CC RID: 22476 RVA: 0x0017B880 File Offset: 0x00179C80
        protected override bool TryCastShot()
        {
            //If we are aiming at something real on the map
            if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
            {
                return false;
            }
            ThingDef projectile = this.Projectile;
            if (projectile == null)
            {
                return false;
            }
            //Check to see if we have line of sight
            ShootLine shootLine;
            bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
            if (this.verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }
            if (this.ownerEquipment != null)
            {
                CompChangeableProjectile comp = this.ownerEquipment.GetComp<CompChangeableProjectile>();
                if (comp != null)
                {
                    //!!!!!!!!!!!!!!!! This could be used to subtract from a battery pack
                    comp.Notify_ProjectileLaunched();
                }
            }
            //Launcher is the user; equipment is what is being used to shoot
            Thing launcher = this.caster;
            Thing equipment = this.ownerEquipment;
            CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = this.caster;
            }
            //This drawPos may need to be modified so that the beam emerges from the weapon and not the user
            Vector3 drawPos = this.caster.DrawPos;
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, this.caster.Map);
            //Can the projectile strike something on its flightpath?
            projectile2.FreeIntercept = (this.canFreeInterceptNow && !projectile2.def.projectile.flyOverhead);
            //ShotReport gives us our chance that the shot actual hit
            ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
            //If the shot was a miss...
            if (Rand.Value > shotReport.ChanceToNotGoWild_IgnoringPosture)
            {
                if (DebugViewSettings.drawShooting)
                {
                    MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToWild", -1f);
                }
                //Modify the shoot line to hit something else
                shootLine.ChangeDestToMissWild();
                if (this.currentTarget.HasThing)
                {
                    //Ensure that the projectile actually hits something else
                    projectile2.ThingToNeverIntercept = this.currentTarget.Thing;
                }
                if (!projectile2.def.projectile.flyOverhead)
                {
                    projectile2.InterceptWalls = true;
                }
                //Launch doomed projectile for shootLine's bad destination
                //Consider giving the beam projectile a function to switch destinations when something gets between launcher and shootLine.Dest
                projectile2.Launch(launcher, drawPos, shootLine.Dest, equipment, this.currentTarget.Thing);
                return true;
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
                    if (!projectile2.def.projectile.flyOverhead)
                    {
                        projectile2.InterceptWalls = true;
                    }
                    projectile2.Launch(launcher, drawPos, randomCoverToMissInto, equipment, this.currentTarget.Thing);
                    return true;
                }
            }
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToHit", -1f);
            }
            if (!projectile2.def.projectile.flyOverhead)
            {
                projectile2.InterceptWalls = (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full);
            }
            if (this.currentTarget.Thing != null)
            {
                projectile2.Launch(launcher, drawPos, this.currentTarget, equipment, this.currentTarget.Thing);
            }
            else
            {
                projectile2.Launch(launcher, drawPos, shootLine.Dest, equipment, this.currentTarget.Thing);
            }
            return true;
        }

        public virtual void WarmupComplete()
        {
            this.burstShotsLeft = this.ShotsPerBurst;
            this.state = VerbState.Bursting;
            this.TryCastNextBurstShot();
            if (this.CasterIsPawn && this.currentTarget.HasThing)
            {
                Pawn pawn = this.currentTarget.Thing as Pawn;
                if (pawn != null && pawn.IsColonistPlayerControlled)
                {
                    this.CasterPawn.records.AccumulateStoryEvent(StoryEventDefOf.AttackedPlayer);
                }
            }
        }

        protected void TryCastNextBurstShot()
        {
            LocalTargetInfo localTargetInfo = this.currentTarget;
            //On successful firing of the beam
            if (this.TryCastShot())
            {
                if (this.verbProps.muzzleFlashScale > 0.01f)
                {
                    MoteMaker.MakeStaticMote(this.caster.Position, this.caster.Map, ThingDefOf.Mote_ShotFlash, this.verbProps.muzzleFlashScale);
                }
                if (this.verbProps.soundCast != null)
                {
                    this.verbProps.soundCast.PlayOneShot(new TargetInfo(this.caster.Position, this.caster.Map, false));
                }
                if (this.verbProps.soundCastTail != null)
                {
                    this.verbProps.soundCastTail.PlayOneShotOnCamera(this.caster.Map);
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
                this.burstShotsLeft--;
            }
            else
            {
                this.burstShotsLeft = 0;
            }
            if (this.burstShotsLeft > 0)
            {
                this.ticksToNextBurstShot = this.verbProps.ticksBetweenBurstShots;
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

    public override void PostDraw()
    {
        base.PostDraw();
        if (this.TicksLeft <= 0)
        {
            return;
        }
        Vector3 drawPos = this.parent.DrawPos;
        float num = ((float)this.parent.Map.Size.z - drawPos.z) * 1.41421354f;
        Vector3 a = Vector3Utility.FromAngleFlat(this.angle - 90f);
        Vector3 a2 = drawPos + a * num * 0.5f;
        a2.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
        float num2 = Mathf.Min((float)this.TicksPassed / 10f, 1f);
        Vector3 b = a * ((1f - num2) * num);
        float num3 = 0.975f + Mathf.Sin((float)this.TicksPassed * 0.3f) * 0.025f;
        if (this.TicksLeft < this.fadeOutDuration)
        {
            num3 *= (float)this.TicksLeft / (float)this.fadeOutDuration;
        }
        Color color = this.Props.color;
        color.a *= num3;
        CompOrbitalBeam.MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
        Matrix4x4 matrix = default(Matrix4x4);
        matrix.SetTRS(a2 + a * this.BeamEndHeight * 0.5f + b, Quaternion.Euler(0f, this.angle, 0f), new Vector3(this.Props.width, 1f, num));
        Graphics.DrawMesh(MeshPool.plane10, matrix, CompOrbitalBeam.BeamMat, 0, null, 0, CompOrbitalBeam.MatPropertyBlock);
        Vector3 pos = drawPos + b;
        pos.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
        Matrix4x4 matrix2 = default(Matrix4x4);
        matrix2.SetTRS(pos, Quaternion.Euler(0f, this.angle, 0f), new Vector3(this.Props.width, 1f, this.BeamEndHeight));
        Graphics.DrawMesh(MeshPool.plane10, matrix2, CompOrbitalBeam.BeamEndMat, 0, null, 0, CompOrbitalBeam.MatPropertyBlock);
    }

    public override void Draw()
    {
        base.Draw();
        Vector3 b = new Vector3(0.5f, 0f, 0.75f);
        if (base.Rotation == Rot4.North || base.Rotation == Rot4.South)
        {
            b = new Vector3(0f, 0f, 0.21875f);
        }
        if (base.Rotation == Rot4.East)
        {
            b = new Vector3(0.5f, 0f, 1.46875f);
        }
        if (base.Rotation == Rot4.West)
        {
            b = new Vector3(-0.5f, 0f, 1.46875f);
        }
        if (this.currentTargetInt.IsValid && this.firing > 0)
        {
            float alpha = DubUtils.MRC(0f, (float)this.burstTime.SecondsToTicks(), 0f, 1f, (float)this.firing);
            this.DrawArc(base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather) + b, this.currentTargetInt.Thing.DrawPos, FadedMaterialPool.FadedVersionOf(Graphics.obeliskBeam, alpha));
        }
        if (this.firing > 0 || this.burstWarmupTicksLeft > 0)
        {
            float alpha2 = DubUtils.MRC(0f, (float)this.warmupTime.SecondsToTicks(), 1f, 0f, (float)this.burstWarmupTicksLeft);
            Graphics.DrawMesh(Graphics.obeliskCharge.MeshAt(base.Rotation), base.DrawPos + Altitudes.AltIncVect, Quaternion.identity, FadedMaterialPool.FadedVersionOf(Graphics.obeliskCharge.MatAt(base.Rotation, null), alpha2), 0);
            return;
        }
        if (this.burstCooldownTicksLeft > 0)
        {
            float alpha3 = DubUtils.MRC((float)(this.lastBurstCooldownTicksLeft - 240), (float)this.lastBurstCooldownTicksLeft, 0f, 1f, (float)this.burstCooldownTicksLeft);
            Graphics.DrawMesh(Graphics.obeliskCharge.MeshAt(base.Rotation), base.DrawPos + Altitudes.AltIncVect, Quaternion.identity, FadedMaterialPool.FadedVersionOf(Graphics.obeliskCharge.MatAt(base.Rotation, null), alpha3), 0);
        }
    }


}
