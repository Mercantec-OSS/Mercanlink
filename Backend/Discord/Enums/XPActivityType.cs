using System.Diagnostics;

namespace Backend.Discord.Enums;

public enum XpActivityType
{
    Message,
    Reaction,
    VoiceMinute,
    DailyLogin,
    CommandUsed,
    KnowledgeCenterApproved,
}

internal static class XpActivityTypeMethods
{
    public static string GetName(this XpActivityType type)
    {
        return type switch
        {
            XpActivityType.Message => "Message",
            XpActivityType.Reaction => "Reaction",
            XpActivityType.VoiceMinute => "VoiceMinute",
            XpActivityType.DailyLogin => "DailyLogin",
            XpActivityType.CommandUsed => "CommandUsed",
            XpActivityType.KnowledgeCenterApproved => "KnowledgeCenterApproved",
            _ => throw new UnreachableException()
        };
    }
}
