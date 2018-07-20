using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Suffixware.Util
{
    public static class SocialInfoUtility
    {
        private static Dictionary<Pawn, CachedSocialEntries> cachedEntries = new Dictionary<Pawn, CachedSocialEntries>();

        public static List<SocialEntry> GetSocialEntries(Pawn selPawnForSocialInfo)
        {
            if (!cachedEntries.ContainsKey(selPawnForSocialInfo)) //TODO: Refresh old entries
            {
                var newlyCached = new CachedSocialEntries();
                newlyCached.lastUpdated = Math.Min(Time.frameCount, int.MaxValue);
                newlyCached.socialEntries = SocialInfoUtility.FindSocialEntries(selPawnForSocialInfo);
                cachedEntries[selPawnForSocialInfo] = newlyCached;
            }
            return cachedEntries[selPawnForSocialInfo].socialEntries;
        }

        private static List<SocialEntry> FindSocialEntries(Pawn selPawnForSocialInfo)
        {
            List<SocialEntry> entries = new List<SocialEntry>();
            HashSet<Pawn> knownPawns = new HashSet<Pawn>();
            foreach (Pawn relativePawn in selPawnForSocialInfo.relations.RelatedPawns)
            {
                //Add all relatives of the pawn
                knownPawns.Add(relativePawn);
            }
            if (selPawnForSocialInfo.MapHeld != null)
            {
                foreach (var mapPawn in selPawnForSocialInfo.MapHeld.mapPawns.AllPawnsSpawned)
                {
                    //Add all other pawns the pawn cares about
                    if (mapPawn.RaceProps.Humanlike
                        && mapPawn != selPawnForSocialInfo
                        && !knownPawns.Contains(mapPawn))
                    {
                        knownPawns.Add(mapPawn);
                    }
                }
            }
            foreach (var knownPawn in knownPawns)
            {
                var entry = new SocialEntry()
                {
                    otherPawn = knownPawn,
                    opinionOfOtherPawn = selPawnForSocialInfo.relations.OpinionOf(knownPawn),
                    opinionOfMe = knownPawn.relations.OpinionOf(selPawnForSocialInfo),
                    relations = new List<PawnRelationDef>()
                };
                foreach (var relationType in selPawnForSocialInfo.GetRelations(knownPawn))
                {
                    entry.relations.Add(relationType);
                    //Consider revealing the offset of the relations for finer control and "duty" triggers
                    //"Duty": For instance, your new spouse's child you aren't yet acquainted with
                }
                if (entry.opinionOfOtherPawn != 0)
                {
                    entries.Add(entry);
                }
            }
            return entries;
        }

        public struct CachedSocialEntries
        {
            public int lastUpdated;
            public List<SocialEntry> socialEntries;
        }

        public struct SocialEntry
        {
            public Pawn otherPawn;
            public int opinionOfOtherPawn;
            public int opinionOfMe;
            public List<PawnRelationDef> relations;
            public int relationOffset;
        }
    }
}
