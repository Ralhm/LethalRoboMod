using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using RobotMod.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


namespace RobotMod.Patches
{


    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {


        static bool AlreadyInput;

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

            


            if (UnityInput.Current.GetKeyDown(KeyCode.T) && !AlreadyInput)
            {
                AlreadyInput = true;
                GiveRobotCommandIdle(orig, self);
            }
            if (UnityInput.Current.GetKeyUp(KeyCode.T))
            {
                AlreadyInput = false;
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.Y) && !AlreadyInput)
            {
                AlreadyInput = true;
                GiveRobotCommandFollow(orig, self);
            }
            if (UnityInput.Current.GetKeyUp(KeyCode.Y))
            {

                AlreadyInput = false;
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.H) && !AlreadyInput)
            {
                AlreadyInput = true;
                GiveRobotItem(orig, self);
            }
            if (UnityInput.Current.GetKeyUp(KeyCode.H))
            {

                AlreadyInput = false;
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.J) && !AlreadyInput)
            {
                AlreadyInput = true;
                GiveRobotCommandDropItem(orig, self);
            }
            if (UnityInput.Current.GetKeyUp(KeyCode.J))
            {

                AlreadyInput = false;
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

        [Harmony]
        public static void GiveRobotItem(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
        {
            if (self.currentlyHeldObjectServer == null)
            {
                Debug.Log("-----CURRENTLY HELD OBJECT IS NULL--------");
                return;
            }

            RaycastHit hit;
            Ray NewInteractRay = new Ray(self.gameplayCamera.transform.position, self.gameplayCamera.transform.forward);
            Debug.Log("-----ATTEMPTING TO GIVE AN ITEM COMMAND--------");

            if (Physics.Raycast(NewInteractRay, out hit, self.grabDistance, 832))
            {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.layer == 6)
                {

                    RobotAI ai = hit.collider.gameObject.GetComponent<RobotAI>();
                    if (ai != null)
                    {

                        
                        Debug.Log("-----TRACE HIT THE ROBOT!!!!-------");
                        //ai.GrabItemIfClose();
                        for (int i = 0; i < self.ItemSlots.Length; i++)
                        {
                            if (self.ItemSlots[i] == self.currentlyHeldObjectServer)
                            {
                                self.ItemSlots[i] = null;
                            }
                        }
                        GrabbableObject ObjectRef = self.currentlyHeldObjectServer;
                        ObjectRef.heldByPlayerOnServer = false;
                        ObjectRef.parentObject = null;
                        self.currentlyHeldObjectServer = null;

                        self.playerBodyAnimator.SetBool("cancelHolding", value: true);
                        self.playerBodyAnimator.SetTrigger("Throw");
                        ObjectRef.isPocketed = false;
                        HUDManager.Instance.itemSlotIcons[self.currentItemSlot].enabled = false;
                        HUDManager.Instance.holdingTwoHandedItem.enabled = false;

                        //ai.ReceiveCommand(RobotAI.CommandType.FindScrap);
                        if (ai.HoldObject(ObjectRef.GetComponent<NetworkObject>()))
                        {
                            //self.DiscardHeldObject(ai);
                            Debug.Log("-----SUCCEEDED IN HOLDING THE OBJECT!!!!-------");
                        }
                        else
                        {
                            Debug.Log("-----FAILED TO HOLD THE OBJECT!!!!-------");
                        }
                        
                    }

                }


                return;
            }
        }

        [Harmony]
        public static void GiveRobotCommandDropItem(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
        {
            RaycastHit hit;
            Ray NewInteractRay = new Ray(self.gameplayCamera.transform.position, self.gameplayCamera.transform.forward);
            Debug.Log("-----ATTEMPTING TO GIVE A COMMAND--------");

            if (Physics.Raycast(NewInteractRay, out hit, self.grabDistance, 832))
            {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.layer == 6)
                {
                    RobotAI ai = hit.collider.gameObject.GetComponent<RobotAI>();
                    if (ai != null)
                    {
                        Debug.Log("-----TRACE HIT THE ROBOT!!!!-------");
                        ai.DropObject();
                    }

                }


                return;
            }
        }
    }


}


