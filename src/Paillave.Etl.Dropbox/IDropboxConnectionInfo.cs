namespace Paillave.Etl.Dropbox;

public interface IDropboxConnectionInfo
{
    string Token { get; set; }
    string AppKey { get; set; }
    string AppSecret { get; set; }
    int MaxAttempts { get; set; }
}
