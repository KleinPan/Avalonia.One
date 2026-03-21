using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Helpers;
using One.Toolbox.Services;
using One.Toolbox.Views.Note;

using System.Diagnostics;
using System.Text;

namespace One.Toolbox.ViewModels.Note;

public partial class EditFileInfoVM : ObservableObject
{
    /// <summary>文件名</summary>
    [ObservableProperty]
    private string fileName = string.Empty;

    /// <summary>文件名</summary>
    [ObservableProperty]
    private bool isEditFileName;

    [ObservableProperty]
    private DateTime createTime;

    [ObservableProperty]
    private DateTime modifyTime;

    [ObservableProperty]
    private bool isDirty;

    [ObservableProperty]
    private bool isReadOnly;

    [ObservableProperty]
    private string isReadOnlyReason = string.Empty;

    /// <summary>当前打开的文件路径</summary>
    [ObservableProperty]
    private string fileParentDirectory = string.Empty;

    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;

    [ObservableProperty]
    private string mdContent = string.Empty;

    [ObservableProperty]
    private bool showInDesktop;

    public Action? UpdateInfoAction { get; set; }

    public string FilePath => FileParentDirectory + "\\" + FileName;

    public string FileFullPath => FilePath + suffix;

    public const string suffix = ".md";

    private string lastFileName = string.Empty;
    private readonly LittleNoteWnd littleNotePage;

    /// <summary>UI 展示数据使用</summary>
    public EditFileInfoVM()
    {
        littleNotePage = new LittleNoteWnd { DataContext = this };
    }

    /// <summary>正常使用</summary>
    /// <param name="filePath"></param>
    public EditFileInfoVM(string filePath)
    {
        FileParentDirectory = Directory.GetParent(filePath)!.FullName;
        FileName = Path.GetFileNameWithoutExtension(filePath);
        lastFileName = FileName;

        littleNotePage = new LittleNoteWnd { DataContext = this };
    }

    #region RelayCommand

    [RelayCommand]
    private async Task OpenFile()
    {
        try
        {
            var filesService = App.Current?.Services?.GetService<IFilesService>();
            if (filesService is null)
            {
                throw new NullReferenceException("Missing File Service instance.");
            }

            var file = await filesService.OpenFileAsync();
            if (file is null)
            {
                return;
            }

            if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024)
            {
                await using var readStream = await file.OpenReadAsync();
                using var reader = new StreamReader(readStream);
                _ = await reader.ReadToEndAsync();
            }
            else
            {
                throw new Exception("File exceeded 1MB limit.");
            }
        }
        catch (Exception e)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(e.ToString());
        }
    }

    [RelayCommand]
    private void OpenFilePath()
    {
        Process.Start("explorer.exe", PathHelper.dataPath);
    }

    [RelayCommand]
    private Task SaveFile()
    {
        return SaveDocument();
    }

    [RelayCommand]
    private void RenameFile(object obj)
    {
        var parent = obj as Grid;
        var textBox = parent?.GetLogicalChildren().OfType<TextBox>().FirstOrDefault();
        if (textBox is null)
        {
            return;
        }

        IsEditFileName = true;

        textBox.LostFocus += EditFileInfoVM_LostFocus;
        textBox.KeyDown += EditFileInfoVM_KeyDown;
        textBox.Focus();
    }

    private void EditFileInfoVM_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        if (e.Key == Avalonia.Input.Key.Enter)
        {
            IsEditFileName = false;

            FileName = textBox.Text?.Trim() ?? string.Empty;

            UpdateInfoAction?.Invoke();
            _ = SaveDocument();

            File.Delete(FileParentDirectory + "\\" + lastFileName + suffix);
            lastFileName = FileName;
        }
    }

    private void EditFileInfoVM_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        IsEditFileName = false;

        textBox.LostFocus -= EditFileInfoVM_LostFocus;
        textBox.KeyDown -= EditFileInfoVM_KeyDown;
    }

    #endregion RelayCommand

    partial void OnMdContentChanged(string? oldValue, string newValue)
    {
        if (oldValue == null)
        {
            return;
        }

        IsDirty = oldValue != newValue;
    }

    partial void OnShowInDesktopChanged(bool value)
    {
        if (value)
        {
            littleNotePage.Show();
        }
        else
        {
            littleNotePage.Hide();
        }
    }

    public async Task LoadDocument()
    {
        var res = await LoadDocument(FilePath + suffix);

        if (!res)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage($"{FilePath + suffix} 不存在！");
        }
    }

    /// <summary>创建文件</summary>
    /// <returns></returns>
    public bool CreateNewFile()
    {
        if (!File.Exists(FilePath + suffix))
        {
            File.Create(FilePath + suffix).Dispose();

            ModifyTime = CreateTime = DateTime.Now;
        }
        else
        {
            return false;
        }

        return true;
    }

    private async Task<bool> LoadDocument(string filePath)
    {
        if (File.Exists(filePath))
        {
            MdContent = await File.ReadAllTextAsync(filePath);
            return true;
        }

        return false;
    }

    public async Task SaveDocument()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            throw new ArgumentNullException(nameof(FilePath));
        }

        if (IsDirty)
        {
            ModifyTime = DateTime.Now;

            await File.WriteAllTextAsync(FilePath + suffix, MdContent);
            App.Current!.Services.GetService<INotifyService>()!.ShowInfoMessage("Save success!");
        }
    }
}
