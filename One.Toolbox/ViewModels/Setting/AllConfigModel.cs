namespace One.Toolbox.ViewModels.Setting;

public class AllConfigModel
{
    public SettingModel Setting { get; set; } = new SettingModel();
    public List<EditFileInfo> EditFileInfoList { get; set; } = new();
}

public class EditFileInfo
{
    public string FileName { get; set; }

    public string FilePath { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime ModifyTime { get; set; }
}