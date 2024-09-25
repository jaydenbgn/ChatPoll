using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace ChatPoll.Patches
{
    [HarmonyPatch]
    internal static class PollPatch
    {
        private static PollManager pollManager;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        private static void InitializePollManagerPatch(StartOfRound __instance)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            if (pollManager != null)
            {
                Plugin.Logger.LogError($"Tried to create a poll manager but one already exists");
                return;
            }
            pollManager = new GameObject("PollManager").AddComponent<PollManager>();
            pollManager.gameObject.AddComponent<PollCreatorUI>();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "__rpc_handler_2930587515")]
        private static bool ProcessIncomingChatMessagePatch(ref NetworkBehaviour target, ref FastBufferReader reader, ref __RpcParams rpcParams)
        {
            if (!pollManager.IsPollActive)
                return true;

            reader.ReadValueSafe(out bool value, default);
            if (!value)
                return true;
            reader.ReadValueSafe(out string message, oneByteChars: false);
            reader.Seek(0);

            if (!int.TryParse(message, out int votedOption))
                return true;

            pollManager.AddVote(rpcParams.Server.Receive.SenderClientId, votedOption - 1);

            return false;
        }

    }
}
