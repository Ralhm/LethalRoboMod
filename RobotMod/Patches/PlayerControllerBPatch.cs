using GameNetcodeStuff;
using HarmonyLib;
using RobotMod.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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


        public void GiveRobotCommand()
        {
            //Do a ray trace in front of the player, just like when calling BeginGrabObject
            //


            RaycastHit hit;
            /*
            Ray interactRay = new Ray(gameplayCamera.transform.position, gameplayCamera.transform.forward);
            if (!Physics.Raycast(interactRay, out hit, grabDistance, interactableObjectsMask) || hit.collider.gameObject.layer == 8 || !(hit.collider.tag == "PhysicsProp") || twoHanded || sinkingValue > 0.73f)
            {

                hit.collider.gameObject.GetComponent<RobotController>().ReceiveCommand(RobotController.CommandType.FindScrap);

                return;
            }
            
            */


        }


    }
}
