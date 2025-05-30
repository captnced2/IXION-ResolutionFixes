﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using IMHelper;
using UnityEngine;

namespace ResolutionFixes;

[BepInPlugin(Guid, Name, Version)]
[BepInProcess("IXION.exe")]
[BepInDependency("captnced.IMHelper")]
public class Plugin : BasePlugin
{
    private const string Guid = "captnced.ResolutionFixes";
    private const string Name = "ResolutionFixes";
    private const string Version = "1.1.0";
    internal new static ManualLogSource Log;
    internal static ConfigFile config;
    private static bool enabled = true;
    private static Harmony harmony;

    public override void Load()
    {
        Log = base.Log;
        config = Config;
        harmony = new Harmony(Guid);
        GameStateHelper.addSceneChangedListener(FixMainMenuUI, GameStateHelper.GameScene.MainMenu);
        GameStateHelper.addSceneChangedToInGameListener(FixInGameUI);
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
            FixMainMenuUI();
            init();
        }
        else
        {
            FixMainMenuUI(0, 0);
            disable();
        }
    }

    internal static void FixUI(Resolution res)
    {
        if (GameStateHelper.isInGame())
            FixInGameUI(res);
        else if (GameStateHelper.currentScene == GameStateHelper.GameScene.MainMenu) FixMainMenuUI(res);
        else Log.LogError("\"" + GameStateHelper.currentScene + " is not a valid scene to reposition UI");
    }

    private static void FixMainMenuUI()
    {
        if (!enabled) return;
        FixMainMenuUI(Screen.currentResolution.width, Screen.currentResolution.height);
    }

    private static void FixMainMenuUI(Resolution res)
    {
        if (!enabled) return;
        FixMainMenuUI(res.width, res.height);
    }

    private static void FixMainMenuUI(int w, int h)
    {
        var menu = GameObject.Find("Canvas/1920x1080/Canvas Group/Default").transform;
        var version = GameObject.Find("Canvas/1920x1080/Build Version").transform;
        if ((w == 2560 && h == 1080) ||
            (w == 3440 && h == 1440))
        {
            menu.localPosition = new Vector3(-300, 0, 0);
            version.localPosition = new Vector3(1250, -530, 0);
        }
        else
        {
            menu.localPosition = new Vector3(0, 0, 0);
            version.localPosition = new Vector3(950, -530, 0);
        }

        Log.LogInfo("Repositioned main menu UI");
    }

    private static void FixInGameUI()
    {
        FixInGameUI(Screen.currentResolution.width, Screen.currentResolution.height);
    }

    private static void FixInGameUI(Resolution res)
    {
        FixInGameUI(res.width, res.height);
    }

    private static void FixInGameUI(int w, int h)
    {
        if (!enabled) return;
        var sectors = GameObject.Find("Canvas/1920x1080/Bottom/Minimap Controller").transform;
        var camera = GameObject.Find("Canvas/1920x1080/Bottom/UI Camera Controls").transform;
        var navigation = GameObject.Find("Canvas/1920x1080/Bottom/Navigation Menu").transform;
        var objectives = GameObject.Find("Canvas/1920x1080/Top/Objective System").transform;
        var settings = GameObject.Find("Canvas/1920x1080/Top/Settings Button").transform;
        var travel = GameObject.Find("Canvas/1920x1080/Bottom/TravelInfos").transform;
        var spaceVehicles = GameObject.Find("Canvas/1920x1080/Space Vehicles").transform;
        var systemButtons = spaceVehicles.parent;
        var buildings = GameObject.Find("Canvas/1920x1080/WindowManagerRight").transform;
        if ((w == 2560 && h == 1080) ||
            (w == 3440 && h == 1440))
        {
            sectors.localPosition = new Vector3(-1260, -530, 0);
            camera.localPosition = new Vector3(-1273, -540, 0);
            navigation.localPosition = new Vector3(1275, -530, 0);
            objectives.localPosition = new Vector3(-310, 0, 0);
            settings.localPosition = new Vector3(320, 0, 0);
            travel.localPosition = new Vector3(310, 0, 0);
            systemButtons.localPosition = new Vector3(-565, 0, 0);
            spaceVehicles.localPosition = new Vector3(-480, -540, 0);
            buildings.localPosition = new Vector3(320, 0, 0);
        }
        else
        {
            sectors.localPosition = new Vector3(-950, -530, 0);
            camera.localPosition = new Vector3(-960, -540, 0);
            navigation.localPosition = new Vector3(945, -530, 0);
            objectives.localPosition = new Vector3(0, 0, 0);
            settings.localPosition = new Vector3(0, 0, 0);
            travel.localPosition = new Vector3(0, 0, 0);
            systemButtons.localPosition = new Vector3(0, 0, 0);
            spaceVehicles.localPosition = new Vector3(-950, -540, 0);
            buildings.localPosition = new Vector3(0, 0, 0);
        }

        Log.LogInfo("Repositioned in-game UI");
    }
}