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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace MoreOrLessDense {
    [BepInPlugin("org.bepinex.plugins.moreorlessdense", "More or less dense", "1.0.2")]
    [BepInIncompatibility("Denseiverse")]
    [BepInProcess("DSPGAME.exe")]
    public class MoreOrLessDense : BaseUnityPlugin {
        // Apply all patches
        void Start() {
            Harmony.CreateAndPatchAll(typeof(MoreOrLessDense));
        }
        
        private static readonly string configFolder = @"./BepInEx/config/MoreOrLessDense";
        private static DensityDTO densityDto = null;

        private static Slider starDensitySlider = null;
        private static Text starDensitySliderText = null;
        private static Text starDensityText = null;
        private static UIGalaxySelect uiGalaxySelect = null;
        private static float sliderDefaultValue = 4f;

        private static bool shitsFuckedYo = false;

        public static void OnDensityChanged() {
            if (shitsFuckedYo) {
                return;
            }

            if (!starDensitySlider || !starDensitySliderText) {
                Debug.LogError($"More or Less Dense::OnDensityChanged() -- FATAL ERROR: missing slider or text for star density info");
                shitsFuckedYo = true;
                return;
            }
            
            // Update text
            starDensitySliderText.text = Math.Round(starDensitySlider.value * 0.25f, 2).ToString() + "x";

            // Regenerate galaxy
            SetDensityInformation(starDensitySlider.value);
            uiGalaxySelect.SetStarmapGalaxy();
        }

        // Buildup
        [HarmonyPostfix, HarmonyPatch(typeof(UIGalaxySelect), "_OnOpen")]
        public static void Patch(ref UIGalaxySelect __instance) {
            if (shitsFuckedYo) {
                Debug.LogError("More or Less Dense::_OnOpen() -- refused to start because plugin had errored.");
                return;
            }
            CreateDensitySlider(__instance);
            SetDensityInformation(starDensitySlider.value);
            OnDensityChanged();
            Debug.Log("Started modifying star density");
        }

        private static void CreateDensitySlider(UIGalaxySelect __instance) {
            TeardownDensitySlider();
            Debug.Log("creating star density slider");

            if ( uiGalaxySelect == null ) {
                uiGalaxySelect = __instance;
            }

            // Create a new slider for star density
            if ( starDensitySlider == null ) {
                starDensitySlider = Instantiate(__instance.starCountSlider, __instance.starCountSlider.transform);

                Vector3 newPosition = starDensitySlider.transform.position;
                //Debug.Log($"slider is at ({newPosition.x}, {newPosition.y}, {newPosition.z})");
                newPosition.x -= 1f;
                newPosition.z += 5f;
                Vector3 newScale = starDensitySlider.transform.localScale;
                //Debug.Log($"slider is now at ({newPosition.x}, {newPosition.y}, {newPosition.z})");
                //newScale.x *= 1.25f;
                //newScale.y *= 1.25f;
                //newScale.z *= 1.25f;
                starDensitySlider.transform.position = newPosition;
                starDensitySlider.transform.localScale = newScale;
                starDensitySlider.minValue = 1f;
                starDensitySlider.maxValue = 10f;
                starDensitySlider.value = sliderDefaultValue;
                starDensitySlider.wholeNumbers = true;
                starDensitySlider.onValueChanged.AddListener(delegate {
                    OnDensityChanged();
                });
                starDensitySliderText = Instantiate(__instance.starCountText, starDensitySlider.transform);
                starDensitySliderText.text = "1x";
            }

            // Create a text to go along with the slider
            if ( starDensityText == null ) {
                starDensityText = Instantiate(__instance.starCountText, starDensitySlider.transform);
                // normally id just move this down a little bit, but moving it in the y axis causes it to vanish. not sure why.
                Vector3 newPosition = starDensityText.transform.position;
                Vector3 newScale = starDensityText.transform.localScale;
                newPosition.x -= 2.5f;
                //newPosition.y += 1f;
                //newPosition.z -= 1.5f;
                newScale.x *= 1.6f;
                newScale.y *= 1.6f;
                newScale.z *= 1.6f;
                starDensityText.transform.position = newPosition;
                starDensityText.transform.localScale = newScale;
                starDensityText.text = "Star Density";

            }
        }

        // Teardown
        [HarmonyPrefix, HarmonyPatch(typeof(UIGalaxySelect), "_OnClose")]
        public static void Patch() {
            TeardownDensitySlider();
        }
        private static void TeardownDensitySlider() {
            Debug.Log("Toredown slider");
            Vector3 astupidplace = new Vector3(100000f, 100000f, 100000f);
            if ( starDensitySlider != null ) {
                starDensitySlider.transform.position = astupidplace;
                starDensitySlider.onValueChanged.RemoveAllListeners();
                Destroy(starDensitySlider);
                starDensitySlider = null;
            }
            if ( starDensityText != null ) {
                starDensityText.transform.position = astupidplace;
                Destroy(starDensityText);
                starDensityText = null;
            }
            if (starDensitySliderText != null) {
                starDensitySliderText.transform.position = astupidplace;
                Destroy(starDensitySliderText);
                starDensitySliderText = null;
            }
        }

        // Update density information
        public static void SetDensityInformation(float sliderValue) {
            if (shitsFuckedYo) {
                return;
            }

            if (densityDto == null) {
                densityDto = new DensityDTO();
            }

            // Actual slider range is 0.25 to 2.5x
            double value = starDensitySlider == null ? 1d : ((int)starDensitySlider.value) * 0.25d;

            // Thank god for desmos.com/calculator
            densityDto.minDist = Math.Round(Math.Log(value + 2.05d, 2d), 1);
            densityDto.minStepLen = Math.Round(Math.Log(value + 2.05d, 2d), 1);
            densityDto.maxStepLen = Math.Round(Math.Exp(( value / 2d ) + 0.4d), 1);
            //Debug.Log($"density debug A: {( starDensitySlider == null ? "null" : $"{starDensitySlider.value}: {densityDto.minDist}; {densityDto.minStepLen}; {densityDto.maxStepLen}" )}");
        }
        
        // Change the star density
        [HarmonyPrefix, HarmonyPatch(typeof(UniverseGen), "GenerateTempPoses")]
        public static void Patch(ref int seed, ref int targetCount, ref int iterCount, ref double minDist, ref double minStepLen, ref double maxStepLen, ref double flatten) {
            if (densityDto != null && !shitsFuckedYo) {
                minDist = densityDto.minDist;
                minStepLen = densityDto.minStepLen;
                maxStepLen = densityDto.maxStepLen;
            }
            //Debug.Log($"density debug B: {(densityDto == null ? "null": $"{starDensitySlider.value}: {minDist}; {minStepLen}; {maxStepLen}")}");
        }
        
        // Save density information
        public static void SaveDensityInfo(string saveName, DensityDTO densityInformation) {
            string path = $"{configFolder}/{saveName}.txt";
            Debug.Log($"More or Less Dense -- Saving {saveName} at {path}");
            if ( !Directory.Exists(configFolder) ) {
                Directory.CreateDirectory(configFolder);
            }
            File.WriteAllText(path, densityInformation.ToString());
        }
        // Load density information
        public static bool LoadDensityInfo(string saveName, out DensityDTO density) {
            density = null;
            if ( !Directory.Exists(configFolder) ) {
                Directory.CreateDirectory(configFolder);
            }
            string path = $"{configFolder}/{saveName}.txt";
            Debug.Log($"More or Less Dense -- Loading save {saveName} at {path}");
            try {
                if (!File.Exists(path) ) {
                    Debug.Log($"More or Less Dense -- No save file found");
                    return false;
                }
                density = DensityDTO.FromString(File.ReadAllText(path));
                return true;
            }
            catch (Exception e) {
                Debug.LogWarning($"More or Less Dense -- possible bad path for save file at '{path}'");
                return false;
            }
        }
        // Hook game save
        [HarmonyPrefix, HarmonyPatch(typeof(GameSave), "SaveCurrentGame")]
        public static void Patch (string saveName) {
            if (densityDto == null) {
                Debug.Log($"More or Less Dense::SaveCurrentGame() -- No information to save");
            }
            if (densityDto != null && !shitsFuckedYo) {
                try {
                    SaveDensityInfo(saveName, densityDto);
                    Debug.Log($"Saved density information: {densityDto.minDist};{densityDto.minStepLen};{densityDto.maxStepLen}");
                }
                catch (Exception e) {
                    Debug.LogError($"More or Less Dense::SaveCurrentGame() -- FATAL ERROR: {e.Message}");
                    shitsFuckedYo = true;
                }
            }
        }
        // Hook game load
        [HarmonyPrefix, HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
        public static void Prefix(string saveName) {
            if ( string.IsNullOrEmpty(saveName) ) {
                Debug.LogError("More or Less Dense::LoadCurrentGame() -- Can't load nameless save. Is this the main menu?");
            }
            if (!shitsFuckedYo ) {
                try {
                    if ( LoadDensityInfo(saveName, out DensityDTO density) ) {
                        densityDto = density;
                        Debug.Log($"Loaded density information: {densityDto.minDist};{densityDto.minStepLen};{densityDto.maxStepLen}");
                    }
                    else {
                        densityDto = null;
                    }
                }
                catch (Exception e) {
                    Debug.LogError($"More or Less Dense::LoadCurrentGame() -- FATAL ERROR: {e.Message}");
                    densityDto = null;
                    shitsFuckedYo = true;
                }
            }
        }
    }
}
