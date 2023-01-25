using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BlogWrite.Core.Contracts.Services;

public interface IFileDialogService
{

    Task<StorageFile?> GetOpenOpmlFileDialog(IntPtr hwnd);

    Task<StorageFile?> GetSaveOpmlFileDialog(IntPtr hwnd);
}
