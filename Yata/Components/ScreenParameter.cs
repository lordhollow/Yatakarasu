using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Yata.Components
{
    public class ScreenParameter
    {
        public int ScreenWidth { get; set; } = 1280;  //1024
        public int ScreenHeight { get; set; } = 800;  //600
        public float ScreenScale { get; set; } = 1.0f;
    }
}
