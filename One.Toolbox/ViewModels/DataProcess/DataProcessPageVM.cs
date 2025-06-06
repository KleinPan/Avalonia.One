﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

using AvaloniaEdit;

using Microsoft.Extensions.DependencyInjection;

using One.Control.Helpers;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Helpers;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace One.Toolbox.ViewModels.DataProcess;

public partial class DataProcessPageVM : BasePageVM
{
    public ObservableCollection<string> ConverterTaskList { get; set; } = new ObservableCollection<string>();

    [ObservableProperty]
    private string selectedConverterTask;

    [ObservableProperty]
    private string leftSelectedConverterTask;

    public ObservableCollection<string> SelectedConverterTaskList { get; set; } = new ObservableCollection<string>();

    [ObservableProperty]
    private string inputString = "1234";

    [ObservableProperty]
    private string outputString;

    [ObservableProperty]
    private double lineHeight;

    public DataProcessPageVM()
    {
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.DataProcess);
    }

    public override void InitializeViewModel()
    {
        base.InitializeViewModel();

        foreach (KeyValuePair<string, Func<byte[], byte[]>> i in converters)
            ConverterTaskList.Add(i.Key);

        InputString = "";

        SelectedConverterTask = ConverterTaskList.First();

        GetTargetControl();
    }

    private void DoConvert()
    {
        if (string.IsNullOrEmpty(InputString))
            return;

        byte[] row = Encoding.Default.GetBytes(InputString);
        for (int i = 0; i < SelectedConverterTaskList.Count; i++)
        {
            string name = SelectedConverterTaskList[i] as string;
            if (converters.ContainsKey(name))
                row = converters[name](row);
        }
        OutputString = Encoding.Default.GetString(row);
    }

    private void DoConvert2()
    {
        if (string.IsNullOrEmpty(InputString))
            return;

        byte[] row = Encoding.Default.GetBytes(InputString);

        if (converters.ContainsKey(SelectedConverterTask))
            row = converters[SelectedConverterTask](row);

        OutputString = Encoding.Default.GetString(row);
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void AddNewTask()
    {
        if (SelectedConverterTask == null)
            return;
        SelectedConverterTaskList.Add(SelectedConverterTask);
        DoConvert();
    }

    [RelayCommand]
    private void DeleceLastTask()
    {
        if (SelectedConverterTaskList.Count == 0)
            return;

        if (string.IsNullOrEmpty(LeftSelectedConverterTask))
        {
            return;
        }
        SelectedConverterTaskList.Remove(LeftSelectedConverterTask);
        DoConvert();
    }

    [RelayCommand]
    private void ExcuteSelectedOme()
    {
        if (SelectedConverterTask == null)
            return;
        LineHeight = 0.0;

        if (wrapPanel != null)
        {
            wrapPanel.Children.Clear();
        }

        DoConvert2();
    }

    private void GetTargetControl()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var a = TopLevel.GetTopLevel(currentPage);

            wrapPanel = MyVisualTreeHelper.FindControlByName<WrapPanel>(a, "wrapPanel");
        });
    }

    /// <summary>转换器</summary>
    private Dictionary<string, Func<byte[], byte[]>> converters = new Dictionary<string, Func<byte[], byte[]>>
    {
        ["InitToHexArray"] = (e) => Encoding.Default.GetBytes(InitArrayEvent(Encoding.Default.GetString(e))),
        ["String to Hex(with space)"] = (e) => Encoding.Default.GetBytes(BitConverter.ToString(e).Replace("-", " ")),
        ["String to Hex(without space)"] = (e) => Encoding.Default.GetBytes(BitConverter.ToString(e).Replace("-", "")),
        ["Hex to String"] = (e) => Hex2byte(Encoding.Default.GetString(e)),
        ["String to Base64"] = (e) => { try { return Encoding.Default.GetBytes(System.Convert.ToBase64String(e)); } catch (Exception ee) { return Encoding.Default.GetBytes(ee.Message); } },
        ["Base64 to String"] = (e) => { try { return System.Convert.FromBase64String(Encoding.Default.GetString(e)); } catch (Exception ee) { return Encoding.Default.GetBytes(ee.Message); } },
        ["URL encode"] = (e) => Encoding.Default.GetBytes(System.Web.HttpUtility.UrlEncode(Encoding.Default.GetString(e))),
        ["URL decode"] = (e) => Encoding.Default.GetBytes(System.Web.HttpUtility.UrlDecode(Encoding.Default.GetString(e))),
        ["HTML encode"] = (e) => Encoding.Default.GetBytes(System.Web.HttpUtility.HtmlEncode(Encoding.Default.GetString(e))),
        ["HTML decode"] = (e) => Encoding.Default.GetBytes(System.Web.HttpUtility.HtmlDecode(Encoding.Default.GetString(e))),
        ["String to Unicode"] = (e) => Encoding.Default.GetBytes(String2Unicode(Encoding.Default.GetString(e))),
        ["Unicode to String"] = (e) => Encoding.Default.GetBytes(Unicode2String(Encoding.Default.GetString(e))),
        ["String to MD5 (Hex)"] = (e) => Encoding.Default.GetBytes(BitConverter.ToString(MD5Encrypt(e)).Replace("-", "")),
        ["String to SHA-1 (Hex)"] = (e) => Encoding.Default.GetBytes(BitConverter.ToString(Sha1Encrypt(e)).Replace("-", "")),
        ["String to SHA-256 (Hex)"] = (e) => Encoding.Default.GetBytes(BitConverter.ToString(Sha256Encrypt(e)).Replace("-", "")),
        ["String to SHA-512 (Hex)"] = (e) => Encoding.Default.GetBytes(BitConverter.ToString(Sha512Encrypt(e)).Replace("-", "")),
    };

    #region Helpers

    private static string InitArrayEvent(string input)
    {
        try
        {
            string output = "";

            if (input.Contains(' '))
            {
                input = input.Replace(" ", "");
            }
            else
            {
            }

            int len = input.Length;

            for (int i = 0; i < len; i = i + 2)
            {
                if (i > input.Length)
                {
                    break;
                }

                string single = "0x" + input[i] + input[i + 1] + ",";
                output += single;
            }

            return output;
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.Message);
            return "";
        }
    }

    public static byte[] Hex2byte(string mHex)
    {
        mHex = Regex.Replace(mHex, "[^0-9A-Fa-f]", "");
        if (mHex.Length % 2 != 0)
            mHex = mHex.Remove(mHex.Length - 1, 1);
        if (mHex.Length <= 0) return new byte[] { };
        byte[] vBytes = new byte[mHex.Length / 2];
        for (int i = 0; i < mHex.Length; i += 2)
            if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2]))
                vBytes[i / 2] = 0;
        return vBytes;
    }

    public static byte[] MD5Encrypt(byte[] b)
    {
        MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
        byte[] hashedDataBytes;
        hashedDataBytes = md5Hasher.ComputeHash(b);
        return hashedDataBytes;
    }

    public static byte[] Sha1Encrypt(byte[] b)
    {
        SHA1CryptoServiceProvider md5Hasher = new SHA1CryptoServiceProvider();
        byte[] hashedDataBytes;
        hashedDataBytes = md5Hasher.ComputeHash(b);
        return hashedDataBytes;
    }

    public static byte[] Sha256Encrypt(byte[] b)
    {
        SHA256CryptoServiceProvider md5Hasher = new SHA256CryptoServiceProvider();
        byte[] hashedDataBytes;
        hashedDataBytes = md5Hasher.ComputeHash(b);
        return hashedDataBytes;
    }

    public static byte[] Sha512Encrypt(byte[] b)
    {
        SHA512CryptoServiceProvider md5Hasher = new SHA512CryptoServiceProvider();
        byte[] hashedDataBytes;
        hashedDataBytes = md5Hasher.ComputeHash(b);
        return hashedDataBytes;
    }

    public static string String2Unicode(string source)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(source);
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i += 2)
        {
            stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
        }
        return stringBuilder.ToString();
    }

    public static string Unicode2String(string source)
    {
        return new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(source, x => System.Convert.ToChar(System.Convert.ToUInt16(x.Result("1"), 16)).ToString());
    }

    #endregion Helpers

    #region ShowIndex

    private WrapPanel wrapPanel;

    [RelayCommand]
    private void ShowIndexEvent()
    {
        try
        {
            if (string.IsNullOrEmpty(InputString)) { return; }
            OutputString = "";

            wrapPanel.IsVisible = true;

            string input = InputString.Trim();

            //归一
            if (input.Contains(' '))
            {
                input = input.Replace(" ", "");
            }

            int len1 = input.Length;

            List<string> showList = new List<string>();
            for (int i = 0; i < len1; i += 2)
            {
                if (i > input.Length)
                {
                    break;
                }
                string single = "";

                single = input[i].ToString() + input[i + 1].ToString();

                showList.Add(single);
            }

            GenerateBox(wrapPanel, showList);
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.Message);
        }
    }

    private void GenerateBox(object element, List<string> strings)
    {
        WrapPanel wrapPanel = (WrapPanel)element;
        wrapPanel.Children.Clear();

        for (int i = 0; i < strings.Count; i++)
        {
            Border border = new Border();

            //border.BorderThickness = new Thickness(1);
            //border.BorderBrush = System.Windows.Media.Brushes.Blue;

            Grid innerGrid = new Grid();

            innerGrid.RowDefinitions.Add(new RowDefinition());
            innerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            innerGrid.RowDefinitions.Add(new RowDefinition());

            border.Margin = new Thickness(5, 5, 0, 5);

            innerGrid.Width = 20;

            //内容
            TextBlock txbChar = new TextBlock();
            txbChar.FontSize = 15;
            txbChar.Margin = new Thickness(0, 0, 0, 1);
            txbChar.VerticalAlignment = VerticalAlignment.Bottom;
            txbChar.HorizontalAlignment = HorizontalAlignment.Center;
            //txbChar.Background = System.Windows.Media.Brushes.Cyan;
            txbChar.Text = strings[i].ToString();
            txbChar.Foreground = Brushes.Black;

            innerGrid.Children.Add(txbChar);
            Grid.SetRow(txbChar, 0);

            Line line = new();
            line.StartPoint = new Point(1, 0);
            line.EndPoint = new Point(15, 0);

            // line.Stroke = Brushes.Black;
            line.Stroke = (SolidColorBrush)ResourceHelper.FindObjectResource("SemiColorPrimary");

            line.HorizontalAlignment = HorizontalAlignment.Center;

            innerGrid.Children.Add(line);
            Grid.SetRow(line, 1);
            //编号
            TextBlock txbIndex = new TextBlock();
            txbIndex.Text = i.ToString();
            txbIndex.Margin = new Thickness(0, 1, 0, 0);
            txbIndex.VerticalAlignment = VerticalAlignment.Top;
            txbIndex.HorizontalAlignment = HorizontalAlignment.Center;

            txbIndex.Foreground = (SolidColorBrush)ResourceHelper.FindObjectResource("SemiColorText1");
            innerGrid.Children.Add(txbIndex);
            Grid.SetRow(txbIndex, 2);

            border.Child = innerGrid;

            wrapPanel.Children.Add(border);
        }
    }

    #endregion ShowIndex

    #region DataFormate

    [ObservableProperty]
    private string formateButtonContent = "Formate";

    [ObservableProperty]
    private string rawString = "";

    [ObservableProperty]
    private string formateString = "";

    [ObservableProperty]
    private string showString = "";

    private bool isFormated = false;

    [RelayCommand]
    private void Formate()
    {
        if (!isFormated)
        {
            RawString = ShowString;//备份

            FormateString = JsonFormate(RawString);

            ShowString = FormateString;
            FormateButtonContent = "ConvertBack";
            isFormated = true;
        }
        else
        {
            ShowString = RawString;

            FormateButtonContent = "Formate";

            isFormated = false;
        }
    }

    private JsonSerializerOptions jsonOption = new JsonSerializerOptions()
    {
        // 整齐打印
        WriteIndented = true,
        //重新编码，解决中文乱码问题
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private string JsonFormate(string input)
    {
        try
        {
            var jsonDocument = JsonDocument.Parse(input);

            var formatJson = JsonSerializer.Serialize(jsonDocument, jsonOption);
            // 格式化输出
            return formatJson;
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.ToString());
            return "";
        }
    }

    #endregion DataFormate
}