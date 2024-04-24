using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RobotMod.Patches;
using RobotMod.Robot;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LethalLib.Modules;

namespace RobotMod
{

    [BepInPlugin(modGUID, modName, modVersion)]
    public class RobotModBase : BaseUnityPlugin
    {
        private const string modGUID = "AIRobot.LCMod";
        private const string modName = "Robot Mod";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static RobotModBase Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("The robot mod is alive");
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "robotmod");
            mls.LogInfo("ASSET DIR: " + assetDir);

            harmony.PatchAll(typeof(RobotModBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(RobotItem));
            harmony.PatchAll(typeof(RobotAI));



            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            
            Item RobotItem = bundle.LoadAsset<Item>("Assets/RobotItem.asset");
            NetworkPrefabs.RegisterNetworkPrefab(RobotItem.spawnPrefab);
            Utilities.FixMixerGroups(RobotItem.spawnPrefab);

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "This is info about the robot\n\n";
            Items.RegisterShopItem(RobotItem, null, null, node, 5);
        }



    }
}
