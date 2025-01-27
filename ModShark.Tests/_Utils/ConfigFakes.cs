namespace ModShark.Tests._Utils;

public static class ConfigFakes
{
    public static SharkeyConfig MakeSharkeyConfig() => new()
    {
        ServiceAccount = "instance.actor",
        ApiEndpoint = "http://127.0.0.1:3000",
        PublicHost = "example.com",
        FilesDirectoryPath = "/home/sharkey/files"
    };
}