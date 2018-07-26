using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Suffixware.TrueLaserWeapons
{
    class VerbProperties_ShootBeam : VerbProperties
    {
        //public float warmupTime; exists by default
        //The amount of time the specialized muzzle flash effect is drawn, delaying beam firing
        //Setting flashTime equal to contact time means that the beam and flash sprites are drawn as one
        public float flashTime = 0f;
        //The length of the beam pulse
        public float pulseTime = 1f;
        //Whether or not this beam affects all targets along its range
        public bool canPierceAllInRange = false;

    }
}
