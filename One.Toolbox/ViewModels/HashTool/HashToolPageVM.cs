using One.Base.Helpers.EncryptionHelpers;
using One.Toolbox.ViewModels.Base;

using System.Diagnostics;

namespace One.Toolbox.ViewModels.HashTool;

public partial class HashToolPageVM : BasePageVM
{
    public HashToolPageVM()
    {
    }

    public override void UpdateTitle()
    {
        Title = "HashTool";
    }

    [ObservableProperty]
    private string anylizeResult;

    [ObservableProperty]
    private string filePath;

    #region CheckBox

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

    #endregion CheckBox

    [ObservableProperty]
    private int progress;

    private string GetFileName(Uri filePath)
    {
        //var res = System.IO.Path.GetFileName(filePath);

        var res = filePath.LocalPath;
        return res;
    }

    private FileInfo GetFileInfo(string filePath)
    {
        System.IO.FileInfo res = new FileInfo(filePath);

        return res;
    }

    private string GetCRC32(string filePath)
    {
        var res = CalculateFileCRC32(filePath);

        return res.ToString("x2");
    }

    private FileVersionInfo GetFileVersionInfo(string filePath)
    {
        FileVersionInfo res = FileVersionInfo.GetVersionInfo(filePath);

        return res;
    }

    private string GetMD5(string filePath)
    {
        var res = Md5Helper.GetFileMd5(filePath);
        return res;
    }

    private string GetSHA1(string filePath)
    {
        var res = SHAHelper.CalculateFileSHA1(filePath);
        return res;
    }

    private string GetSHA256(string filePath)
    {
        var res = SHAHelper.CalculateFileSHA256(filePath);
        return res;
    }

    private int test()
    {
        int a = 0;

        if (ShowFilePath)
            a++;

        if (ShowFileSize)
            a++;
        if (ShowFileVersion)
            a++;
        if (ShowFileModifyTime)
            a++;
        if (ShowMD5)
            a++;
        if (ShowSHA1)
            a++;
        if (ShowSHA256)
            a++;
        if (ShowCRC32)
            a++;

        return a;
    }

    private List<Uri> Last;

    [RelayCommand]
    private void Drop(object obj)
    {
        Last = obj as List<Uri>;
        DoCal(Last);
    }

    [RelayCommand]
    private void Start()
    {
        DoCal(Last);
    }

    private void DoCal(List<Uri> last)
    {
        int add = 100 / last.Count;
        int everyAdd = add / test();
        Progress = 0;
        try
        {
            foreach (var item in last)
            {
                var path = GetFileName(item);
                if (ShowFilePath)
                {
                    AnylizeResult += "文件路径：" + path + "\r\n";

                    Progress += everyAdd;
                }

                if (ShowFileSize)
                {
                    var fileInfo = GetFileInfo(path);
                    //AnylizeResult += "文件大小：" + Math.Round(fileInfo.Length / 1024.0, 2) + "kb (" + fileInfo.Length + "字节)\r\n";
                    AnylizeResult += "文件大小：" + FormatBytes(fileInfo.Length) + " (" + fileInfo.Length + "字节)\r\n";
                    Progress += everyAdd;
                }
                if (ShowFileModifyTime)
                {
                    var fileInfo = GetFileInfo(path);
                    AnylizeResult += "修改时间：" + fileInfo.LastWriteTime + "\r\n";
                    Progress += everyAdd;
                }

                if (ShowMD5)
                {
                    var md5 = GetMD5(path);
                    AnylizeResult += "MD5：" + md5 + "\r\n";
                    Progress += everyAdd;
                }
                if (ShowSHA1)
                {
                    var SHA1 = GetSHA1(path);
                    AnylizeResult += "SHA1：" + SHA1 + "\r\n";
                    Progress += everyAdd;
                }
                if (ShowSHA256)
                {
                    var SHA256 = GetSHA256(path);
                    AnylizeResult += "SHA256：" + SHA256 + "\r\n";
                    Progress += everyAdd;
                }
                if (ShowCRC32)
                {
                    var CRC32 = GetCRC32(path);
                    AnylizeResult += "CRC32：" + CRC32 + "\r\n";
                    Progress += everyAdd;
                }
                if (ShowFileVersion)
                {
                    var fileInfo = GetFileVersionInfo(path);
                    AnylizeResult += "文件版本：" + fileInfo.FileVersion.ToString() + "\r\n";
                    Progress += everyAdd;
                }
                Progress = 100;
                AnylizeResult += "\r\n";
            }
        }
        catch (Exception ex) { }
    }

    [RelayCommand]
    private void Clear()
    {
        AnylizeResult = "";
        Progress = 0;
    }

    #region Helpers

    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    #endregion Helpers

    #region CRC32

    static uint CalculateFileCRC32(string filePath)
    {
        const uint polynomial = 0xedb88320;
        uint crc = 0xffffffff;

        byte[] buffer = new byte[4096];
        using (FileStream fs = File.OpenRead(filePath))
        {
            int bytesRead;
            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    crc ^= buffer[i];
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc >> 1) ^ ((crc & 1) * polynomial);
                    }
                }
            }
        }

        return ~crc;
    }

    #endregion CRC32
}