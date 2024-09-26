using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ChatPoll
{
    internal class PollCreatorUI : MonoBehaviour
    {
        private const string WindowTitle = "Poll Creator";
        private const int WindowWidth = 600;
        private const int WindowHeight = 350;
        private static readonly GUILayoutOption[] WindowOptions =
        [
            GUILayout.Width(WindowWidth),
            GUILayout.Height(WindowHeight),
        ];

        private bool isWindowOpen;
        private Vector2 windowPollOptionsScrollPosition;

        private string pollTitle = string.Empty;
        private bool hasDuration;
        private float duration = 40;
        private const float MinDuration = 10;
        private const float MaxDuration = 90;
        private readonly List<string> pollOptions = new();
        private const int MinimumPollOptions = 2;
        private const int MaximumPollOptions = 10;

        private void Start()
        {
            for (int i = 0; i < MinimumPollOptions; i++)
            {
                pollOptions.Add(string.Empty);
            }

            Plugin.inputActions.OpenPollCreator.performed += OpenPollCreator_performed;
        }

        private void OnDestroy()
        {
            Plugin.inputActions.OpenPollCreator.performed -= OpenPollCreator_performed;
        }

        private void OpenPollCreator_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (isWindowOpen) return;
            if (InputActions.ShouldInputBeIgnored()) return;

            SetOpen(true);
        }

        private void Update()
        {
            if (isWindowOpen && !StartOfRound.Instance.localPlayerController.quickMenuManager.isMenuOpen)
                SetOpen(false);
        }

        internal void SetOpen(bool isVisible)
        {
            isWindowOpen = isVisible;

            StartOfRound.Instance.localPlayerController.quickMenuManager.isMenuOpen = isVisible;
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isVisible;
        }

        private void OnGUI()
        {
            if (!isWindowOpen)
                return;

            float scale = Plugin.config.pollCreatorMenu_Scale.Value;
            if (Plugin.config.pollCreatorMenu_ScaleWithScreenSize.Value)
                scale *= Math.Min(Screen.width / 1920f, Screen.height / 1080f);
            int windowX = (Screen.width - WindowWidth) / 2;
            int windowY = (Screen.height - WindowHeight) / 2;
            float matrixOffsetX = ((Screen.width * scale) - Screen.width) / -2;
            float matrixOffsetY = ((Screen.height * scale) - Screen.height) / -2;

            GUI.matrix = Matrix4x4.TRS(new Vector2(matrixOffsetX, matrixOffsetY), Quaternion.identity, Vector3.one * scale);

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            GUILayout.Window(controlID, new Rect(windowX, windowY, WindowWidth, WindowHeight), DrawUI, WindowTitle, WindowOptions);
        }

        private void DrawUI(int id)
        {
            GUILayout.BeginHorizontal();

            // I'm using max width here to stop the UI from squishing each other.
            // This is probably not the correct way to do this but I've been struggling to get the UI to behave for a day now and this is the best I've got.
            GUILayoutOption maxSectionWidth = GUILayout.MaxWidth(WindowWidth / 2);
            GUILayout.BeginVertical(maxSectionWidth);
            DrawSettingsUI();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(maxSectionWidth);
            DrawOptionsUI();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawSettingsUI()
        {
            GUILayout.BeginVertical();
            {
                // Title setting
                GUILayout.Label("Title:");
                pollTitle = GUILayout.TextArea(pollTitle, 80);

                // Duration setting
                hasDuration = GUILayout.Toggle(hasDuration, $" Duration: ({(hasDuration ? $"{duration} seconds" : "disabled")})");
                GUI.enabled = hasDuration;
                duration = Mathf.Round(GUILayout.HorizontalSlider(duration, MinDuration, MaxDuration));
                GUI.enabled = true;

                // Fill the rest with empty space
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            // Poll start and end button
            Color originalColor = GUI.color;
            if (PollManager.Instance.IsPollActive)
            {
                GUI.color = Color.red;
                if (GUILayout.Button("End poll"))
                {
                    PollManager.Instance.EndPoll();
                    if (Plugin.config.pollCreatorMenu_CloseOnEnd.Value)
                        SetOpen(false);
                }
            }
            else
            {
                GUI.color = Color.green;
                if (GUILayout.Button("Start poll"))
                {
                    PollManager.Instance.StartPoll(pollTitle, hasDuration ? duration : float.PositiveInfinity, pollOptions.ToArray());
                    if (Plugin.config.pollCreatorMenu_CloseOnStart.Value)
                        SetOpen(false);
                }
            }
            GUI.color = originalColor;
        }

        private void DrawOptionsUI()
        {
            // Options scroll view
            windowPollOptionsScrollPosition = GUILayout.BeginScrollView(windowPollOptionsScrollPosition, GUI.skin.box);
            {
                int removeIndex = -1;
                for (int i = 0; i < pollOptions.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    pollOptions[i] = GUILayout.TextArea(pollOptions[i], 80);

                    GUI.enabled = pollOptions.Count > MinimumPollOptions;
                    Color originalColor = GUI.color;
                    GUI.color = Color.red;
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                        removeIndex = i;
                    GUI.color = originalColor;
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                }
                if (removeIndex >= 0)
                    pollOptions.RemoveAt(removeIndex);
            }
            GUILayout.EndScrollView();

            // Add option button
            GUI.enabled = pollOptions.Count < MaximumPollOptions;
            if (GUILayout.Button("Add option"))
                pollOptions.Add(string.Empty);
            GUI.enabled = true;
        }
    }
}
