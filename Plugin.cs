using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace ChatPoll
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess("Lethal Company.exe")]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "BGN.ChatPoll";
        public const string NAME = "Chat Poll";
        public const string VERSION = "1.1.0";

        internal static new ManualLogSource Logger;
        internal static ChatPollConfig config;
#nullable enable
        internal static readonly InputActions inputActions = new();

        private static readonly MethodInfo hudManagerAddChatMessageMethod = typeof(HUDManager).GetMethod("AddChatMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo hudManagerAddTextMessageServerRpcMethod = typeof(HUDManager).GetMethod("AddTextMessageServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            config = new ChatPollConfig(Config);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo($"Plugin {GUID} is loaded!");
        }

        internal static void PrintToChatLocally(string message)
        {
            hudManagerAddChatMessageMethod.Invoke(HUDManager.Instance, [message, ""]);
        }

        internal static void PrintToChat(string message)
        {
            hudManagerAddTextMessageServerRpcMethod.Invoke(HUDManager.Instance, [message]);
        }
    }
}