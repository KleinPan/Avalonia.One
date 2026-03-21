using One.Toolbox.Enums;

using System.Net;

using WebDav;

namespace One.Toolbox.Services;

public sealed class CloudSettingSyncService
{
    private const string TargetDir = "One.Toolbox";
    private const string TargetFile = $"{TargetDir}/Setting.json";

    public async Task<string?> DownloadSettingAsync(
        WebDAVTypeEnum webDavType,
        string userName,
        string password,
        bool useProxy,
        string proxyAddress,
        string localPath,
        CancellationToken cancellationToken = default)
    {
        var param = CreateParam(webDavType, userName, password, useProxy, proxyAddress);
        using var client = new WebDavClient(param);

        var resMK = await client.Mkcol(TargetDir).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!resMK.IsSuccessful)
        {
            return "Connect failed!";
        }

        var res = await client.Propfind(TargetFile).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!res.IsSuccessful || res.Resources.Count == 0)
        {
            return "Not find cloud setting!";
        }

        var rsp = await client.GetRawFile(TargetFile).WaitAsync(cancellationToken).ConfigureAwait(false);
        await using var fileStream = File.Create(localPath);
        await rsp.Stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        return null;
    }

    public async Task<string?> UploadSettingAsync(
        WebDAVTypeEnum webDavType,
        string userName,
        string password,
        bool useProxy,
        string proxyAddress,
        string localPath,
        CancellationToken cancellationToken = default)
    {
        var param = CreateParam(webDavType, userName, password, useProxy, proxyAddress);
        using var client = new WebDavClient(param);

        var resMK = await client.Mkcol(TargetDir).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!resMK.IsSuccessful)
        {
            return $"Error {resMK.Description};";
        }

        await using var fileStream = File.OpenRead(localPath);
        await client.PutFile(TargetFile, fileStream).WaitAsync(cancellationToken).ConfigureAwait(false);
        return null;
    }

    private static WebDavClientParams CreateParam(
        WebDAVTypeEnum webDavType,
        string userName,
        string password,
        bool useProxy,
        string proxyAddress)
    {
        string addr = webDavType switch
        {
            WebDAVTypeEnum.坚果云 => "https://dav.jianguoyun.com/dav/",
            WebDAVTypeEnum.Yandex => "https://webdav.yandex.com/",
            _ => throw new NotSupportedException("Unsupport WebDAV type!"),
        };

        var clientParams = new WebDavClientParams
        {
            BaseAddress = new Uri(addr),
            Credentials = new NetworkCredential(userName, password),
        };

        if (useProxy && !string.IsNullOrWhiteSpace(proxyAddress))
        {
            clientParams.Proxy = new WebProxy(proxyAddress);
        }

        return clientParams;
    }
}
