using HarmonyLib;
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
                Plugin.PrintToChatLocally("<color=red>You cannot open the poll menu; you are not the host</color>");
                return false;
            }
            if (PollManager.Instance == null)
            {
                Plugin.PrintToChatLocally("<color=red>You cannot open the poll menu; the Poll Manager was not created. This message should be impossible to see</color>");
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

            reader.ReadValueSafe(out bool value, default);
            if (!value)
                return true;
            reader.ReadValueSafe(out string message, oneByteChars: false);
            reader.Seek(0);

            if (!int.TryParse(message, out int votedOption))
                return true;

            PollManager.Instance.AddVote(rpcParams.Server.Receive.SenderClientId, votedOption - 1);

            return false;
        }

    }
}
