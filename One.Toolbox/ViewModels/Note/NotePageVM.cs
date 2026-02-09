using Avalonia.Controls;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;

using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Helpers;
using One.Toolbox.Messenger;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Setting;

using System.Collections.ObjectModel;
using System.IO;

namespace One.Toolbox.ViewModels.Note;

public partial class NotePageVM : BasePageVM
{
    [ObservableProperty]
    private EditFileInfoVM selectedEditFileInfo;

    public ObservableCollection<EditFileInfoVM> EditFileInfoViewModelOC { get; set; } = new ObservableCollection<EditFileInfoVM>();

    public NotePageVM()
    {
        // Register a message in some module
        WeakReferenceMessenger.Default.Register<CloseMessage>(this, (r, m) =>
        {
            // Handle the message here, with r being the recipient and m being the input message.
            // Using the recipient passed as input makes it so that the lambda expression doesn't
            // capture "this", improving performance.

            SaveSetting();
        });

        InitData();
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.Note);
    }

    void InitData()
    {
        LoadSetting();
    }

    public override void OnNavigatedLeave()
    {
        base.OnNavigatedLeave();

        SaveSetting();
    }

    #region Command

    [RelayCommand]
    private void NewFile()
    {
        int index = EditFileInfoViewModelOC.Count;
    tag:
        string filePath = PathHelper.notePath + "untitled" + index + ".md";
        EditFileInfoVM editFileInfoViewModel = new EditFileInfoVM(filePath);
        editFileInfoViewModel.UpdateInfoAction += Update;
        var res = editFileInfoViewModel.CreateNewFile();

        if (res)
        {
            EditFileInfoViewModelOC.Add(editFileInfoViewModel);
            SaveSetting();
        }
        else
        {
            index++;
            goto tag;
            App.Current!.Services.GetService<INotifyService>()!.ShowWarnMessage("File already exist!");
        }
    }

    partial void OnSelectedEditFileInfoChanged(EditFileInfoVM? oldValue, EditFileInfoVM newValue)
    {
        oldValue?.SaveDocument();
        newValue?.LoadDocument();
    }

   
    [RelayCommand]
    private void DeleteFile(object obj)
    {
        if (SelectedEditFileInfo != null)
        {
            if (File.Exists(SelectedEditFileInfo.FileFullPath))
            {
                File.Delete(SelectedEditFileInfo.FileFullPath);
            }

            EditFileInfoViewModelOC.Remove(SelectedEditFileInfo);

            SaveSetting();
        }
    }

    #endregion Command

    #region Setting

    public void SaveSetting()
    {
        var service = App.Current.Services.GetService<SettingService>();

        service.AllConfig.EditFileInfoList.Clear();
        foreach (var item in EditFileInfoViewModelOC)
        {
            EditFileInfo editFileInfo = new()
            {
                FilePath = item.FileFullPath,
                FileName = item.FileName,
                CreateTime = item.CreateTime,
                ModifyTime = item.ModifyTime,
            };
            item.SaveDocument();

            service.AllConfig.EditFileInfoList.Add(editFileInfo);
        }

        service.SaveCommonSetting();
    }

    public void LoadSetting()
    {
        EditFileInfoViewModelOC.Clear();

        var service = App.Current.Services.GetService<SettingService>();

        service.LoadLocalDefaultSetting();
        foreach (var item in service.AllConfig.EditFileInfoList)
        {
            EditFileInfoVM editFileInfo = new(item.FilePath);
            editFileInfo.CreateTime = item.CreateTime;
            editFileInfo.ModifyTime = item.ModifyTime;
            editFileInfo.UpdateInfoAction += Update;

            EditFileInfoViewModelOC.Add(editFileInfo);
        }

        if (EditFileInfoViewModelOC.Count > 0)
        {
            SelectedEditFileInfo = EditFileInfoViewModelOC.First();
        }
    }

    private void Update()
    {
        SaveSetting();
    }

    #endregion Setting
}