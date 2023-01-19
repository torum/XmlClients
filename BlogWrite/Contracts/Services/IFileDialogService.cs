using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BlogWrite.Contracts.Services;

public interface IFileDialogService
{
    Task<StorageFile?> GetOpenOpmlFileDialog();

    Task<StorageFile?> GetSaveOpmlFileDialog();
}
