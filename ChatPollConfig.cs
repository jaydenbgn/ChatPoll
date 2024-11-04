using BepInEx.Configuration;

#nullable enable

namespace ChatPoll
{
    internal class ChatPollConfig
    {
        internal ChatPollConfig(ConfigFile config)
        {
            const string pollCreatorMenuSection = "PollCreatorMenu";

            pollCreatorMenu_CloseOnStart = config.Bind(pollCreatorMenuSection, "CloseOnStart", true,
                "Whether or not the Poll Creator menu should close when the 'Start poll' button is pressed.");

            pollCreatorMenu_CloseOnEnd = config.Bind(pollCreatorMenuSection, "CloseOnEnd", true,
                "Whether or not the Poll Creator menu should close when the 'End poll' button is pressed.");

            pollCreatorMenu_OpenPhrase = config.Bind(pollCreatorMenuSection, "OpenPhrase", "/poll",
                "The phrase said in chat to open the poll menu. Leave blank to disable this feature.");

            pollCreatorMenu_Scale = config.Bind(pollCreatorMenuSection, "Scale", 1f,
                "The base scale of the Poll Creator menu.");

            pollCreatorMenu_ScaleWithScreenSize = config.Bind(pollCreatorMenuSection, "ScaleWithScreenSize", true,
                "Whether or not the Poll Creator menu should scale with screen size.");


            const string voteMessageSection = "VoteMessage";

            voteMessage_Visible = config.Bind(voteMessageSection, "Visible", true,
                "Whether or not to show a local message in chat when someone votes.");

            voteMessage_ShowVotedOption = config.Bind(voteMessageSection, "ShowVotedOption", false,
                "Whether or not to show what the player voted for in the vote message.");

            voteMessage_ShowTotalVotes = config.Bind(voteMessageSection, "ShowTotalVotes", true,
                "Whether or not to show how many players have voted in the vote message.");
        }
        
        internal readonly ConfigEntry<bool> pollCreatorMenu_CloseOnStart;
        internal readonly ConfigEntry<bool> pollCreatorMenu_CloseOnEnd;
        internal readonly ConfigEntry<string> pollCreatorMenu_OpenPhrase;
        internal readonly ConfigEntry<float> pollCreatorMenu_Scale;
        internal readonly ConfigEntry<bool> pollCreatorMenu_ScaleWithScreenSize;

        internal readonly ConfigEntry<bool> voteMessage_Visible;
        internal readonly ConfigEntry<bool> voteMessage_ShowVotedOption;
        internal readonly ConfigEntry<bool> voteMessage_ShowTotalVotes;
    }
}
