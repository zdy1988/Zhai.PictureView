using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace Zhai.PictureView
{
    internal class PictureWindowViewModel : BaseViewModel
    {
        private Folder folder;
        public Folder Folder
        {
            get => folder;
            set
            {
                if (SetProperty(ref folder, value))
                {
                    IsShowPictureListView = folder?.Count > 1;
                }
            }
        }

        private bool isShowPictureListView = false;
        public bool IsShowPictureListView
        {
            get => isShowPictureListView;
            set => SetProperty(ref isShowPictureListView, value);
        }

        private bool isShowPictureEditView = false;
        public bool IsShowPictureEditView
        {
            get => isShowPictureEditView;
            set => SetProperty(ref isShowPictureEditView, value);
        }

        private Picture currentPicture;
        public Picture CurrentPicture
        {
            get => currentPicture;
            set
            {
                if (SetProperty(ref currentPicture, value))
                {
                    CurrentPictureChanged?.Invoke(this, value);
                }
            }
        }

        private int currentPictureIndex;
        public int CurrentPictureIndex
        {
            get => currentPictureIndex;
            set
            {
                if (SetProperty(ref currentPictureIndex, value))
                {
                    DisplayedPictureIndex = value + 1;
                }
            }
        }

        private int displayedPictureIndex = 1;
        public int DisplayedPictureIndex
        {
            get => displayedPictureIndex;
            set => SetProperty(ref displayedPictureIndex, value);
        }

        private bool isPictureMoving = false;
        public bool IsPictureMoving
        {
            get => isPictureMoving;
            set => SetProperty(ref isPictureMoving, value);
        }

        private double rotateAngle = 0.0;
        public double RotateAngle
        {
            get => rotateAngle;
            set => SetProperty(ref rotateAngle, value);
        }

        private double scale = 1.0;
        public double Scale
        {
            get => scale;
            set
            {
                if (SetProperty(ref scale, value))
                {
                    ScaleChanged?.Invoke(this, value);
                }
            }
        }

        private bool isPictureCarouselPlaing = false;
        public bool IsPictureCarouselPlaing
        {
            get => isPictureCarouselPlaing;
            set
            {
                if (SetProperty(ref isPictureCarouselPlaing, value))
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

        private KeyValuePair<String, ShaderEffect> currentPictureEffect;
        public KeyValuePair<String, ShaderEffect> CurrentPictureEffect
        {
            get => currentPictureEffect;
            set => SetProperty(ref currentPictureEffect, value);
        }


        public Dictionary<String, ShaderEffect> Effects { get; }

        public PictureWindowViewModel()
        {
            Effects = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.FullName.Contains("Zhai.PictureView.ShaderEffects"))
                .ToDictionary(x => x.Name, x => System.Activator.CreateInstance(x) as System.Windows.Media.Effects.ShaderEffect);

            Effects.Add("Original", null);
        }

        public event EventHandler<Picture> CurrentPictureChanged;

        public event EventHandler<Double> ScaleChanged;


        public override void Clean()
        {
            Folder.Clean();
        }
    }
}
