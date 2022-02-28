using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;

namespace Zhai.PictureView
{
    internal class PictureEffect
    {
        public string Name { get; set; }

        public Effect Effect { get; set; }

        public PictureEffect(string name, Effect effect)
        { 
            this.Name = name;
            this.Effect = effect;
        }
    }
}
