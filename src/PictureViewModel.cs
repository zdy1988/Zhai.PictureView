using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Zhai.PictureView
{
    internal class PictureViewModel : INotifyPropertyChanged
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

        private int displayedPictureIndex;
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


        public event EventHandler<Picture> CurrentPictureChanged;
        public event EventHandler<Double> ScaleChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetProperty<T>(ref T storage, T value, bool isCheckEquals = true, [CallerMemberName] string propertyName = null)
        {
            if (isCheckEquals)
                if (object.Equals(storage, value)) { return false; }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
