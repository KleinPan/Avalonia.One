﻿using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace One.Toolbox.Services;

public interface IFilesService
{
    public Task<IStorageFile?> OpenFileAsync();

    public Task<IStorageFile?> SaveFileAsync();
}

public class FilesService : IFilesService
{
    private readonly Window _target;

    public FilesService(Window target)
    {
        _target = target;
    }

    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync()
    {
        return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Text File"
        });
    }
}