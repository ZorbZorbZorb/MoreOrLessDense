using BepInEx;
using HarmonyLib;
using MoreOrLessDense.Dtos;
using MoreOrLessDense.Helpers;
using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Configuration;
using MoreOrLessDense.Patches;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace MoreOrLessDense {
    [BepInPlugin("org.bepinex.plugins.moreorlessdense", "More or less dense", "1.0.4")]
    [BepInIncompatibility("Denseiverse")]
    [BepInProcess("DSPGAME.exe")]
    public class MoreOrLessDense : BaseUnityPlugin {
        public static ConfigEntry<float> lastSetSliderValue;
        
        internal void Awake() {
            Harmony harmony = new Harmony("org.bepinex.plugins.moreorlessdense");

            lastSetSliderValue = Config.Bind("Memory", "lastSetSliderValue", 4f, "The last value you had the slider set to");

            Harmony.CreateAndPatchAll(typeof(PatchGameSave), harmony.Id);
            Harmony.CreateAndPatchAll(typeof(PatchUIGalaxySelect), harmony.Id);
            Harmony.CreateAndPatchAll(typeof(PatchUniverseGen), harmony.Id);

        }
    }
}
