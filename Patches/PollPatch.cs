using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Numerics;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace ChatPoll.Patches
{
    [HarmonyPatch]
    internal static class PollPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        private static void InitializePollManagerPatch(StartOfRound __instance)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            if (PollManager.Instance != null)
            {
                Plugin.Logger.LogError($"Tried to create a poll manager but one already exists");
                return;
            }
            new GameObject("PollManager").AddComponent<PollManager>();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "AddTextToChatOnServer")]
        private static bool OpenPollPhrasePatch(ref string chatMessage)
        {
            // Allow the message to pass if it isn't the poll phrase
            if (chatMessage.ToLower() != Plugin.config.pollCreatorMenu_OpenPhrase.Value)
                return true;

            // Check if polls are allowed
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                Plugin.PrintToChatLocally("<color=red>You cannot open the Poll Creator menu; you are not the host.</color>");
                return false;
            }
            if (PollManager.Instance == null)
            {
                Plugin.PrintToChatLocally("<color=red>You cannot open the Poll Creator menu; the Poll Manager was not created. If you're seeing this message, something went terribly wrong.</color>");
                return false;
            }

            PollManager.Instance.PollCreatorUI.SetOpen(true);

            return false;
        }

        // This is a patch for the rpc handler of AddPlayerChatMessageServerRpc
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "__rpc_handler_2930587515")]
        private static bool ProcessIncomingChatMessagePatch(ref NetworkBehaviour target, ref FastBufferReader reader, ref __RpcParams rpcParams)
        {
            if (PollManager.Instance == null) return true;
            if (!PollManager.Instance.IsPollActive) return true;

            // Get player script
            ulong clientId = rpcParams.Server.Receive.SenderClientId;
            if (!StartOfRound.Instance.ClientPlayerList.TryGetValue(clientId, out int playerId))
                return true;
            if (playerId < 0 || playerId >= StartOfRound.Instance.allPlayerScripts.Length)
                return true;
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];

            // Read rpc values
            reader.ReadValueSafe(out bool value, default);
            if (!value)
                return true;
            reader.ReadValueSafe(out string message, oneByteChars: false);
            reader.Seek(0);
            
            // Parse vote number
            if (!int.TryParse(message, out int votedOption))
                return true;
            int votedOptionIndex = votedOption - 1;
            if (votedOptionIndex < 0 || votedOptionIndex >= PollManager.Instance.Options.Count)
                return true;
            if (PollManager.Instance.TryGetVote(clientId, out int? previousVotedOptionIndex)
                && votedOptionIndex == previousVotedOptionIndex)
                return false;

            // Accept vote
            PollManager.Instance.AddVote(rpcParams.Server.Receive.SenderClientId, votedOptionIndex);
            VoteMessage(player.playerUsername, previousVotedOptionIndex != null, PollManager.Instance.Options[votedOptionIndex]);

            return false;
        }

        private static void VoteMessage(string username, bool hasVotedAlready, string votedOption)
        {
            if (!Plugin.config.voteMessage_Visible.Value)
                return;

            StringBuilder stringBuilder = new($"{username} ");

            string action      = hasVotedAlready ? "changed their vote" : "voted";
            string preposition = hasVotedAlready ? "to"                 : "for";

            stringBuilder.Append(action);
            if (Plugin.config.voteMessage_ShowVotedOption.Value)
                stringBuilder.Append($" {preposition} '{votedOption}'");
            if (!hasVotedAlready && Plugin.config.voteMessage_ShowTotalVotes.Value)
                stringBuilder.Append($" ({PollManager.Instance.TotalVoteCount}/{StartOfRound.Instance.ClientPlayerList.Count})");
            stringBuilder.Append('.');

            Plugin.PrintToChatLocally(stringBuilder.ToString());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnClientDisconnect))]
        private static void RemoveVoteOnDisconnect(ref ulong clientId)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            PollManager.Instance.RemoveVote(clientId);
        }
    }
}
