using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections;

namespace RobotMod.Robot
{
    public class RobotAI : NetworkBehaviour
    {
        public bool DebugMode;

        // Start Network Stuff 
        public float syncMovementSpeed = 0.22f;

        [HideInInspector]
        public Vector3 serverPosition;

        [HideInInspector]
        public Vector3 serverRotation;

        private float previousYRotation;

        private float targetYRotation;

        [HideInInspector]
        public NetworkObject thisNetworkObject;

        public float updatePositionThreshold = 1f;

        public bool isClientCalculatingAI;
        // End Network Stuff

        // Start AI Variables
        public enum CommandType
        {
            Follow, ReturnToShip, Attack, FindScrap, Idle
        }

        public CommandType CurrentState;

        float CheckRadius = 40.0f;

        private Collider[] NearScrapColliders;

        public bool debugAI;

        public NavMeshAgent agent;

        [HideInInspector]
        public NavMeshPath path1;

        public GameObject[] allAINodes;

        public Transform targetNode;

        public Transform favoriteSpot;

        [HideInInspector]
        public float mostOptimalDistance;

        [HideInInspector]
        public float pathDistance;

        public bool moveTowardsDestination = true;

        public Vector3 destination;

        private float updateDestinationInterval;

        // Start Path Finding Info
        public AISearchRoutine currentSearch;

        public Coroutine searchCoroutine;

        public Coroutine chooseTargetNodeCoroutine;

        private System.Random searchRoutineRandom;
        // End Path Finding Info

        public GrabbableObject targetObject;

        public bool movingTowardsTarget;

        [HideInInspector]
        public float tempDist;

        private Vector3 tempVelocity;
        // End AI Variables

        // Both AI and Netcode
        [Header("AI Calculation / Netcode")]
        public float AIIntervalTime = 0.2f;

        public bool isOutside;

        private Vector3 mainEntrancePosition;

        public virtual void Start()
        {
            try
            {
                agent = base.gameObject.GetComponentInChildren<NavMeshAgent>();
                thisNetworkObject = base.gameObject.GetComponentInChildren<NetworkObject>();
                serverPosition = base.transform.position;
                isOutside = base.transform.position.y > -80f;
                mainEntrancePosition = RoundManager.FindMainEntrancePosition(getTeleportPosition: true, isOutside);
                if (isOutside)
                {
                    if (allAINodes == null || allAINodes.Length == 0)
                    {
                        allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                    }
                }
                else if (allAINodes == null || allAINodes.Length == 0)
                {
                    allAINodes = GameObject.FindGameObjectsWithTag("AINode");
                }
                path1 = new NavMeshPath();
                if (base.IsOwner)
                {
                    SyncPositionToClients();
                }
                else
                {
                    SetClientCalculatingAI(enable: false);
                }
            }
            catch (Exception arg)
            {
                Debug.LogError($"Error when initializing enemy variables for {base.gameObject.name} : {arg}");
            }
        }

        public virtual void Update()
        {
            if (!base.IsOwner)
            {
                if (currentSearch.inProgress)
                {
                    StopSearch(currentSearch);
                }
                SetClientCalculatingAI(enable: false);

                base.transform.position = Vector3.SmoothDamp(base.transform.position, serverPosition, ref tempVelocity, syncMovementSpeed);
                base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, Mathf.LerpAngle(base.transform.eulerAngles.y, targetYRotation, 15f * Time.deltaTime), base.transform.eulerAngles.z);

                return;
            }
            if (updateDestinationInterval >= 0f)
            {
                updateDestinationInterval -= Time.deltaTime;
            }
            else
            {
                DoAIInterval();
                updateDestinationInterval = AIIntervalTime;
            }
            if (Mathf.Abs(previousYRotation - base.transform.eulerAngles.y) > 6f)
            {
                previousYRotation = base.transform.eulerAngles.y;
                targetYRotation = previousYRotation;
                if (base.IsServer)
                {
                    UpdateEnemyRotationClientRpc((short)previousYRotation);
                }
                else
                {
                    UpdateEnemyRotationServerRpc((short)previousYRotation);
                }
            }
        }

        private void OnEnable()
        {
            agent.enabled = true;
            CurrentState = CommandType.Follow;
        }

        private void OnDisable()
        {
            agent.enabled = false;
        }

        // Start AI Methods

        public virtual void DoAIInterval()
        {
            switch (CurrentState)
            {
                case CommandType.Idle:

                    break;
                case CommandType.Follow:

                    break;
                case CommandType.ReturnToShip:

                    break;

                case CommandType.Attack:

                    break;

                case CommandType.FindScrap:

                    break;
            }


            if (moveTowardsDestination)
            {
                agent.SetDestination(destination);
            }
            SyncPositionToClients();
        }

        public void ReceiveCommand(CommandType command)
        {
            switch (command)
            {
                case CommandType.Idle:

                    break;
                case CommandType.Follow:

                    break;
                case CommandType.ReturnToShip:

                    break;

                case CommandType.Attack:

                    break;

                case CommandType.FindScrap:
                    FindScrap();
                    break;
            }
        }

        public void FindScrap()
        {
            if (Physics.OverlapSphereNonAlloc(base.transform.position, CheckRadius, NearScrapColliders) > 0)
            {



                CurrentState = CommandType.FindScrap;
            }
            else
            {
                CurrentState = CommandType.Idle;
            }
        }

        public void StartSearch(Vector3 startOfSearch, AISearchRoutine newSearch = null)
        {
            StopSearch(currentSearch);
            movingTowardsTarget = false;
            if (newSearch == null)
            {
                currentSearch = new AISearchRoutine();
                newSearch = currentSearch;
            }
            else
            {
                currentSearch = newSearch;
            }
            currentSearch.currentSearchStartPosition = startOfSearch;
            if (currentSearch.unsearchedNodes.Count <= 0)
            {
                currentSearch.unsearchedNodes = allAINodes.ToList();
            }
            searchRoutineRandom = new System.Random(RoundUpToNearestFive(startOfSearch.x) + RoundUpToNearestFive(startOfSearch.z));
            searchCoroutine = StartCoroutine(CurrentSearchCoroutine());
            currentSearch.inProgress = true;
        }

        private int RoundUpToNearestFive(float x)
        {
            return (int)(x / 5f) * 5;
        }

        public void StopSearch(AISearchRoutine search, bool clear = true)
        {
            if (search != null)
            {
                if (searchCoroutine != null)
                {
                    StopCoroutine(searchCoroutine);
                }
                if (chooseTargetNodeCoroutine != null)
                {
                    StopCoroutine(chooseTargetNodeCoroutine);
                }
                search.calculatingNodeInSearch = false;
                search.inProgress = false;
                if (clear)
                {
                    search.unsearchedNodes = allAINodes.ToList();
                    search.timesFinishingSearch = 0;
                    search.nodesEliminatedInCurrentSearch = 0;
                    search.currentTargetNode = null;
                    search.currentSearchStartPosition = Vector3.zero;
                    search.nextTargetNode = null;
                    search.choseTargetNode = false;
                }
            }
        }

        private IEnumerator CurrentSearchCoroutine()
        {
            yield return null;
            while (searchCoroutine != null && base.IsOwner)
            {
                yield return null;
                if (currentSearch.unsearchedNodes.Count <= 0)
                {
                    FinishedCurrentSearchRoutine();
                    if (!currentSearch.loopSearch)
                    {
                        currentSearch.inProgress = false;
                        searchCoroutine = null;
                        yield break;
                    }
                    currentSearch.unsearchedNodes = allAINodes.ToList();
                    currentSearch.timesFinishingSearch++;
                    currentSearch.nodesEliminatedInCurrentSearch = 0;
                    yield return new WaitForSeconds(1f);
                }
                if (currentSearch.choseTargetNode && currentSearch.unsearchedNodes.Contains(currentSearch.nextTargetNode))
                {
                    if (debugAI)
                    {
                        Debug.Log($"finding next node: {currentSearch.choseTargetNode}; node already found ahead of time");
                    }
                    currentSearch.currentTargetNode = currentSearch.nextTargetNode;
                }
                else
                {
                    if (debugAI)
                    {
                        Debug.Log("finding next node; calculation not finished ahead of time");
                    }
                    currentSearch.waitingForTargetNode = true;
                    StartCalculatingNextTargetNode();
                    yield return new WaitUntil(() => currentSearch.choseTargetNode);
                }
                currentSearch.waitingForTargetNode = false;
                if (currentSearch.unsearchedNodes.Count <= 0 || currentSearch.currentTargetNode == null)
                {
                    continue;
                }
                if (debugAI)
                {
                    int num = 0;
                    for (int j = 0; j < currentSearch.unsearchedNodes.Count; j++)
                    {
                        if (currentSearch.unsearchedNodes[j] == currentSearch.currentTargetNode)
                        {
                            Debug.Log($"Found node {currentSearch.unsearchedNodes[j]} within list of unsearched nodes at index {j}");
                            num++;
                        }
                    }
                    Debug.Log($"Copies of the node {currentSearch.currentTargetNode} found in list: {num}");
                    Debug.Log($"unsearched nodes contains {currentSearch.currentTargetNode}? : {currentSearch.unsearchedNodes.Contains(currentSearch.currentTargetNode)}");
                    Debug.Log($"Removing {currentSearch.currentTargetNode} from unsearched nodes list with Remove()");
                }
                currentSearch.unsearchedNodes.Remove(currentSearch.currentTargetNode);
                if (debugAI)
                {
                    Debug.Log($"Removed. Does list now contain {currentSearch.currentTargetNode}?: {currentSearch.unsearchedNodes.Contains(currentSearch.currentTargetNode)}");
                }
                SetDestinationToPosition(currentSearch.currentTargetNode.transform.position);
                for (int i = currentSearch.unsearchedNodes.Count - 1; i >= 0; i--)
                {
                    if (Vector3.Distance(currentSearch.currentTargetNode.transform.position, currentSearch.unsearchedNodes[i].transform.position) < currentSearch.searchPrecision)
                    {
                        EliminateNodeFromSearch(i);
                    }
                    if (i % 10 == 0)
                    {
                        yield return null;
                    }
                }
                StartCalculatingNextTargetNode();
                int timeSpent = 0;
                while (searchCoroutine != null)
                {
                    if (debugAI)
                    {
                        Debug.Log("Current search not null");
                    }
                    timeSpent++;
                    if (timeSpent >= 32)
                    {
                        break;
                    }
                    yield return new WaitForSeconds(0.5f);
                    if (Vector3.Distance(base.transform.position, currentSearch.currentTargetNode.transform.position) < currentSearch.searchPrecision)
                    {
                        if (debugAI)
                        {
                            Debug.Log("Enemy: Reached the target " + currentSearch.currentTargetNode.name);
                        }
                        ReachedNodeInSearch();
                        break;
                    }
                    if (debugAI)
                    {
                        Debug.Log($"Enemy: We have not reached the target node {currentSearch.currentTargetNode.transform.name}, distance: {Vector3.Distance(base.transform.position, currentSearch.currentTargetNode.transform.position)} ; {currentSearch.searchPrecision}");
                    }
                }
                if (debugAI)
                {
                    Debug.Log("Reached destination node");
                }
            }
            if (!base.IsOwner)
            {
                StopSearch(currentSearch);
            }
        }

        private void StartCalculatingNextTargetNode()
        {
            if (debugAI)
            {
                Debug.Log("Calculating next target node");
                Debug.Log($"Is calculate node coroutine null? : {chooseTargetNodeCoroutine == null}; choseTargetNode: {currentSearch.choseTargetNode}");
            }
            if (chooseTargetNodeCoroutine == null)
            {
                if (debugAI)
                {
                    Debug.Log("NODE A");
                }
                currentSearch.choseTargetNode = false;
                chooseTargetNodeCoroutine = StartCoroutine(ChooseNextNodeInSearchRoutine());
            }
            else if (!currentSearch.calculatingNodeInSearch)
            {
                if (debugAI)
                {
                    Debug.Log("NODE B");
                }
                currentSearch.choseTargetNode = false;
                currentSearch.calculatingNodeInSearch = true;
                StopCoroutine(chooseTargetNodeCoroutine);
                chooseTargetNodeCoroutine = StartCoroutine(ChooseNextNodeInSearchRoutine());
            }
        }

        private IEnumerator ChooseNextNodeInSearchRoutine()
        {
            yield return null;
            float closestDist = 500f;
            bool gotNode = false;
            GameObject chosenNode = null;
            for (int j = 0; j < currentSearch.unsearchedNodes.Count; j++)
            {
            }
            for (int i = currentSearch.unsearchedNodes.Count - 1; i >= 0; i--)
            {
                if (!base.IsOwner)
                {
                    currentSearch.calculatingNodeInSearch = false;
                    yield break;
                }
                if (i % 5 == 0)
                {
                    yield return null;
                }
                if (Vector3.Distance(currentSearch.currentSearchStartPosition, currentSearch.unsearchedNodes[i].transform.position) > currentSearch.searchWidth)
                {
                    EliminateNodeFromSearch(i);
                }
                else if (PathIsIntersectedByLineOfSight(currentSearch.unsearchedNodes[i].transform.position, calculatePathDistance: true, avoidLineOfSight: false))
                {
                    EliminateNodeFromSearch(i);
                }
                else if (pathDistance < closestDist && (!currentSearch.randomized || !gotNode || searchRoutineRandom.Next(0, 100) < 65))
                {
                    closestDist = pathDistance;
                    chosenNode = currentSearch.unsearchedNodes[i];
                    gotNode = true;
                }
            }
            if (debugAI)
            {
                Debug.Log($"NODE C; chosen node: {chosenNode}");
            }
            if (currentSearch.waitingForTargetNode)
            {
                currentSearch.currentTargetNode = chosenNode;
                if (debugAI)
                {
                    Debug.Log("NODE C1");
                }
            }
            else
            {
                currentSearch.nextTargetNode = chosenNode;
                if (debugAI)
                {
                    Debug.Log("NODE C2");
                }
            }
            currentSearch.choseTargetNode = true;
            if (debugAI)
            {
                Debug.Log($"Chose target node?: {currentSearch.choseTargetNode} ");
            }
            currentSearch.calculatingNodeInSearch = false;
            chooseTargetNodeCoroutine = null;
        }

        public virtual void ReachedNodeInSearch()
        {
        }

        private void EliminateNodeFromSearch(GameObject node)
        {
            currentSearch.unsearchedNodes.Remove(node);
            currentSearch.nodesEliminatedInCurrentSearch++;
        }

        private void EliminateNodeFromSearch(int index)
        {
            currentSearch.unsearchedNodes.RemoveAt(index);
            currentSearch.nodesEliminatedInCurrentSearch++;
        }

        public virtual void FinishedCurrentSearchRoutine()
        {
        }

        public bool PathIsIntersectedByLineOfSight(Vector3 targetPos, bool calculatePathDistance = false, bool avoidLineOfSight = true)
        {
            pathDistance = 0f;
            if (!agent.CalculatePath(targetPos, path1))
            {
                return true;
            }
            if (DebugMode)
            {
                for (int i = 1; i < path1.corners.Length; i++)
                {
                    Debug.DrawLine(path1.corners[i - 1], path1.corners[i], Color.red);
                }
            }
            if (Vector3.Distance(path1.corners[path1.corners.Length - 1], RoundManager.Instance.GetNavMeshPosition(targetPos, RoundManager.Instance.navHit, 2.7f)) > 1.5f)
            {
                return true;
            }
            if (calculatePathDistance)
            {
                for (int j = 1; j < path1.corners.Length; j++)
                {
                    pathDistance += Vector3.Distance(path1.corners[j - 1], path1.corners[j]);
                    if (avoidLineOfSight && Physics.Linecast(path1.corners[j - 1], path1.corners[j], 262144))
                    {
                        return true;
                    }
                }
            }
            else if (avoidLineOfSight)
            {
                for (int k = 1; k < path1.corners.Length; k++)
                {
                    Debug.DrawLine(path1.corners[k - 1], path1.corners[k], Color.green);
                    if (Physics.Linecast(path1.corners[k - 1], path1.corners[k], 262144))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool SetDestinationToPosition(Vector3 position, bool checkForPath = false)
        {
            if (checkForPath)
            {
                position = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 1.75f);
                path1 = new NavMeshPath();
                if (!agent.CalculatePath(position, path1))
                {
                    return false;
                }
                if (Vector3.Distance(path1.corners[path1.corners.Length - 1], RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 2.7f)) > 1.55f)
                {
                    return false;
                }
            }
            moveTowardsDestination = true;
            movingTowardsTarget = false;
            destination = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, -1f);
            return true;
        }
        // End AI Methods

        // Start Networking Methods
        public void SyncPositionToClients()
        {
            if (Vector3.Distance(serverPosition, base.transform.position) > updatePositionThreshold)
            {
                serverPosition = base.transform.position;
                if (base.IsServer)
                {
                    UpdateEnemyPositionClientRpc(serverPosition);
                }
                else
                {
                    UpdateEnemyPositionServerRpc(serverPosition);
                }
            }
        }

        [ServerRpc]
        private void UpdateEnemyPositionServerRpc(Vector3 newPos)
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
                UpdateEnemyPositionClientRpc(newPos);
            }
        }

        [ClientRpc]
        private void UpdateEnemyPositionClientRpc(Vector3 newPos)
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

        [ServerRpc]
        private void UpdateEnemyRotationServerRpc(short rotationY)
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
                FastBufferWriter bufferWriter = __beginSendServerRpc(3079913705u, serverRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValueBitPacked(bufferWriter, rotationY);
                __endSendServerRpc(ref bufferWriter, 3079913705u, serverRpcParams, RpcDelivery.Reliable);
            }
            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
            {
                UpdateEnemyRotationClientRpc(rotationY);
            }
        }

        [ClientRpc]
        private void UpdateEnemyRotationClientRpc(short rotationY)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default(ClientRpcParams);
                    FastBufferWriter bufferWriter = __beginSendClientRpc(1258118513u, clientRpcParams, RpcDelivery.Reliable);
                    BytePacker.WriteValueBitPacked(bufferWriter, rotationY);
                    __endSendClientRpc(ref bufferWriter, 1258118513u, clientRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    previousYRotation = base.transform.eulerAngles.y;
                    targetYRotation = rotationY;
                }
            }
        }

        public void SetClientCalculatingAI(bool enable)
        {
            isClientCalculatingAI = enable;
            agent.enabled = enable;
        }
        // End Networking Methods
    }
}
