using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Unlock_Cheat.RocketTile
{
    internal class RocketTile
    {
        [HarmonyPatch(typeof(RocketEnvelopeWindowTileConfig))]
        [HarmonyPatch("DoPostConfigureComplete")]
        public class RocketTile_RocketEnvelopeWindowTileConfig
        {
            public static void Postfix(ref GameObject go)
            {
                go.GetComponent<Deconstructable>().allowDeconstruction = true;
            }
        }

        [HarmonyPatch(typeof(RocketWallTileConfig))]
        [HarmonyPatch("DoPostConfigureComplete")]
        public class RocketTile_RocketWallTileConfig
        {
            public static void Postfix(ref GameObject go)
            {
                go.GetComponent<Deconstructable>().allowDeconstruction = true;
            }
        }


    }
}
