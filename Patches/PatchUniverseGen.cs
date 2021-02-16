using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using MoreOrLessDense.Dtos;

namespace MoreOrLessDense.Patches {
    [HarmonyPatch(typeof(UniverseGen))]
    public static class PatchUniverseGen {
        // Change the star density
        [HarmonyPrefix, HarmonyPatch(typeof(UniverseGen), "GenerateTempPoses")]
        public static void Patch(ref int seed, ref int targetCount, ref int iterCount, ref double minDist, ref double minStepLen, ref double maxStepLen, ref double flatten) {
            if ( DensityDTO.density != null) {
                minDist = DensityDTO.density.minDist;
                minStepLen = DensityDTO.density.minStepLen;
                maxStepLen = DensityDTO.density.maxStepLen;
            }
        }
    }
}
