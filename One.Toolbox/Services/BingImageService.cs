using Microsoft.Extensions.DependencyInjection;

using One.Base.ExtensionMethods;
using One.Base.Helpers;
using One.Base.Helpers.HttpHelper;
using One.Toolbox.ViewModels.BingImage;
using One.Toolbox.ViewModels.Setting;

using System.Text.Json;

namespace One.Toolbox.Services;

public sealed class BingImageService
{
    private readonly string configPath = Helpers.PathHelper.imagePath + "ImageInfo.json";
    private readonly SemaphoreSlim ioLock = new(1, 1);

    public async Task<IReadOnlyList<UsefullImageInfoVM>> LoadLatestAsync(CancellationToken cancellationToken = default)
    {
        var latest = await GetLatestImageInfoAsync(cancellationToken).ConfigureAwait(false);
        var imageList = await MergeAndSaveAsync(latest, cancellationToken).ConfigureAwait(false);

        foreach (var item in imageList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await item.LoadCover().ConfigureAwait(false);
        }

        return imageList
            .OrderByDescending(x => x.LocalImageName)
            .ToList();
    }

    private static async Task<BingImageOriginalModel?> GetLatestImageInfoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var json = await HTTPClientHelper.GetStringAsync("http://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8")
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            return JsonSerializer.Deserialize(json, SourceGenerationContext.Default.BingImageOriginalModel);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.Message);
            return null;
        }
    }

    private async Task<List<UsefullImageInfoVM>> MergeAndSaveAsync(BingImageOriginalModel? bingImageModel, CancellationToken cancellationToken)
    {
        await ioLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var list = new List<UsefullImageInfoModel>();
            try
            {
                list = IOHelper.Instance.ReadContentFromLocalSourceGeneration(configPath, SourceGenerationContext.Default.ListUsefullImageInfoModel)
                    ?? new List<UsefullImageInfoModel>();
            }
            catch
            {
                list = new List<UsefullImageInfoModel>();
            }

            if (bingImageModel?.images != null)
            {
                foreach (var item in bingImageModel.images)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (list.Any(x => x.LocalImageName == item.fullstartdate))
                    {
                        continue;
                    }

                    list.Add(new UsefullImageInfoModel
                    {
                        DownloadUrl = "http://cn.bing.com" + item.url,
                        Copyright = item.copyright,
                        Title = item.title,
                        LocalImageName = item.fullstartdate,
                        LocalImagePath = Helpers.PathHelper.imagePath + item.fullstartdate + ".jpg",
                    });
                }
            }

            IOHelper.Instance.WriteContentTolocalSourceGeneration(list, configPath, SourceGenerationContext.Default.ListUsefullImageInfoModel);
            return list.Select(x => x.ToVM()).ToList();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.Message);
            return new List<UsefullImageInfoVM>();
        }
        finally
        {
            ioLock.Release();
        }
    }
}
