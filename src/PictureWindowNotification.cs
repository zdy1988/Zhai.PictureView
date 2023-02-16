using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhai.Famil.Common.Mvvm;

namespace Zhai.PictureView
{
    internal partial class PictureWindowViewModel : ViewModelBase
    {
        public void SendNotificationMessage(string message)
        {
            if (!String.IsNullOrWhiteSpace(message))
            {
                NotificationMessage = "";
                NotificationMessage = message;
            }
        }

        private string notificationMessage;
        public string NotificationMessage
        {
            get { return notificationMessage; }
            set { Set(() => NotificationMessage, ref notificationMessage, value); }
        }
    }
}
