using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace RobotMod.Patches
{

    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        /*
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private TerminalNode ParsePlayerSentence()
        {
            broadcastedCodeThisFrame = false;
            string s = screenText.text.Substring(screenText.text.Length - textAdded);
            s = RemovePunctuation(s);
            string[] array = s.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            TerminalKeyword terminalKeyword = null;
            if (currentNode != null && currentNode.overrideOptions)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    TerminalNode terminalNode = ParseWordOverrideOptions(array[i], currentNode.terminalOptions);
                    if (terminalNode != null)
                    {
                        return terminalNode;
                    }
                }
                return null;
            }
            if (array.Length > 1)
            {
                switch (array[0])
                {
                    case "switch":
                        {
                            int num = CheckForPlayerNameCommand(array[0], array[1]);
                            if (num != -1)
                            {
                                StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync(num);
                                return terminalNodes.specialNodes[20];
                            }
                            break;
                        }
                    case "flash":
                        {
                            int num = CheckForPlayerNameCommand(array[0], array[1]);
                            if (num != -1)
                            {
                                StartOfRound.Instance.mapScreen.FlashRadarBooster(num);
                                return terminalNodes.specialNodes[23];
                            }
                            if (StartOfRound.Instance.mapScreen.radarTargets[StartOfRound.Instance.mapScreen.targetTransformIndex].isNonPlayer)
                            {
                                StartOfRound.Instance.mapScreen.FlashRadarBooster(StartOfRound.Instance.mapScreen.targetTransformIndex);
                                return terminalNodes.specialNodes[23];
                            }
                            break;
                        }
                    case "ping":
                        {
                            int num = CheckForPlayerNameCommand(array[0], array[1]);
                            if (num != -1)
                            {
                                StartOfRound.Instance.mapScreen.PingRadarBooster(num);
                                return terminalNodes.specialNodes[21];
                            }
                            break;
                        }
                    case "transmit":
                        {
                            SignalTranslator signalTranslator = UnityEngine.Object.FindObjectOfType<SignalTranslator>();
                            if (!(signalTranslator != null) || !(Time.realtimeSinceStartup - signalTranslator.timeLastUsingSignalTranslator > 8f) || array.Length < 2)
                            {
                                break;
                            }
                            string text = s.Substring(8);
                            if (!string.IsNullOrEmpty(text))
                            {
                                if (!base.IsServer)
                                {
                                    signalTranslator.timeLastUsingSignalTranslator = Time.realtimeSinceStartup;
                                }
                                HUDManager.Instance.UseSignalTranslatorServerRpc(text.Substring(0, Mathf.Min(text.Length, 10)));
                                return terminalNodes.specialNodes[22];
                            }
                            break;
                        }
                }
            }
            terminalKeyword = CheckForExactSentences(s);
            if (terminalKeyword != null)
            {
                if (terminalKeyword.accessTerminalObjects)
                {
                    CallFunctionInAccessibleTerminalObject(terminalKeyword.word);
                    PlayBroadcastCodeEffect();
                    return null;
                }
                if (terminalKeyword.specialKeywordResult != null)
                {
                    return terminalKeyword.specialKeywordResult;
                }
            }
            string value = Regex.Match(s, "\\d+").Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                playerDefinedAmount = Mathf.Clamp(int.Parse(value), 0, 10);
            }
            else
            {
                playerDefinedAmount = 1;
            }
            if (array.Length > 5)
            {
                return null;
            }
            TerminalKeyword terminalKeyword2 = null;
            TerminalKeyword terminalKeyword3 = null;
            new List<TerminalKeyword>();
            bool flag = false;
            hasGottenNoun = false;
            hasGottenVerb = false;
            for (int j = 0; j < array.Length; j++)
            {
                terminalKeyword = ParseWord(array[j]);
                if (terminalKeyword != null)
                {
                    Debug.Log("Parsed word: " + array[j]);
                    if (terminalKeyword.isVerb)
                    {
                        if (hasGottenVerb)
                        {
                            continue;
                        }
                        hasGottenVerb = true;
                        terminalKeyword2 = terminalKeyword;
                    }
                    else
                    {
                        if (hasGottenNoun)
                        {
                            continue;
                        }
                        hasGottenNoun = true;
                        terminalKeyword3 = terminalKeyword;
                        if (terminalKeyword.accessTerminalObjects)
                        {
                            broadcastedCodeThisFrame = true;
                            CallFunctionInAccessibleTerminalObject(terminalKeyword.word);
                            flag = true;
                        }
                    }
                    if (!flag && hasGottenNoun && hasGottenVerb)
                    {
                        break;
                    }
                }
                else
                {
                    Debug.Log("Could not parse word: " + array[j]);
                }
            }
            if (broadcastedCodeThisFrame)
            {
                PlayBroadcastCodeEffect();
                return terminalNodes.specialNodes[19];
            }
            hasGottenNoun = false;
            hasGottenVerb = false;
            if (terminalKeyword3 == null)
            {
                return terminalNodes.specialNodes[10];
            }
            if (terminalKeyword2 == null)
            {
                if (!(terminalKeyword3.defaultVerb != null))
                {
                    return terminalNodes.specialNodes[11];
                }
                terminalKeyword2 = terminalKeyword3.defaultVerb;
            }
            for (int k = 0; k < terminalKeyword2.compatibleNouns.Length; k++)
            {
                if (terminalKeyword2.compatibleNouns[k].noun == terminalKeyword3)
                {
                    Debug.Log($"noun keyword: {terminalKeyword3.word} ; verb keyword: {terminalKeyword2.word} ; result null? : {terminalKeyword2.compatibleNouns[k].result == null}");
                    Debug.Log("result: " + terminalKeyword2.compatibleNouns[k].result.name);
                    return terminalKeyword2.compatibleNouns[k].result;
                }
            }
            return terminalNodes.specialNodes[12];
        }
        */

    }
}
