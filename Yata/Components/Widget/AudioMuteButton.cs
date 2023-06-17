using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Yata.Components.Widget
{
    class AudioMuteButton : OwnerDrawWidget
    {
        IEndpointMuteController controller;
        Image img;
        Rectangle normal;
        Rectangle mute;
        bool needDraw = true;
        bool prev;

        public AudioMuteButton(AudioVolumeController ctrl, bool isCapure) : base(WidgetScaleMode.Fixed, 48, 48)
        {
            img = CommonResource.LoadYataImage("Volume.png");
            if (isCapure)
            {
                normal = new Rectangle(0, 48, 48, 48);
                mute = new Rectangle(48, 48, 48, 48);
                controller = new CaptureMuteController(ctrl);
            }
            else
            {
                normal = new Rectangle(0, 0, 48, 48);
                mute = new Rectangle(48, 0, 48, 48);

                controller = new RenderMuteController(ctrl);

            }
        }

        public override bool Update()
        {
            var ret = needDraw;
            needDraw = false;

            var m = controller.Mute;
            if (m != prev) ret = true;
            prev = m;
            return ret;
        }

        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);
            graphics.DrawImage(img, new Rectangle(0, 0, Width, Height), prev ? mute : normal, GraphicsUnit.Pixel);
        }

        public override void Click(int x, int y)
        {
            controller.Mute = !controller.Mute;
            needDraw = true;
        }

    }

    interface IEndpointMuteController
    {
        bool Mute { get; set; }
    }

    class RenderMuteController : IEndpointMuteController
    {
        AudioVolumeController controller;

        public RenderMuteController(AudioVolumeController ctrl)
        {
            controller = ctrl;
        }

        public bool Mute
        {
            get => controller.Mute;
            set => controller.Mute = value;
        }
    }
    class CaptureMuteController : IEndpointMuteController
    {
        AudioVolumeController controller;

        public CaptureMuteController(AudioVolumeController ctrl)
        {
            controller = ctrl;
        }

        public bool Mute
        {
            get => controller.CaptureMute;
            set => controller.CaptureMute = value;
        }
    }

}
