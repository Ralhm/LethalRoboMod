using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

//NOTES
//All enemies use a DoAIInterval function, including the MaskedPlayerEnemy. We can observe and use a similar function, probably
//Players should just hand an item to the robot
//Players should be able to give commands either from the console or in person by just pressing a button or something while looking at the robot
//Attack function should look for enemies within a certain radius and proceed to pursue/attack if they're holding a weapon.
//If it finds no enemies within range or is not holding a weapon on the initial attack command, do nothing
//FindScrap should work similarly to Attack but instead just walk towards nearest scrap


//Notable functions/variables:
//ForceTurnTowardsTarget()
//The Item class has a variable IsScrap. We can use this to detect scrap with the robots FindScrap function
//The Item class is a scriptable object. basically all grabbable items like scrap and flashlights have a Public Item variable
//We'll probably need to make a patch for the terminal to add the robot as a purchasable item
//RoundManager.FindMainEntrance should help to find the entrance
//TeleportMaskedEnemy will also make for a good reference
//GrabItem in HoarderBugAI


//Useful PlayerController Functions
//SetDestinationToPosition and SetMovingTowardsTargetPlayerin EnemyAI should be useful references
//SetDestinationToPosition just calls the current target players position
//DiscardHeldObject
//isHoldingObjecy
//BeginGrabObject (In order for this to work, we'll need to make the robot look at the object to be grabbed, or refactor the function)
//I'm thinking we either just hand objects to it, or tell it to grab the nearest piece of scrap
//PlaceGrabbableObject
//Grabbable Object has isInShipRoom variable


//Networkign notes
//UpdatePlayerPositionServerRpc UpdatePlayerPositionClientRpc in PlayerControllerB will be usful probably


//Hurdles:
//The terminals list of buyable items is an array of Items. An Array. Not a list. This could make adding the robot to it difficult
//In order to make the robot purchasable by the terminal, we'll need to give it an Item variable. This could complicate things (maybe. Need to look into it more)
//Networking. IDK how it works on unity and we may have to figure shit out for how Lethal Company makes it work. 
//We could probably reverse engineer what's there and hopefuly copy/paste what's there into our thing.

namespace RobotMod.Robot
{
    //[RequireComponent(typeof(RobotAI))]
    public class RobotItem : GrabbableObject
    {

        // Real Robot shit
        [SerializeField] RobotAI robotAIPrefab;

        //private RoundManager roundManager;

        // Radar Variables
        public string robotName = "";

        [HideInInspector] public bool setRandomRobotName = false;
        [HideInInspector] public int robotNameIndex = -1;


        void Awake()
        {
            Debug.Log("----------USING DLL-----------");
            //CommandType type = CommandType.Follow;
        }

        public override void Start()
        {
            base.Start();
            Debug.Log("----------USING DLL-----------");
            //roundManager = UnityEngine.Object.FindObjectOfType<RoundManager>();

            if (!setRandomRobotName)
            {
                //setRandomRobotName = true;
                //int num = (robotNameIndex = ((robotNameIndex != -1) ? robotNameIndex : new System.Random(Mathf.Min(StartOfRound.Instance.randomMapSeed + (int)base.NetworkObjectId, 99999999)).Next(0, StartOfRound.Instance.randomNames.Length)));
                //robotName = StartOfRound.Instance.randomNames[num];
            }
        }

        void OnEnable()
        {
            Debug.Log("----------USING DLL-----------");
            Debug.Log("----------Updated DLL-----------");
        }

        public override int GetItemDataToSave()
        {
            base.GetItemDataToSave();
            return robotNameIndex;
        }

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            robotNameIndex = saveData;
            robotName = StartOfRound.Instance.randomNames[robotNameIndex];
        }

        public override void GrabItem()
        {
            base.GrabItem();
            Debug.Log("USighn SPECILA GARB");
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            Debug.Log("USighn SPECILA Discard!");
        }

        public override void EquipItem()
        {
            base.EquipItem();
        }

        public override void PocketItem()
        {
            base.PocketItem();
            isBeingUsed = false;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (base.IsOwner)
		    {
			    playerHeldBy.DiscardHeldObject();
		    }
            Debug.Log("Activating Item: " + used);

            BecomeRobot();
        }

        public override void Update()
        {
            base.Update();
        }

        public void GetHoldItem()
        {

        }

        public void FollowPlayer()
        {

        }

        public void AttackBehavior()
        {

        }

        public void FindEnemies()
        {

        }

        public void BecomeRobot()
        {
            if(robotAIPrefab)
            {
                StartCoroutine(DelayBecomeBot());
            }
        }

        private IEnumerator DelayBecomeBot()
        {
            yield return new WaitForSeconds(1);

            if (!isHeld && !isHeldByEnemy)
            {

                RobotAI newRobot = Instantiate(robotAIPrefab, transform.position, transform.rotation);

                newRobot.robotName = robotName;
                newRobot.setRandomRobotName = setRandomRobotName;
                newRobot.robotNameIndex = robotNameIndex;

                Destroy(gameObject);
            }
        }

        protected override void __initializeVariables()
        {
            base.__initializeVariables();
        }

        /*
         * 
        public void SyncPositionToClients()
        {
            if (Vector3.Distance(serverPosition, base.transform.position) > updatePositionThreshold)
            {
                serverPosition = base.transform.position;
                if (base.IsServer)
                {
                    UpdateRobotPositionClientRpc(serverPosition);
                }
                else
                {
                    UpdateRobotPositionServerRpc(serverPosition);
                }
            }
        }

        [ServerRpc]
        private void UpdateRobotPositionServerRpc(Vector3 newPos)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
            {
                if (base.OwnerClientId != networkManager.LocalClientId)
                {
                    if (networkManager.LogLevel <= LogLevel.Normal)
                    {
                        Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
                    }
                    return;
                }
                ServerRpcParams serverRpcParams = default(ServerRpcParams);
                FastBufferWriter bufferWriter = __beginSendServerRpc(255411420u, serverRpcParams, RpcDelivery.Reliable);
                bufferWriter.WriteValueSafe(in newPos);
                __endSendServerRpc(ref bufferWriter, 255411420u, serverRpcParams, RpcDelivery.Reliable);
            }
            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
            {
                UpdateRobotPositionClientRpc(newPos);
            }
        }

        [ClientRpc]
        private void UpdateRobotPositionClientRpc(Vector3 newPos)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default(ClientRpcParams);
                    FastBufferWriter bufferWriter = __beginSendClientRpc(4287979896u, clientRpcParams, RpcDelivery.Reliable);
                    bufferWriter.WriteValueSafe(in newPos);
                    __endSendClientRpc(ref bufferWriter, 4287979896u, clientRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost) && !base.IsOwner)
                {
                    serverPosition = newPos;
                    OnSyncPositionFromServer(newPos);
                }
            }
        }

        public virtual void OnSyncPositionFromServer(Vector3 newPos)
        {
        }
        */
    }
}
