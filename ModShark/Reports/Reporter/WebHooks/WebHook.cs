using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ModShark.Reports.Render;

namespace ModShark.Reports.Reporter.WebHooks;

[PublicAPI]
public class WebHook
{
    public string Url { get; set; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter<WebHookType>))]
    public WebHookType Type { get; set; } = WebHookType.Discord;

    public int MaxLength { get; set; } = 2000;

    [JsonConverter(typeof(JsonStringEnumConverter<FlagInclusion>))]
    public FlagInclusion FlagInclusion { get; set; } = FlagInclusion.None;
}

public enum WebHookType
{
    Discord
}