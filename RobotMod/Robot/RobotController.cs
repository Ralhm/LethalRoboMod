using GameNetcodeStuff;
using HarmonyLib;
using System;
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


//Hurdles:
//The terminals list of buyable items is an array of Items. An Array. Not a list. This could make adding the robot to it difficult
//In order to make the robot purchasable by the terminal, we'll need to give it an Item variable. This could complicate things (maybe. Need to look into it more)
//Networking. IDK how it works on unity and we may have to figure shit out for how Lethal Company makes it work. 
//We could probably reverse engineer what's there and hopefuly copy/paste what's there into our thing.


namespace RobotMod.Robot
{
    internal class RobotController : PlayerControllerB
    {

        public PlayerControllerB targetPlayer;

        public bool movingTowardsTargetPlayer;

        public bool moveTowardsDestination = true;

        public NavMeshAgent agent;

        public Vector3 destination;

        public float updatePositionThreshold = 1f;

        [HideInInspector]
        public Vector3 serverPosition;

        [HideInInspector]
        public Vector3 serverRotation;

        private float updateDestinationInterval;

        public Item item;

        void Awake()
        {
            item = new Item();
        }

        enum CommandType
        {
            Follow, ReturnToShip, Attack, FindScrap
        }

        public void GetHoldItem()
        {

        }

        public void ReceiveCommand()
        {

        }

        public void AttackBehavior()
        {

        }

        public void FindScrap()
        {

        }

        public virtual void DoAIInterval()
        {
            if (moveTowardsDestination)
            {
                agent.SetDestination(destination);
            }
            SyncPositionToClients();
        }

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
    }
}
