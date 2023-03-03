using Windows.Storage;

namespace BlogWrite.Core.Contracts.Services;

public interface IFileDialogService
{

    Task<StorageFile?> GetOpenOpmlFileDialog(IntPtr hwnd);

    Task<StorageFile?> GetSaveOpmlFileDialog(IntPtr hwnd);
}
