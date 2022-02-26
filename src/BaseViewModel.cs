using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Zhai.PictureView
{
    internal abstract class BaseViewModel : INotifyPropertyChanged, IClean
    {
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

        public abstract void Clean();
    }

    internal interface IClean
    { 
        void Clean();
    }
}
