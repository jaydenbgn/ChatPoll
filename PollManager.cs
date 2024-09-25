using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChatPoll
{
    internal class PollManager : MonoBehaviour
    {
        private const string PrimaryTextColorCode = "green";
        private const string SecondaryTextColorCode = "yellow";
        private const string TertiaryTextColorCode = "white";

        internal static PollManager Instance { get; private set; }

        internal bool IsPollActive { get; private set; }
        private float timer;
        private string[] options;
        private readonly Dictionary<ulong, int> votersOptions = new();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Plugin.inputActions.EndPoll.performed += EndPoll_performed;
        }

        private void OnDestroy()
        {
            Plugin.inputActions.EndPoll.performed -= EndPoll_performed;
        }

        private void EndPoll_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (InputActions.ShouldInputBeIgnored()) return;

            EndPoll();
        }

        private void Update()
        {
            if (!IsPollActive)
                return;

            timer -= Time.deltaTime;
            if (timer <= 0)
                EndPoll();
        }

        internal void StartPoll(string title, float duration, string[] options)
        {
            if (IsPollActive)
                return;

            IsPollActive = true;
            
            timer = duration;
            this.options = options;

            votersOptions.Clear();

            StringBuilder stringBuilder = new($"<color={PrimaryTextColorCode}>");
            {
                // Header
                stringBuilder.AppendLine("<align=flush>╔ POLL ╗</align>");
                if (!string.IsNullOrEmpty(title))
                    stringBuilder.AppendLine($"<align=justified><color={SecondaryTextColorCode}>\"{title}\"</color></align>");

                // Options
                stringBuilder.Append($"<color={TertiaryTextColorCode}>");
                for (int i = 0; i < options.Length; i++)
                {
                    stringBuilder.Append($"{i + 1}. {options[i]}");
                    if (i < options.Length - 1)
                        stringBuilder.AppendLine();
                }
                stringBuilder.AppendLine("</color>");

                // Instructions
                stringBuilder.AppendLine($"<align=justified><color={SecondaryTextColorCode}>^ Vote by sending the number!</color></align>");
                stringBuilder.Append("<align=flush>╚╝</align>");
            }
            stringBuilder.Append("</color>");
            Plugin.PrintToChat(stringBuilder.ToString());
        }

        internal void EndPoll()
        {
            if (!IsPollActive)
                return;

            IsPollActive = false;

            // This is atrocious
            Dictionary<int, int> talliedOptionVotes = new(options.Length);
            for (int i = 0; i < options.Length; i++)
            {
                talliedOptionVotes[i] = 0;
            }
            foreach (int optionIndex in votersOptions.Values)
            {
                talliedOptionVotes[optionIndex]++;
            }


            StringBuilder stringBuilder = new($"<color={PrimaryTextColorCode}>");
            {
                stringBuilder.AppendLine($"<align=flush>╔ RESULTS ╗</align>");
                if (votersOptions.Count > 0)
                {
                    stringBuilder.Append($"<color={TertiaryTextColorCode}>");
                    for (int i = 0; i < options.Length; i++)
                    {
                        float optionVotePercent = ((float)talliedOptionVotes[i] / votersOptions.Count) * 100;
                        stringBuilder.Append($"({optionVotePercent:n0}%) {options[i]}");
                        if (i < options.Length - 1)
                            stringBuilder.AppendLine();
                    }
                    stringBuilder.AppendLine("</color>");
                }   
                else
                {
                    stringBuilder.AppendLine($"<color={SecondaryTextColorCode}>No one voted...</color>");
                }
                stringBuilder.Append($"<align=flush>╚╝</align>");
            }
            stringBuilder.Append("</color>");
            Plugin.PrintToChat(stringBuilder.ToString());
        }

        internal void AddVote(ulong clientId, int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= options.Length)
                return;

            votersOptions[clientId] = optionIndex;
        }

    }
}
