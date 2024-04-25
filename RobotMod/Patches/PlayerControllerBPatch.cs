using GameNetcodeStuff;
using HarmonyLib;
using RobotMod.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


namespace RobotMod.Patches
{


    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {

        [HarmonyPatch("Update")]
        [HarmonyPostfix]

        static void patchUpdate(ref float ___sprintMeter)
        {
            //___sprintMeter = 1.0f;
            //Debug.Log("-------USING DLL PLAYER SCRIPT In Patch!!!!!!!!!!!!!!");
            //if (Input.KeyCode)


        }


        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void patchAwake()
        {
            On.GameNetcodeStuff.PlayerControllerB.Update += PlayerControllerB_Update;
        }


        private static void PlayerControllerB_Update(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
        {
            // Code here runs before the original method
            orig(self); // Call the original method with its arguments
                        // Code here runs after the original method

            bool InputAlready = false;
            if (Keyboard.current[Key.T].wasPressedThisFrame && !InputAlready)
            {
                InputAlready = true;
                GiveRobotCommandIdle(orig, self);
            }

            if (Keyboard.current[Key.Y].wasPressedThisFrame && !InputAlready)
            {

                GiveRobotCommandFollow(orig, self);
            }

            if (Keyboard.current[Key.Y].wasReleasedThisFrame)
            {

                InputAlready = false;
            }

            if (Keyboard.current[Key.T].wasReleasedThisFrame)
            {

                InputAlready = false;
            }
        }

        [Harmony]
        public static void GiveRobotCommandIdle(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
        {
            RaycastHit hit;
            Ray NewInteractRay = new Ray(self.gameplayCamera.transform.position, self.gameplayCamera.transform.forward);
            Debug.Log("-----ATTEMPTING TO GIVE A COMMAND--------");

            if (Physics.Raycast(NewInteractRay, out hit, self.grabDistance, 832))
            {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.layer == 6)
                {
                    Debug.Log("-----TRACE HIT THE ROBOT!!!!-------");

                    RobotAI ai = hit.collider.gameObject.GetComponent<RobotAI>();
                    if (ai != null)
                    {
                        ai.ReceiveCommand(RobotAI.CommandType.Idle);
                    }
                }


                return;
            }
        }
        [Harmony]
        public static void GiveRobotCommandFollow(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
        {


            RaycastHit hit;
            Ray NewInteractRay = new Ray(self.gameplayCamera.transform.position, self.gameplayCamera.transform.forward);
            Debug.Log("-----ATTEMPTING TO GIVE A COMMAND--------");

            if (Physics.Raycast(NewInteractRay, out hit, self.grabDistance, 832))
            {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.layer == 6)
                {
                    Debug.Log("-----TRACE HIT THE ROBOT!!!!-------");
                    RobotAI ai = hit.collider.gameObject.GetComponent<RobotAI>();
                    if (ai != null)
                    {
                        ai.ReceiveCommand(RobotAI.CommandType.Follow);
                    }
                    
                }


                return;
            }
        }
    }


}


