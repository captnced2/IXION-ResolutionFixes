using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using IMHelper;
using UnityEngine;
using UnityEngine.UI;

namespace ResolutionFixes;

[BepInPlugin(Guid, Name, Version)]
[BepInProcess("IXION.exe")]
[BepInDependency("captnced.IMHelper")]
public class Plugin : BasePlugin
{
    private const string Guid = "captnced.ResolutionFixes";
    private const string Name = "ResolutionFixes";
    private const string Version = "2.0.0";
    internal new static ManualLogSource Log;
    internal static ConfigFile config;
    internal static bool enabled = true;
    private static Harmony harmony;
    private static MonoHelper monoHelper;

    public override void Load()
    {
        Log = base.Log;
        config = Config;
        harmony = new Harmony(Guid);
        monoHelper = AddComponent<MonoHelper>();
        if (IL2CPPChainloader.Instance.Plugins.ContainsKey("captnced.IMHelper")) enabled = ModsMenu.isSelfEnabled();
        if (!enabled)
            Log.LogInfo("Disabled by IMHelper!");
        else
            init();
    }

    private static void init()
    {
        harmony.PatchAll();
        foreach (var patch in harmony.GetPatchedMethods())
            Log.LogInfo("Patched " + patch.DeclaringType + ":" + patch.Name);
        Log.LogInfo("Loaded \"" + Name + "\" version " + Version + "!");
    }

    private static void disable()
    {
        harmony.UnpatchSelf();
        Log.LogInfo("Unloaded \"" + Name + "\" version " + Version + "!");
    }

    public static void enable(bool value)
    {
        enabled = value;
        if (enabled)
        {
            FixMainMenuUI(Screen.currentResolution.width, Screen.currentResolution.height);
            init();
        }
        else
        {
            FixMainMenuUI(1920, 1080);
            disable();
        }
    }

    internal static void FixMainMenuUI(int w, int h)
    {
        foreach (var a in Resources.FindObjectsOfTypeAll<AspectRatioFitter>()) a.aspectRatio = (float)w / h;
    }

    internal static void FixInGameUI(int w, int h)
    {
        var science = GameObject.Find("Canvas/1920x1080/Top/Top Bar/Static/Science");
        var scienceIcon = science.transform.FindChild("Icon");
        if (scienceIcon != null)
        {
            var o = new GameObject("Science ResolutionFix");
            o.transform.SetParent(science.transform.parent);
            scienceIcon.transform.SetParent(o.transform);
        }

        foreach (var a in Resources.FindObjectsOfTypeAll<AspectRatioFitter>())
        {
            a.enabled = true;
            a.aspectRatio = (float)w / h;
        }

        monoHelper.StartCoroutine(fixUiDelayed(1f));
    }

    private static IEnumerator fixUiDelayed(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        var systemButtons = GameObject.Find("Canvas/1920x1080/Space Vehicles").transform.parent;
        systemButtons.localPosition = new Vector3(-245, 0, 0);
        var rect = GameObject.Find("Canvas/1920x1080/Top/Top Bar/Static/Science").GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
        while (rect.rect.x is < -40 or > -30)
            rect.sizeDelta = rect.rect.x < -40
                ? new Vector2(rect.sizeDelta.x - 10, rect.sizeDelta.y)
                : new Vector2(rect.sizeDelta.x + 10, rect.sizeDelta.y);

        foreach (var a in Resources.FindObjectsOfTypeAll<AspectRatioFitter>()) a.enabled = false;
    }

    private class MonoHelper : MonoBehaviour
    {
    }
}