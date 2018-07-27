using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Suffixware
{
    /// <summary>
    /// Contains links to textures that could not be included in XML files.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class GraphicsLookup
    {
        public static readonly Material Dot = MaterialPool.MatFrom("Things/FX/RA_dot", ShaderDatabase.MoteGlow);
        public static readonly Material Orb = MaterialPool.MatFrom("Things/FX/RA_glow", ShaderDatabase.MoteGlow);
        public static readonly Material Beam = MaterialPool.MatFrom("Things/FX/RA_beam", ShaderDatabase.MoteGlow);
    }
}
