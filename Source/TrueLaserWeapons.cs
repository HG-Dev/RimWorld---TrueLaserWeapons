using System;
using System.Reflection;
using System.Collections.Generic;
using Verse;
using Harmony;
using RimWorld;
using UnityEngine;
using Suffixware;

namespace Suffixware.TrueLaserWeapons
{
    public class ModController : Mod
    {
        //public static Settings

        public ModController(ModContentPack content) : base(content)
        {
            // Apply Harmony patches
            HarmonyStarter.RunPatches();
        }

        public override string SettingsCategory()
        {
            return "True Laser Weapons";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            //TODO
            base.DoSettingsWindowContents(inRect);
        }
    }
}
