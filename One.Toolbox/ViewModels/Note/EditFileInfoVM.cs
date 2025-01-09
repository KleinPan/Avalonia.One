using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Helpers;
using One.Toolbox.Services;
using One.Toolbox.Views.Note;

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace One.Toolbox.ViewModels.Note;

public partial class EditFileInfoVM : ObservableObject
{
    /// <summary>文件名</summary>
    [ObservableProperty]
    private string fileName;

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
    private string isReadOnlyReason;

    /// <summary>当前打开的文件路径</summary>
    [ObservableProperty]
    private string fileParentDirectory;

    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;

    [ObservableProperty]
    private string mdContent;

    [ObservableProperty]
    private bool showInDesktop;

    public Action UpdateInfoAction { get; set; }

    public string FilePath
    {
        get => FileParentDirectory + "\\" + FileName;
    }

    public string FileFullPath
    {
        get => FilePath + suffix;
    }

    public const string suffix = ".md";

    /// <summary>UI 展示数据使用</summary>
    public EditFileInfoVM() { }

    /// <summary>正常使用</summary>
    /// <param name="filePath"></param>
    public EditFileInfoVM(string filePath)
    {
        FileParentDirectory = Directory.GetParent(filePath)!.FullName;
        FileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        //Extension = System.IO.Path.GetExtension(FilePath);
        lastFileName = FileName;
        littleNotePage = new LittleNoteWnd();
        littleNotePage.DataContext = this;
    }

    #region RelayCommand

    [RelayCommand]
    private async void OpenFile()
    {
        //var dlg = new OpenFileDialog();
        //if (dlg.ShowDialog().GetValueOrDefault())
        //{
        //    var fileViewModel = LoadDocument(dlg.FileName);
        //}

        try
        {
            var filesService = App.Current?.Services?.GetService<IFilesService>();
            if (filesService is null)
                throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.OpenFileAsync();
            if (file is null)
                return;

            // Limit the text file to 1MB so that the demo wont lag.
            if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 1)
            {
                await using var readStream = await file.OpenReadAsync();
                using var reader = new StreamReader(readStream);
                var FileText = await reader.ReadToEndAsync();
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
    private async void SaveFile()
    {
        await SaveDocument();
    }

    [RelayCommand]
    private void RenameFile(object obj)
    {
        var parent = obj as Grid;

        var a = parent!.GetLogicalChildren().OfType<TextBox>().FirstOrDefault();

        IsEditFileName = true;

        a!.LostFocus += EditFileInfoVM_LostFocus;
        a!.KeyDown += EditFileInfoVM_KeyDown;
        a.Focus();
    }

    [RelayCommand]
    private void DeleteFile(object obj)
    {
        File.Delete(FilePath + suffix);
    }

    private string lastFileName = "";

    private void EditFileInfoVM_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        //可能触发多次
        //if (!IsEditFileName)
        //{
        //    return;
        //}
        TextBox textBox = sender as TextBox;
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            IsEditFileName = false;

            FileName = textBox.Text.Trim();

            UpdateInfoAction?.Invoke();
            SaveDocument();

            File.Delete(FileParentDirectory + "\\" + lastFileName + suffix);
            lastFileName = FileName;
        }
    }

    private void EditFileInfoVM_LostFocus(object? sender, RoutedEventArgs e)
    {
        TextBox textBox = sender as TextBox;

        IsEditFileName = false;

        textBox.LostFocus -= EditFileInfoVM_LostFocus;
        textBox.KeyDown -= EditFileInfoVM_KeyDown;
    }

    #endregion RelayCommand

    private LittleNoteWnd littleNotePage;

    partial void OnMdContentChanged(string? oldValue, string newValue)
    {
        if (oldValue == null)//第一次加载
        {
            return;
        }
        if (oldValue == newValue)
        {
            IsDirty = false;
        }
        else
        {
            IsDirty = true;
        }
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

    async Task<bool> LoadDocument(string filePath)
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
        if (FilePath == null || string.IsNullOrEmpty(FilePath))
            throw new ArgumentNullException("fileName");

        if (IsDirty)
        {
            ModifyTime = DateTime.Now;

            await File.WriteAllTextAsync(FilePath + suffix, MdContent);
            App.Current!.Services.GetService<INotifyService>()!.ShowInfoMessage("Save success!");
        }
    }
}