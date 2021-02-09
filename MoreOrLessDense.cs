using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoreOrLessDense
{
    [BepInPlugin("org.bepinex.plugins.moreorlessdense", "More or less dense", "1.0.0.0")]
    [BepInProcess("DSPGAME.exe")]
    public class MoreOrLessDense : BaseUnityPlugin {
        // Apply all patches
        void Start() {
            Harmony.CreateAndPatchAll(typeof(MoreOrLessDense));
        }

        private static readonly int desiredMinStars = 10;
        private static readonly int desiredMaxStars = 200;

        private static Slider starDensitySlider = null;
        private static Text starDensityText = null;
        private static UIGalaxySelect uiGalaxySelect = null;

        // Change the minimum and maximum values of the galaxy select slider
        [HarmonyPrefix, HarmonyPatch(typeof(UIGalaxySelect), "UpdateUIDisplay")]
        public static void Patch(ref UIGalaxySelect __instance, GalaxyData galaxy) {

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            Slider starCountSlider = (Slider)ReflectionHelper.GetField(__instance, "starCountSlider", flags);
            Text starCountText = (Text)ReflectionHelper.GetField(__instance, "starCountText", flags);

            starCountSlider.minValue = desiredMinStars;
            starCountSlider.maxValue = desiredMaxStars;

            if ( uiGalaxySelect == null ) {
                uiGalaxySelect = __instance;
            }

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
        }

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
        }

        [HarmonyPrefix, HarmonyPatch(typeof(UniverseGen), "GenerateTempPoses")]
        public static void Patch(ref int seed, ref int targetCount, ref int iterCount, ref double minDist, ref double minStepLen, ref double maxStepLen, ref double flatten) {
            double value;
            if ( starDensitySlider == null ) {
                value = 1d;
            }
            else {
                value = starDensitySlider.value * 0.25;  // Actual range is 0.25 to 2.5x
            }
            // Thank god for algebra and desmos.com/calculator
            minDist = Math.Round(Math.Log(value + 2.05d, 2d), 1);
            minStepLen = Math.Round(Math.Log(value + 2.05d, 2d), 1);
            maxStepLen = Math.Round(Math.Exp(( value / 2d ) + 0.4d), 1);
            Debug.Log($"moreorlessstars -- value:{value} minDist:{minDist} minStepLen:{minStepLen} maxStepLen:{maxStepLen}");
        }

        // Remove the hard-coded star limits
        [HarmonyPrefix, HarmonyPatch(typeof(UIGalaxySelect), "OnStarCountSliderValueChange")]
        public static bool Patch(ref UIGalaxySelect __instance, ref float val) {
            // Gather required private fields
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            Slider starCountSlider = (Slider)ReflectionHelper.GetField(__instance, "starCountSlider", flags);
            GameDesc gameDesc = (GameDesc)ReflectionHelper.GetField(__instance, "gameDesc", flags);

            // Replicate the code of the original method
            int num = (int)( starCountSlider.value + 0.1f );
            if ( num < desiredMinStars ) {
                num = desiredMinStars;
            }
            else if ( num > desiredMaxStars ) {
                num = desiredMaxStars;
            }
            if ( num != gameDesc.starCount ) {
                gameDesc.starCount = num;
                __instance.SetStarmapGalaxy();
            }

            // Return to prevent the call of the original method
            return false;
        }
    }
}
