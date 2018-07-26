using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Suffixware.TrueLaserWeapons
{
    class Projectile_Beam : Projectile
    {
        // Draw materials
        public Material warmupTexture;
        public Material flashTexture;
        public Material beamTexture;

        // Effect settings
        public float warmupDrawTicks;
        public float flashDrawTicks;
        /*
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            //TODO: Change beam/flash/warmup to be loaded based on strings from XML
            beamTexture = GraphicsLookup.Beam;
            flashTexture = GraphicsLookup.Orb;
            warmupTexture = GraphicsLookup.Dot;
        }

        public virtual void DrawArc(Vector3 start, Vector3 end, Material mat)
        {
            if (Mathf.Abs(start.x - end.x) < 0.01f && Mathf.Abs(start.z - end.z) < 0.01f)
            {
                return;
            }
            Vector3 pos = (start + end) / 2f;
            if (start == end)
            {
                return;
            }
            start.y = end.y;
            float z = (start - end).MagnitudeHorizontal();
            Quaternion q = Quaternion.LookRotation(start - end);
            Vector3 s = new Vector3(1f, 1f, z);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, q, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
        }

        public override void Draw()
        {
            Graphics.DrawMesh(MeshPool.circle, this.DrawPos, this.ExactRotation, this.warmupTexture, 0);
            Graphics.DrawMesh(MeshPool.circle, this.assignedTarget.DrawPos, this.ExactRotation, this.flashTexture, 0);
            /*
            this.DrawArc(this.flashTexture);
            this.DrawArc(base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Projectile) + b, this.currentTargetInt.Thing.DrawPos, FadedMaterialPool.FadedVersionOf(Graphics.obeliskBeam, alpha));
            base.Draw();*/
        //}
    }
}
