using One.Base.Helpers.EncryptionHelpers;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Services.Navigation;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.Views.HashTool;

using System.Diagnostics;
using System.Text;

namespace One.Toolbox.ViewModels.HashTool;

[ToolPage(90, Language.HashTool, "premium_regular", typeof(HashToolPage))]
public partial class HashToolPageVM : BasePageVM
{
    [ObservableProperty]
    private string anylizeResult = string.Empty;

    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private bool showFilePath = true;

    [ObservableProperty]
    private bool showFileSize;

    [ObservableProperty]
    private bool showFileVersion;

    [ObservableProperty]
    private bool showFileModifyTime;

    [ObservableProperty]
    private bool showMD5 = true;

    [ObservableProperty]
    private bool showSHA1;

    [ObservableProperty]
    private bool showSHA256;

    [ObservableProperty]
    private bool showCRC32 = true;

    [ObservableProperty]
    private int progress;

    private List<Uri>? Last;

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.HashTool)!;
    }

    [RelayCommand]
    private void Drop(object obj)
    {
        Last = obj as List<Uri>;
        if (Last == null || Last.Count == 0)
        {
            return;
        }

        DoCal(Last);
    }

    [RelayCommand]
    private void Start()
    {
        if (Last == null || Last.Count == 0)
        {
            return;
        }

        DoCal(Last);
    }

    private int GetEnabledCount()
    {
        var count = 0;
        if (ShowFilePath) count++;
        if (ShowFileSize) count++;
        if (ShowFileVersion) count++;
        if (ShowFileModifyTime) count++;
        if (ShowMD5) count++;
        if (ShowSHA1) count++;
        if (ShowSHA256) count++;
        if (ShowCRC32) count++;
        return count;
    }

    private static string GetFileName(Uri filePath)
    {
        return filePath.LocalPath;
    }

    private static FileInfo GetFileInfo(string path)
    {
        return new FileInfo(path);
    }

    private static string GetCRC32(string path)
    {
        return CalculateFileCRC32(path).ToString("x2");
    }

    private static FileVersionInfo GetFileVersionInfo(string path)
    {
        return FileVersionInfo.GetVersionInfo(path);
    }

    private static string GetMD5(string path)
    {
        return Md5Helper.GetFileMd5(path);
    }

    private static string GetSHA1(string path)
    {
        return SHAHelper.CalculateFileSHA1(path);
    }

    private static string GetSHA256(string path)
    {
        return SHAHelper.CalculateFileSHA256(path);
    }

    private void DoCal(List<Uri> last)
    {
        if (last.Count == 0)
        {
            return;
        }

        var enabledCount = GetEnabledCount();
        if (enabledCount == 0)
        {
            return;
        }

        var add = 100 / last.Count;
        var everyAdd = add / enabledCount;
        Progress = 0;

        var sb = new StringBuilder(AnylizeResult);

        try
        {
            foreach (var item in last)
            {
                var path = GetFileName(item);
                if (ShowFilePath)
                {
                    sb.Append(I18nManager.GetString(Language.HashFilePathLabel)).Append(": ").AppendLine(path);
                    Progress += everyAdd;
                }

                if (ShowFileSize)
                {
                    var fileInfo = GetFileInfo(path);
                    sb.Append(I18nManager.GetString(Language.HashFileSizeLabel)).Append(": ")
                        .Append(FormatBytes(fileInfo.Length)).Append(" (").Append(fileInfo.Length).Append(' ')
                        .Append(I18nManager.GetString(Language.HashBytesUnit)).AppendLine(")");
                    Progress += everyAdd;
                }

                if (ShowFileModifyTime)
                {
                    var fileInfo = GetFileInfo(path);
                    sb.Append(I18nManager.GetString(Language.HashModifiedTimeLabel)).Append(": ").Append(fileInfo.LastWriteTime).AppendLine();
                    Progress += everyAdd;
                }

                if (ShowMD5)
                {
                    sb.Append("MD5: ").AppendLine(GetMD5(path));
                    Progress += everyAdd;
                }

                if (ShowSHA1)
                {
                    sb.Append("SHA1: ").AppendLine(GetSHA1(path));
                    Progress += everyAdd;
                }

                if (ShowSHA256)
                {
                    sb.Append("SHA256: ").AppendLine(GetSHA256(path));
                    Progress += everyAdd;
                }

                if (ShowCRC32)
                {
                    sb.Append("CRC32: ").AppendLine(GetCRC32(path));
                    Progress += everyAdd;
                }

                if (ShowFileVersion)
                {
                    var fileInfo = GetFileVersionInfo(path);
                    sb.Append(I18nManager.GetString(Language.HashFileVersionLabel)).Append(": ").AppendLine(fileInfo.FileVersion);
                    Progress += everyAdd;
                }

                Progress = 100;
                sb.AppendLine();
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            AnylizeResult = sb.ToString();
        }
    }

    [RelayCommand]
    private void Clear()
    {
        AnylizeResult = string.Empty;
        Progress = 0;
    }

    public static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        var order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    private static uint CalculateFileCRC32(string filePath)
    {
        const uint polynomial = 0xedb88320;
        uint crc = 0xffffffff;

        byte[] buffer = new byte[4096];
        using FileStream fs = File.OpenRead(filePath);
        int bytesRead;
        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (var i = 0; i < bytesRead; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    crc = (crc >> 1) ^ ((crc & 1) * polynomial);
                }
            }
        }

        return ~crc;
    }
}
