using BepInEx;
using HarmonyLib;
using MoreOrLessDense.Dtos;
using MoreOrLessDense.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace MoreOrLessDense {
    [BepInPlugin("org.bepinex.plugins.moreorlessdense", "More or less dense", "1.0.0.0")]
    [BepInProcess("DSPGAME.exe")]
    public class MoreOrLessDense : BaseUnityPlugin {
        // Apply all patches
        void Start() {
            Harmony.CreateAndPatchAll(typeof(MoreOrLessDense));
        }
        
        private static readonly string configFolder = @"./BepInEx/config/MoreOrLessDense/";
        private static DensityDTO densityDto = null;

        private static Slider starDensitySlider = null;
        private static Text starDensityText = null;
        private static UIGalaxySelect uiGalaxySelect = null;

        // Change the minimum and maximum values of the galaxy select slider
        [HarmonyPrefix, HarmonyPatch(typeof(UIGalaxySelect), "UpdateUIDisplay")]
        public static void Patch(ref UIGalaxySelect __instance, GalaxyData galaxy) {

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            Slider starCountSlider = (Slider)ReflectionHelper.GetField(__instance, "starCountSlider", flags);
            Text starCountText = (Text)ReflectionHelper.GetField(__instance, "starCountText", flags);

            if ( uiGalaxySelect == null ) {
                uiGalaxySelect = __instance;
            }

            // Create a new slider for star density
            if ( starDensitySlider == null ) {
                starDensitySlider = Instantiate(starCountSlider, starCountSlider.transform, starCountSlider.transform.parent);
                Vector3 newPosition = starDensitySlider.transform.position;
                //Debug.Log($"slider is at ({newPosition.x}, {newPosition.y}, {newPosition.z})");
                newPosition.x *= 1f;
                newPosition.y *= 1f;
                newPosition.z *= 1.25f;
                Vector3 newScale = starDensitySlider.transform.localScale;
                newScale.x *= 1.25f;
                newScale.y *= 1.25f;
                newScale.z *= 1.25f;
                //Debug.Log($"slider is now at ({newPosition.x}, {newPosition.y}, {newPosition.z})");
                starDensitySlider.transform.position = newPosition;
                starDensitySlider.transform.localScale = newScale;
                starDensitySlider.minValue = 1f;
                starDensitySlider.maxValue = 10f;
                starDensitySlider.value = 4f;
                starDensitySlider.wholeNumbers = true;
                starDensitySlider.onValueChanged.AddListener(delegate {
                    OnValueChanged();
                });
            }

            // Create a text to go along with the slider
            if ( starDensityText == null ) {
                starDensityText = Instantiate(starCountText, starCountText.transform);
                // normally id just move this down a little bit, but moving it in the y axis causes it to vanish. not sure why.
                Vector3 newPosition = starDensityText.transform.position;
                Vector3 newScale = starDensityText.transform.localScale;
                newPosition.x -= 1.75f;
                newPosition.y -= 1f;
                newPosition.z -= 1f;
                newScale.x *= 1.5f;
                newScale.y *= 1.5f;
                newScale.z *= 1.5f;
                starDensityText.transform.position = newPosition;
                starDensityText.transform.localScale = newScale;
                starDensityText.text = "Density";
            }

        }

        public static void OnValueChanged() {
            // Update text
            //starDensitySlider.

            // Regenerate galaxy
            uiGalaxySelect.SetStarmapGalaxy();
            SetDensityInformation(starDensitySlider.value);
        }
        
        // Teardown
        [HarmonyPostfix, HarmonyPatch(typeof(UIGalaxySelect), "_OnDestroy")]
        public static void Patch() {
            if ( starDensitySlider != null ) {
                starDensitySlider.onValueChanged.RemoveAllListeners();
                starDensitySlider.transform.position.Set(1000, 1000, 1000);
                starDensitySlider = null;
            }
            if ( starDensityText != null ) {
                starDensityText = null;
            }
            densityDto = null;
        }

        // Update density information
        public static void SetDensityInformation(float sliderValue) {
            if (densityDto == null) {
                densityDto = new DensityDTO();
            }

            // Actual slider range is 0.25 to 2.5x
            double value = starDensitySlider == null ? 1d : starDensitySlider.value * 0.25;

            // Thank god for desmos.com/calculator
            densityDto.minDist = Math.Round(Math.Log(value + 2.05d, 2d), 1);
            densityDto.minStepLen = Math.Round(Math.Log(value + 2.05d, 2d), 1);
            densityDto.maxStepLen = Math.Round(Math.Exp(( value / 2d ) + 0.4d), 1);
        }
        
        // Change the star density
        [HarmonyPrefix, HarmonyPatch(typeof(UniverseGen), "GenerateTempPoses")]
        public static void Patch(ref int seed, ref int targetCount, ref int iterCount, ref double minDist, ref double minStepLen, ref double maxStepLen, ref double flatten) {
            if (densityDto != null) {
                minDist = densityDto.minDist;
                minStepLen = densityDto.minStepLen;
                maxStepLen = densityDto.maxStepLen;
            }
        }
        // Save density information
        public static void SaveDensityInfo(string saveName, DensityDTO densityInformation) {
            string path = $"{configFolder}\\{saveName}.txt";
            Debug.Log($"More or Less Dense -- Saving {saveName} at {path}");
            if ( !Directory.Exists(configFolder) ) {
                Directory.CreateDirectory(configFolder);
            }
            File.WriteAllText(path, densityInformation.ToString());
        }
        // Load density information
        public static DensityDTO LoadDensityInfo(string saveName) {
            if ( !Directory.Exists(configFolder) ) {
                Directory.CreateDirectory(configFolder);
            }
            string path = $"{configFolder}\\{saveName}.txt";
            Debug.Log($"More or Less Dense -- Loading save {saveName} at {path}");
            if (!File.Exists(path) ) {
                return null;
            }
            return DensityDTO.FromString(File.ReadAllText(path));
        }
        // Hook game save
        [HarmonyPrefix, HarmonyPatch(typeof(GameSave), "SaveCurrentGame")]
        public static void Patch (string saveName) {
            if (densityDto != null) {
                try {
                    SaveDensityInfo(saveName, densityDto);
                }
                catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }
        }
        // Hook game load
        [HarmonyPrefix, HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
        public static void Prefix(string saveName) {
            try {
                densityDto = LoadDensityInfo(saveName);
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
                densityDto = null;
            }
        }
    }
}
