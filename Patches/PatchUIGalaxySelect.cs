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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace MoreOrLessDense.Patches {
    [HarmonyPatch(typeof(UIGalaxySelect))]
    public static class PatchUIGalaxySelect {

        // Values
        private static Slider starDensitySlider = null;
        private static Text starDensitySliderText = null;
        private static Text starDensityText = null;
        private static UIGalaxySelect uiGalaxySelect = null;
        private static float sliderDefaultValue = 4f;

        // Patches
        [HarmonyPrefix]
        [HarmonyPatch("_OnClose")]
        public static void _OnClose() {
            TeardownDensitySlider();
        }
        [HarmonyPostfix]
        [HarmonyPatch("_OnOpen")]
        public static void _OnOpen(ref UIGalaxySelect __instance) {
            CreateDensitySlider(__instance);
            DensityDTO.SetDensityInformation(starDensitySlider?.value);
            OnDensityChanged();
            Debug.Log("Started modifying star density");
        }

        // Methods
        public static void OnDensityChanged() {
            // Null checkup
            if ( !starDensitySlider || !starDensitySliderText ) {
                Debug.LogError($"More or Less Dense::OnDensityChanged() -- FATAL ERROR: missing slider or text for star density info");
                return;
            }

            // Update text
            starDensitySliderText.text = Math.Round(starDensitySlider.value * 0.25f, 2).ToString() + "x";

            // Regenerate galaxy
            DensityDTO.SetDensityInformation(starDensitySlider?.value);
            uiGalaxySelect.SetStarmapGalaxy();
        }
        private static void TeardownDensitySlider() {
            Debug.Log("Toredown slider");
            Vector3 astupidplace = new Vector3(100000f, 100000f, 100000f);
            if ( starDensitySlider != null ) {
                starDensitySlider.transform.position = astupidplace;
                starDensitySlider.onValueChanged.RemoveAllListeners();
                GameObject.Destroy(starDensitySlider);
                starDensitySlider = null;
            }
            if ( starDensityText != null ) {
                starDensityText.transform.position = astupidplace;
                GameObject.Destroy(starDensityText);
                starDensityText = null;
            }
            if ( starDensitySliderText != null ) {
                starDensitySliderText.transform.position = astupidplace;
                GameObject.Destroy(starDensitySliderText);
                starDensitySliderText = null;
            }
        }
        private static void CreateDensitySlider(UIGalaxySelect __instance) {
            TeardownDensitySlider();
            Debug.Log("Creating star density slider");

            if ( uiGalaxySelect == null ) {
                uiGalaxySelect = __instance;
            }

            // Create a new slider for star density
            if ( starDensitySlider == null ) {
                starDensitySlider = GameObject.Instantiate(__instance.starCountSlider, __instance.starCountSlider.transform);

                Vector3 newPosition = starDensitySlider.transform.position;
                newPosition.x -= 1f;
                newPosition.z += 5f;
                Vector3 newScale = starDensitySlider.transform.localScale;
                starDensitySlider.transform.position = newPosition;
                starDensitySlider.transform.localScale = newScale;
                starDensitySlider.minValue = 1f;
                starDensitySlider.maxValue = 10f;
                starDensitySlider.value = MoreOrLessDense.lastSetSliderValue.Value;
                starDensitySlider.wholeNumbers = true;
                starDensitySlider.onValueChanged.AddListener(delegate {
                    OnDensityChanged();
                });
                starDensitySliderText = GameObject.Instantiate(__instance.starCountText, starDensitySlider.transform);
                starDensitySliderText.text = "1x";
            }

            // Create a text to go along with the slider
            if ( starDensityText == null ) {
                starDensityText = GameObject.Instantiate(__instance.starCountText, starDensitySlider.transform);
                // normally id just move this down a little bit, but moving it in the y axis causes it to vanish. Sure, why not.
                Vector3 newPosition = starDensityText.transform.position;
                Vector3 newScale = starDensityText.transform.localScale;
                newPosition.x -= 2.5f;
                newScale.x *= 1.6f;
                newScale.y *= 1.6f;
                newScale.z *= 1.6f;
                starDensityText.transform.position = newPosition;
                starDensityText.transform.localScale = newScale;
                starDensityText.text = "Star Density";
            }
        }
    }
}
