using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zhai.Famil.Common.Mvvm;
using Zhai.Famil.Common.Mvvm.Command;
using MessageBox = Zhai.Famil.Dialogs.MessageBox;
using ConfirmBox = Zhai.Famil.Dialogs.ConfirmBox;

namespace Zhai.PictureView
{
    internal class SaveAsWindowViewModel : ViewModelBase
    {
        #region Properties

        private int quality = 100;
        public int Quality
        {
            get { return quality; }
            set { Set(() => Quality, ref quality, value); }
        }

        private int resizeWidth;
        public int ResizeWidth
        {
            get { return resizeWidth; }
            set
            {
                if (value < MinWidth) value = MinWidth;
                else if (value > MaxWidth) value = MaxWidth;

                if (Set(() => ResizeWidth, ref resizeWidth, value))
                {
                    if (IsLockedResizePercentage)
                    {
                        resizePercentage = AdjustResizePercentage((int)((double)value / (double)width * 100.0), out bool isAdjusted);
                        resizeHeight = (int)((double)height * (double)resizePercentage / 100.0);
                        base.RaisePropertyChanged(nameof(ResizePercentage));
                        base.RaisePropertyChanged(nameof(ResizeHeight));

                        if (isAdjusted)
                        {
                            resizeWidth = (int)((double)width * (double)resizePercentage / 100.0);
                            base.RaisePropertyChanged(nameof(ResizeWidth));
                        }
                    }
                }
            }
        }

        private int resizeHeight;
        public int ResizeHeight
        {
            get { return resizeHeight; }
            set
            {
                if (value < MinHeight) value = MinHeight;
                else if (value > MaxHeight) value = MaxHeight;

                if (Set(() => ResizeHeight, ref resizeHeight, value))
                {
                    if (IsLockedResizePercentage)
                    {
                        resizePercentage = AdjustResizePercentage((int)((double)value / (double)height * 100.0), out bool isAdjusted);
                        resizeWidth = (int)((double)width * (double)resizePercentage / 100.0);
                        base.RaisePropertyChanged(nameof(ResizeWidth));
                        base.RaisePropertyChanged(nameof(ResizePercentage));

                        if (isAdjusted)
                        {
                            resizeHeight = (int)((double)height * (double)resizePercentage / 100.0);
                            base.RaisePropertyChanged(nameof(ResizeHeight));
                        }
                    }
                }
            }
        }

        private int resizePercentage = 100;
        public int ResizePercentage
        {
            get { return resizePercentage; }
            set
            {
                if (Set(() => ResizePercentage, ref resizePercentage, value))
                {
                    if (IsLockedResizePercentage)
                    {
                        resizePercentage = AdjustResizePercentage(value, out bool isAdjusted);

                        resizeWidth = (int)((double)width * (double)resizePercentage / 100.0);
                        resizeHeight = (int)((double)height * (double)resizePercentage / 100.0);
                        base.RaisePropertyChanged(nameof(ResizeWidth));
                        base.RaisePropertyChanged(nameof(ResizeHeight));

                        if (isAdjusted)
                        {
                            base.RaisePropertyChanged(nameof(ResizePercentage));
                        }
                    }
                }
            }
        }

        private bool isLockedResizePercentage = true;
        public bool IsLockedResizePercentage
        {
            get { return isLockedResizePercentage; }
            set
            {
                if (Set(() => IsLockedResizePercentage, ref isLockedResizePercentage, value))
                {
                    if (value)
                    {
                        resizeWidth = (int)((double)width * (double)resizePercentage / 100.0);
                        resizeHeight = (int)((double)height * (double)resizePercentage / 100.0);
                        base.RaisePropertyChanged(nameof(ResizeWidth));
                        base.RaisePropertyChanged(nameof(ResizeHeight));
                    }
                }
            }
        }

        private long fileSize;
        public long FileSize
        {
            get { return fileSize; }
            set { Set(() => FileSize, ref fileSize, value); }
        }

        private long? customFileSize;
        public long? CustomFileSize
        {
            get { return customFileSize; }
            set
            {
                if (value <= 1) value = 1;

                Set(() => CustomFileSize, ref customFileSize, value);
            }
        }

        private string customFileSizeUnit;
        public string CustomFileSizeUnit
        {
            get { return customFileSizeUnit; }
            set { Set(() => CustomFileSizeUnit, ref customFileSizeUnit, value); }
        }

        private bool isCustomFileSize;
        public bool IsCustomFileSize
        {
            get { return isCustomFileSize; }
            set
            {
                if (Set(() => IsCustomFileSize, ref isCustomFileSize, value))
                {
                    this.Extensions = GetExtensions(value);
                    this.Extension = this.extensions.First();
                }
            }
        }

        private string folderPath;
        public string FolderPath
        {
            get { return folderPath; }
            set { Set(() => FolderPath, ref folderPath, value); }
        }

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set { Set(() => FileName, ref fileName, value); }
        }

        private string extension;
        public string Extension
        {
            get { return extension; }
            set
            {
                if (Set(() => Extension, ref extension, value))
                {
                    base.RaisePropertyChanged(nameof(IsCanCustomFileSize));
                }
            }
        }

        private string[] extensions;
        public string[] Extensions
        {
            get { return extensions; }
            set { Set(() => Extensions, ref extensions, value); }
        }

        private bool isSaving;
        public bool IsSaving
        {
            get { return isSaving; }
            set { Set(() => IsSaving, ref isSaving, value); }
        }

        private bool isSavedToShow;
        public bool IsSavedToShow
        {
            get { return isSavedToShow; }
            set { Set(() => IsSavedToShow, ref isSavedToShow, value); }
        }

        #endregion

        private readonly Picture currentPicture;

        private readonly int width;
        private readonly int height;

        private readonly double minRatio = 0.01;
        private readonly double maxRatio = 4.0;

        private int MinWidth => (int)((double)width * minRatio);
        private int MinHeight => (int)((double)height * minRatio);
        private int MaxWidth => (int)((double)width * maxRatio);
        private int MaxHeight => (int)((double)height * maxRatio);

        public static string[] CustomFileSizeUnits => new string[] { "KB", "MB" };

        // 至少大于 5KB 才可以自定义压缩
        public bool IsCanCustomFileSize => this.FileSize > 5 * 1024;

        public SaveAsWindowViewModel(Picture picture)
        {
            this.currentPicture = picture;
            this.width = (int)picture.PixelWidth;
            this.height = (int)picture.PixelHeight;
            this.resizeWidth = width;
            this.resizeHeight = height;

            this.folderPath = Path.GetDirectoryName(picture.PicturePath);
            this.fileName = GetSaveAsFileName();

            this.extensions = GetExtensions();
            this.extension = this.extensions.First();

            this.fileSize = picture.Size;
            this.customFileSizeUnit = CustomFileSizeUnits.First();
        }

        #region Methods

        public int AdjustResizePercentage(int value, out bool isAdjusted)
        {
            if (value > maxRatio * 100)
            {
                isAdjusted = true;
                return 400;
            }
            else if (value < minRatio * 100)
            {
                isAdjusted = true;
                return 1;
            }
            else
            {
                isAdjusted = false;
                return value;
            }
        }

        public string GetSaveAsFileName()
        {
            var folderPath = Path.GetDirectoryName(currentPicture.PicturePath);
            var fileName = Path.GetFileNameWithoutExtension(currentPicture.PicturePath);
            var extension = currentPicture.Extension;

            for (var i = 1; i < 100; i++)
            {
                var name = $"{fileName}({i})";

                var file = Path.Combine(folderPath, $"{name}{extension}");

                if (!File.Exists(file))
                {
                    return name;
                }
            }

            return fileName;
        }

        public string[] GetExtensions(bool isDefault = false)
        {
            var ext = currentPicture.Extension[1..].ToUpper();

            var exts = new List<string>();

            if (!isDefault)
            {
                exts.Add(ext);
            }

            foreach (var item in new string[] { "JPG", "PNG", "BMP" })
            {
                exts.Add(item);
            }

            return exts.Distinct().ToArray();
        }

        public Task<bool> SaveAsync(out string targetPath)
        {
            targetPath = Path.Combine(FolderPath, $"{FileName}.{Extension}");

            async Task<bool> Save(string targetPath)
            {

                IsSaving = true;

                await Task.Delay(1000);

                bool isSuccess;

                var bytes = currentPicture.ReadAllBytes();

                if (IsCustomFileSize && CustomFileSize != null && CustomFileSize > 0)
                {
                    var targetLen = CustomFileSizeUnit == "KB" ? CustomFileSize.Value * 1024L : CustomFileSize.Value * 1024L * 1024L;

                    isSuccess = await ImageDecoder.SaveImageAsync(bytes, targetPath, targetLen, Quality, ResizeWidth, ResizeHeight, !IsLockedResizePercentage);
                }
                else
                {
                    isSuccess = await ImageDecoder.SaveImageAsync(bytes, targetPath, Quality, ResizeWidth, ResizeHeight, !IsLockedResizePercentage);
                }

                IsSaving = false;

                return isSuccess;
            }

            if (File.Exists(targetPath))
            {
                if (ConfirmBox.Show("当前文件已存在！要将此现有文件覆盖吗？") == true)
                {
                    return Save(targetPath);
                }
                else
                {
                    return Task.FromResult(false);
                }
            }

            return Save(targetPath);
        }

        #endregion

        #region Commands

        public RelayCommand ExecuteResetResizePercentageCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            ResizePercentage = 100;
            IsLockedResizePercentage = true;

        })).Value;

        public RelayCommand ExecuteAddResizePercentageCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            ResizePercentage = AdjustResizePercentage(ResizePercentage + 10, out _);

        })).Value;

        public RelayCommand ExecuteSubtractResizePercentageCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            ResizePercentage = AdjustResizePercentage(ResizePercentage - 10, out _);

        })).Value;

        public RelayCommand ExecuteSelectFolderCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            if (Zhai.Famil.Win32.CommonDialog.OpenFolderDialog(null, String.IsNullOrEmpty(FolderPath) ? String.Empty : Path.GetDirectoryName(FolderPath), "", "", false, out string filename) == true)
            {
                FolderPath = filename;
            }

        })).Value;

        public RelayCommand ExecuteSaveImageCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {
            var isSuccess = await SaveAsync(out string targetPath);

            if (isSuccess && IsSavedToShow)
            {
                var info = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
                {
                    Arguments = $"/select,{targetPath}"
                };
                System.Diagnostics.Process.Start(info);
            }

        }, () => currentPicture != null && currentPicture.IsLoaded)).Value;

        #endregion
    }
}
