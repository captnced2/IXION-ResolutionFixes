using System.IO;
using HarmonyLib;
using Stanford.Settings.Video;
using UnityEngine;

namespace ResolutionFixes;

public class Patches
{
    [HarmonyPatch(typeof(ResolutionSetting), nameof(ResolutionSetting.OnApplyValue))]
    public static class ResolutionOnApplyPatch
    {
        private static int count;

        public static void Postfix(ResolutionSetting __instance)
        {
            count++;
            if (count == 2)
            {
                if (!File.Exists(Plugin.config.ConfigFilePath))
                {
                    count++;
                    return;
                }

                var text = File.ReadAllText(Plugin.config.ConfigFilePath);
                text = text.Replace("x ", "").Replace("@ ", "").Replace("Hz", "");
                var array = text.Split(' ');
                int[] arr = [int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2])];
                var i = 0;
                foreach (var r in __instance.resolutions)
                {
                    if (r.width == arr[0] && r.height == arr[1] && r.refreshRate == arr[2])
                    {
                        if (Screen.currentResolution.width == r.width && Screen.currentResolution.height == r.height &&
                            Screen.currentResolution.refreshRate == r.refreshRate)
                        {
                            count++;
                        }
                        else
                        {
                            __instance.SetValue(i);
                            Plugin.Log.LogInfo("Set resolution to \"" + r.width + " x " + r.height + " @ " +
                                               r.refreshRate +
                                               "Hz\"");
                        }

                        return;
                    }

                    i++;
                }

                Plugin.Log.LogError("Failed to find resolution \"" + arr[0] + " x " + arr[1] + " @ " + arr[0] +
                                    "Hz\" in resolutions list! Try changing the resolution in the game settings");
            }
            else if (count > 3)
            {
                File.WriteAllText(Plugin.config.ConfigFilePath, __instance.GetResolution().ToString());
                Plugin.Log.LogInfo("Saved resolution \"" + __instance.GetResolution() + "\" to config");
                Plugin.FixUI(__instance.GetResolution());
            }
        }
    }
}