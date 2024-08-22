using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace ModShark.Reports.Reporter.WebHooks;

[PublicAPI]
public class WebHook
{
    public string Url { get; set; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter<WebHookType>))]
    public WebHookType Type { get; set; } = WebHookType.Discord;

    public int MaxLength { get; set; } = 2000;
}

public enum WebHookType
{
    Discord
}