using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using MoreOrLessDense.Dtos;
using System.IO;

namespace MoreOrLessDense.Patches {
    [HarmonyPatch(typeof(GameSave))]
    public static class PatchGameSave {
        private static readonly string configFolder = @"./BepInEx/config/MoreOrLessDense";

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
                if ( !File.Exists(path) ) {
                    Debug.Log($"More or Less Dense -- No save file found");
                    return false;
                }
                density = DensityDTO.FromString(File.ReadAllText(path));
                return true;
            }
            catch {
                Debug.LogWarning($"More or Less Dense -- possible bad path for save file at '{path}'");
                return false;
            }
        }
        // Hook game save
        [HarmonyPrefix, HarmonyPatch(typeof(GameSave), "SaveCurrentGame")]
        public static void Patch(string saveName) {
            if ( DensityDTO.density == null ) {
                Debug.Log($"More or Less Dense::SaveCurrentGame() -- No information to save");
            }
            if ( DensityDTO.density != null) {
                try {
                    SaveDensityInfo(saveName, DensityDTO.density);
                    Debug.Log($"Saved density information: {DensityDTO.density.minDist};{DensityDTO.density.minStepLen};{DensityDTO.density.maxStepLen}");
                }
                catch ( Exception e ) {
                    Debug.LogError($"More or Less Dense::SaveCurrentGame() -- FATAL ERROR: {e.Message}");
                }
            }
        }
        // Hook game load
        [HarmonyPrefix, HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
        public static void Prefix(string saveName) {
            if ( string.IsNullOrEmpty(saveName) ) {
                Debug.LogError("More or Less Dense::LoadCurrentGame() -- Can't load nameless save. Is this the main menu?");
            }
            try {
                if ( LoadDensityInfo(saveName, out DensityDTO value) ) {
                    DensityDTO.density = value;
                    Debug.Log($"Loaded density information: {DensityDTO.density.minDist};{DensityDTO.density.minStepLen};{DensityDTO.density.maxStepLen}");
                }
                else {
                    DensityDTO.density = null;
                }
            }
            catch ( Exception e ) {
                Debug.LogError($"More or Less Dense::LoadCurrentGame() -- FATAL ERROR: {e.Message}");
                DensityDTO.density = null;
            }
        }
    }
}
