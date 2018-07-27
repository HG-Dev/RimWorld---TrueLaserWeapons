using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Suffixware
{
    /// <summary>
    /// A context-blind beam renderer.
    /// </summary>
    [StaticConstructorOnStartup]
    class CompBeam : ThingComp
    {
        public CompProperties_OrbitalBeam Props
        {
            get
            {
                return (CompProperties_OrbitalBeam)this.props;
            }
        }

        public bool Rendering = false;
        public Vector3 start = new Vector3(0f, 0f, 0f);
        public Vector3 target = new Vector3(0f, 0f, 0f);
        public Vector2 trim = new Vector2(0f, 0f); //Amount to shave off from length of beam (start, target)

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow);

        protected float WidthScalar
        {
            get { return Props.width / BeamMat.mainTexture.width; }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!Rendering
                || (Mathf.Abs(start.x - target.x) < 0.01f && Mathf.Abs(start.z - target.z) < 0.01f)
                || start == target) //Don't bother drawing a very short arc
            {
                return;
            }
            start.y = target.y; //Copy altitude level from that of target
            Vector3 midpoint = (start + target) / 2f;
            float mag = (start - target).MagnitudeHorizontal();
            Quaternion rotate = Quaternion.LookRotation(start - target);
            Vector3 scalar = new Vector3(WidthScalar, 1f, mag);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(midpoint, rotate, scalar);
            Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0);
        }
    }
}
