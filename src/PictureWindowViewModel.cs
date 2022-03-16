using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
            set => SetProperty(ref folder, value);
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

        private bool canPictureCarouselPlay = false;
        public bool CanPictureCarouselPlay
        {
            get => canPictureCarouselPlay;
            set => SetProperty(ref canPictureCarouselPlay, value);
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

        private bool isShowGallery = false;
        public bool IsShowGallery
        {
            get => isShowGallery;
            set => SetProperty(ref isShowGallery, value);
        }

        private PictureEffect currentPictureEffect;
        public PictureEffect CurrentPictureEffect
        {
            get => currentPictureEffect;
            set => SetProperty(ref currentPictureEffect, value);
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



        public event EventHandler<Picture> CurrentPictureChanged;

        public event EventHandler<Double> ScaleChanged;


        public override void Clean()
        {
            Folder.Clean();
        }
    }
}
