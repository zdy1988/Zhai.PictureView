using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Zhai.Famil.Common.Mvvm;
using Zhai.Famil.Common.Mvvm.Command;
using Zhai.Famil.Controls;

namespace Zhai.PictureView
{
    internal partial class PictureWindowViewModel : ViewModelBase
    {
        private Folder folder;
        public Folder Folder
        {
            get => folder;
            set => Set(() => Folder, ref folder, value);
        }

        private bool isShowPictureListView = false;
        public bool IsShowPictureListView
        {
            get => isShowPictureListView;
            set => Set(() => IsShowPictureListView, ref isShowPictureListView, value);
        }

        private bool isShowPictureEffectsView = false;
        public bool IsShowPictureEffectsView
        {
            get => isShowPictureEffectsView;
            set => Set(() => IsShowPictureEffectsView, ref isShowPictureEffectsView, value);
        }

        private Picture currentPicture;
        public Picture CurrentPicture
        {
            get => currentPicture;
            set
            {
                if (Set(() => CurrentPicture, ref currentPicture, value))
                {
                    CurrentPictureChanged?.Invoke(this, value);

                    //if (value != null && value.ThumbState == PictureState.Failed)
                    //{
                    //    ThreadPool.QueueUserWorkItem(_ => value.DrawThumb());
                    //}
                }
            }
        }

        private int currentPictureIndex;
        public int CurrentPictureIndex
        {
            get => currentPictureIndex;
            set
            {
                if (Set(() => CurrentPictureIndex, ref currentPictureIndex, value))
                {
                    DisplayedPictureIndex = value + 1;
                }
            }
        }

        private int displayedPictureIndex = 1;
        public int DisplayedPictureIndex
        {
            get => displayedPictureIndex;
            set => Set(() => DisplayedPictureIndex, ref displayedPictureIndex, value);
        }

        private bool isPictureMoving = false;
        public bool IsPictureMoving
        {
            get => isPictureMoving;
            set => Set(() => IsPictureMoving, ref isPictureMoving, value);
        }

        private double rotateAngle = 0.0;
        public double RotateAngle
        {
            get => rotateAngle;
            set => Set(() => RotateAngle, ref rotateAngle, value);
        }

        private double scale = 0.0;
        public double Scale
        {
            get => scale;
            set
            {
                if (Set(() => Scale, ref scale, value))
                {
                    ScaleChanged?.Invoke(this, value);
                }
            }
        }

        private bool isPictureCountMoreThanOne = false;
        public bool IsPictureCountMoreThanOne
        {
            get => isPictureCountMoreThanOne;
            set => Set(() => IsPictureCountMoreThanOne, ref isPictureCountMoreThanOne, value);
        }

        private bool isPictureCarouselPlaing = false;
        public bool IsPictureCarouselPlaing
        {
            get => isPictureCarouselPlaing;
            set
            {
                if (Set(() => IsPictureCarouselPlaing, ref isPictureCarouselPlaing, value))
                {
                    if (Folder.Any())
                    {
                        if (value)
                            PictureCarousel.Instance.Play();
                        else
                            PictureCarousel.Instance.Stop();
                    }
                }
            }
        }

        private bool isShowGallery = false;
        public bool IsShowGallery
        {
            get => isShowGallery;
            set => Set(() => IsShowGallery, ref isShowGallery, value);
        }

        private PictureEffect currentPictureEffect;
        public PictureEffect CurrentPictureEffect
        {
            get => currentPictureEffect;
            set => Set(() => CurrentPictureEffect, ref currentPictureEffect, value);
        }


        public ObservableCollection<PictureEffect> Effects { get; }


        public PictureWindowViewModel()
        {
            var effects = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.FullName.Contains("Zhai.PictureView.ShaderEffects"))
                .Select(t => new PictureEffect(t.Name, System.Activator.CreateInstance(t) as ShaderEffect));

            Effects = new ObservableCollection<PictureEffect>(effects);

            Effects.Insert(0, new PictureEffect("Original", null));
        }

        #region Commands

        public RelayCommand ExecuteTogglePictureEffectsViewCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            IsShowPictureEffectsView = !IsShowPictureEffectsView;

        }, () => CurrentPicture != null && CurrentPicture.IsLoaded)).Value;

        public RelayCommand ExecuteCopyCurrentPictureSourceCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            System.Windows.Clipboard.SetImage(CurrentPicture.PictureSource);

        }, () => CurrentPicture != null && CurrentPicture.IsLoaded)).Value;

        public RelayCommand ExecuteSaveImageCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            

        }, () => CurrentPicture != null && CurrentPicture.IsLoaded)).Value;

        public RelayCommand ExecuteSaveAsImageCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {
            if (Famil.Win32.CommonDialog.SaveFileDialog(Folder.Current.FullName, "", "另存为...", "", CurrentPicture.Extension, default, out string filename))
            {
                var isSuccess = await CurrentPicture.SaveAsync(filename);

                if (isSuccess)
                {
                    SendNotificationMessage("保存成功！");
                }
                else
                {
                    SendNotificationMessage("保存失败！");
                }
            }

        }, () => CurrentPicture != null && CurrentPicture.IsLoaded)).Value;

        public RelayCommand ExecuteCopyCurrentPicturePathCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            System.Windows.Clipboard.SetText(CurrentPicture.PicturePath);

        })).Value;

        public RelayCommand ExecuteOpenTheFolderCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            var info = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
            {
                Arguments = $"/select,{CurrentPicture.PicturePath}"
            };
            System.Diagnostics.Process.Start(info);

        }, () => CurrentPicture != null)).Value;

        #endregion



        public event EventHandler<Picture> CurrentPictureChanged;

        public event EventHandler<Double> ScaleChanged;

        public override void Cleanup()
        {
            base.Cleanup();

            Folder.Cleanup();
        }
    }
}
