namespace SizManager.Models;

public record UpdateInfo(
    string Version,
    string Changelog,
    string DownloadUrl,
    string FileName
);
