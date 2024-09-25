using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace ChatPoll
{
    internal class InputActions : LcInputActions
    {
        [InputAction(KeyboardControl.P, Name = "Open Poll Creator")]
        internal InputAction OpenPollCreator { get; private set; }

        [InputAction(KeyboardControl.None, Name = "End Poll")]
        internal InputAction EndPoll { get; private set; }

        internal static bool ShouldInputBeIgnored()
        {
            if (StartOfRound.Instance == null) return true;
            if (StartOfRound.Instance.localPlayerController.isTypingChat) return true;
            if (StartOfRound.Instance.localPlayerController.inTerminalMenu) return true;
            if (StartOfRound.Instance.localPlayerController.quickMenuManager.isMenuOpen) return true;

            return false;
        }
    }
}
