using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RobotMod.Patches;
using RobotMod.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            harmony.PatchAll(typeof(RobotModBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(RobotController));


        }



    }
}
