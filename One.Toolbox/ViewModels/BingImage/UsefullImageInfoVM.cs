using Avalonia.Media.Imaging;

using One.Toolbox.Helpers;

using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace One.Toolbox.ViewModels.BingImage;

public partial class UsefullImageInfoVM : ObservableObject
{
    /// <summary> </summary>
    /// <param name="uAction">  指定要设置的参数。参考uAction常数表。 </param>
    /// <param name="uParam">   参考uAction常数表。 </param>
    /// <param name="lpvParam"> 按引用调用的Integer、Long和数据结构。 </param>
    /// <param name="fuWinIni"> 这个参数规定了在设置系统参数的时候，是否应更新用户设置参数。 </param>
    /// <returns> </returns>
    [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
    public static extern int SystemParametersInfo(
                int uAction,
                int uParam,
                string lpvParam,
                int fuWinIni
            );

    public static void SetImageToDesktop(string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SystemParametersInfo(20, 1, filePath, 1);
        }
    }

    [ObservableProperty]
    private string downloadUrl;

    [ObservableProperty]
    private string copyright;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string localImageName;

    [ObservableProperty]
    private string localImagePath;

    [ObservableProperty]
    private Bitmap? imageAvalonia;

    [RelayCommand]
    private void Set()
    {
        SetImageToDesktop(LocalImagePath);
    }

    public UsefullImageInfoModel ToModel()
    {
        UsefullImageInfoModel usefullImageInfoViewModel = new UsefullImageInfoModel();
        usefullImageInfoViewModel.Title = Title;
        usefullImageInfoViewModel.Copyright = Copyright;
        usefullImageInfoViewModel.LocalImagePath = LocalImagePath;
        usefullImageInfoViewModel.LocalImageName = LocalImageName;
        usefullImageInfoViewModel.DownloadUrl = DownloadUrl;

        return usefullImageInfoViewModel;
    }

    public async Task LoadCover()
    {
        await using (var imageStream = await LoadCoverBitmapAsync())
        {
            ImageAvalonia = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
        }
    }

    public async Task<Stream> LoadCoverBitmapAsync()
    {
        if (File.Exists(LocalImagePath))
        {
            return File.OpenRead(LocalImagePath);
        }
        else
        {
            var data = await s_httpClient.GetByteArrayAsync(DownloadUrl);
            WriteImageToDisk(data);

            return new MemoryStream(data);
        }
    }

    private void WriteImageToDisk(byte[] data)
    {
        //创造图片
        using (FileStream fileStream = new FileStream(LocalImagePath, FileMode.Create))
        {
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            //写入图片信息
            binaryWriter.Write(data);
        }
    }

    private static HttpClient s_httpClient = new();
}

public class UsefullImageInfoModel
{
    public string DownloadUrl { get; set; }

    public string Copyright { get; set; }

    public string Title { get; set; }

    public string LocalImageName { get; set; }

    [JsonInclude]
    public string LocalImagePath;//默认情况下，字段不会被序列化。

    public UsefullImageInfoVM ToVM()
    {
        UsefullImageInfoVM usefullImageInfoViewModel = new UsefullImageInfoVM();
        usefullImageInfoViewModel.Title = Title;
        usefullImageInfoViewModel.Copyright = Copyright;
        usefullImageInfoViewModel.LocalImagePath = LocalImagePath;
        usefullImageInfoViewModel.LocalImageName = LocalImageName;
        usefullImageInfoViewModel.DownloadUrl = DownloadUrl;
        //usefullImageInfoViewModel.ImageAvalonia =

        return usefullImageInfoViewModel;
    }
}