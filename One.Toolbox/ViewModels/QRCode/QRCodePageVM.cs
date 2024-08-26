using Avalonia;
using Avalonia.Labs.Qr;
using Avalonia.Media;

using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;

using static Avalonia.Labs.Qr.QrCode;

namespace One.Toolbox.ViewModels.QRCode;

public partial class QRCodePageVM : BaseVM
{
    public QRCodePageVM()
    {
        ResetQrCode();
        //Title = "Qr Generator";

        Levels = new ObservableCollection<EccLevel>(Enum.GetValues<EccLevel>());
    }

    [ObservableProperty]
    private string input;

    public override void OnNavigatedEnter()
    {
        base.OnNavigatedEnter();
        InitData();
    }

    void InitData() { }

    [RelayCommand]
    private void GenerateQRCode()
    {
        //编码内容
        if (string.IsNullOrEmpty(Input))
        {
            return;
        }
        string text = Input.Trim();
        if (string.IsNullOrEmpty(text))
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowWarnMessage("请输入二维码内容");
            return;
        }
    }

    #region Property
    [ObservableProperty]
    private string? _qrCodeString;

    [ObservableProperty]
    private double _qrCodeSize = 250;

    [ObservableProperty]
    private Thickness _qrCodePadding = new(10);

    [ObservableProperty]
    private CornerRadius _qrCodeCornerRadius = new(12);

    [ObservableProperty]
    private Color _qrCodeForegroundColor1;

    [ObservableProperty]
    private Color _qrCodeForegroundColor2;

    [ObservableProperty]
    private Color _qrCodeBackgroundColor1;

    [ObservableProperty]
    private Color _qrCodeBackgroundColor2;

    [ObservableProperty]
    private QrCode.EccLevel _qrCodeEccLevel;

    #endregion


    public ObservableCollection<EccLevel> Levels { get; }
    public void ResetQrCode()
    {
        QrCodeEccLevel = QrCode.EccLevel.Medium;

        QrCodeString =
            "I'm a very long text that you might find somewhere as a link or something else.  It's rendered with smooth edges and gradients for the foreground and background";

        QrCodeForegroundColor1 = Colors.Navy;
        QrCodeForegroundColor2 = Colors.DarkRed;
        QrCodeBackgroundColor1 = Colors.White;
        QrCodeBackgroundColor2 = Colors.White;
    }


    #region Random
    private const string Chars = "qwertyuiopasdfghjklzxcvbnm";
    public void RandomizeData()
    {
        UpdateQrCode(string.Join("", Enumerable.Range(0, 150).Select(_ => Chars[Random.Shared.Next(0, Chars.Length)])));
    }
    public void RandomizeColors()
    {
        byte[] newColors = new byte[12];
        Random.Shared.NextBytes(newColors);

        QrCodeForegroundColor1 = Color.FromRgb(newColors[0], newColors[1], newColors[2]);
        QrCodeForegroundColor2 = Color.FromRgb(newColors[3], newColors[4], newColors[5]);

        QrCodeBackgroundColor1 = Color.FromRgb(newColors[6], newColors[7], newColors[8]);
        QrCodeBackgroundColor2 = Color.FromRgb(newColors[9], newColors[10], newColors[11]);

        string? cuurentCode = QrCodeString;
        QrCodeString = string.Empty;

        UpdateQrCode(cuurentCode);
    }
    public void UpdateQrCode(string text)
    {
        if (string.IsNullOrEmpty(text))
            text = "You didn't put anything here?";
        QrCodeString = text;
    }

    #endregion


}
