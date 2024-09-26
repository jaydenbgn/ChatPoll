using BepInEx.Configuration;

namespace ChatPoll
{
    internal class ChatPollConfig
    {
        internal ChatPollConfig(ConfigFile config)
        {
            const string pollCreatorMenuSection = "PollCreatorMenu";

            pollCreatorMenu_CloseOnStart = config.Bind(pollCreatorMenuSection, "CloseOnStart", true, 
                "Whether or not the Poll Creator menu should close when the 'Start poll' button is pressed");

            pollCreatorMenu_CloseOnEnd = config.Bind(pollCreatorMenuSection, "CloseOnEnd", true,
                "Whether or not the Poll Creator menu should close when the 'End poll' button is pressed");

            pollCreatorMenu_OpenPhrase = config.Bind(pollCreatorMenuSection, "OpenPhrase", "/poll",
                "The phrase said in chat to open the poll menu. Leave blank to disable this feature");

            pollCreatorMenu_Scale = config.Bind(pollCreatorMenuSection, "Scale", 1f,
                "The base scale of the Poll Creator menu");

            pollCreatorMenu_ScaleWithScreenSize = config.Bind(pollCreatorMenuSection, "ScaleWithScreenSize", true,
                "Whether or not the Poll Creator menu should scale with screen size");
        }

        internal readonly ConfigEntry<bool> pollCreatorMenu_CloseOnStart;
        internal readonly ConfigEntry<bool> pollCreatorMenu_CloseOnEnd;
        internal readonly ConfigEntry<string> pollCreatorMenu_OpenPhrase;
        internal readonly ConfigEntry<float> pollCreatorMenu_Scale;
        internal readonly ConfigEntry<bool> pollCreatorMenu_ScaleWithScreenSize;
    }
}
