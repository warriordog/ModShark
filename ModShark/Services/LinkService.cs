using SharkeyDB.Entities;

namespace ModShark.Services;

public interface ILinkService
{
    public string GetLinkToNote(Note note);
    public string GetLinkToUser(User user);
    public string GetLinkToInstance(Instance instance);

    public string GetLocalLinkToNote(Note note);
    public string GetLocalLinkToUser(User user);
}

public class LinkService(SharkeyConfig config) : ILinkService
{
    public string GetLinkToNote(Note note)
        => note.Url ?? GetLocalLinkToNote(note);

    public string GetLinkToUser(User user)
        => user.Uri ?? GetLocalLinkToUser(user);

    public string GetLinkToInstance(Instance instance)
        => $"https://{instance.Host}";

    public string GetLocalLinkToNote(Note note)
        => $"https://{config.PublicHost}/notes/{note.Id}";

    public string GetLocalLinkToUser(User user)
        => user.IsLocal
            ? $"https://{config.PublicHost}/@{user.Username}"
            : $"https://{config.PublicHost}/@{user.Username}@{user.Host}";
}