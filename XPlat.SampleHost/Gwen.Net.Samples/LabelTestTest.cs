﻿using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;

namespace Gwen.Net.Tests.Components
{
    [UnitTest(Category = "Non-Interactive", Order = 100)]
    public class LabelTestTest : GUnit
    {
        private readonly Font font1;
        private readonly Font font2;
        private readonly Font font3;

        public LabelTestTest(ControlBase parent) : base(parent)
        {
            VerticalLayout layout = new VerticalLayout(this);
            {
                Label label = new Label(layout);
                label.Text = "Standard label (not autosized)";
            }
            {
                Label label = new Label(layout);
                label.Text = "Chinese: \u4E45\u6709\u5F52\u5929\u613F \u7EC8\u8FC7\u9B3C\u95E8\u5173";
            }
            {
                Label label = new Label(layout);
                label.Text = "Japanese: \u751F\u3080\u304E\u3000\u751F\u3054\u3081\u3000\u751F\u305F\u307E\u3054";
            }
            {
                Label label = new Label(layout);
                label.Text = "Korean: \uADF9\uC9C0\uD0D0\uD5D8\u3000\uD611\uD68C\uACB0\uC131\u3000\uCCB4\uACC4\uC801\u3000\uC5F0\uAD6C";
            }
            {
                Label label = new Label(layout);
                label.Text = "Hindi: \u092F\u0947 \u0905\u0928\u0941\u091A\u094D\u091B\u0947\u0926 \u0939\u093F\u0928\u094D\u0926\u0940 \u092E\u0947\u0902 \u0939\u0948\u0964";
            }
            {
                Label label = new Label(layout);
                label.Text = "Arabic: \u0627\u0644\u0622\u0646 \u0644\u062D\u0636\u0648\u0631 \u0627\u0644\u0645\u0624\u062A\u0645\u0631 \u0627\u0644\u062F\u0648\u0644\u064A";
            }
            {
                Label label = new Label(layout);
                label.MouseInputEnabled = true; // needed for tooltip
                label.Text = "Wow, Coloured Text (and tooltip)";
                label.TextColor = Color.Blue;
                label.SetToolTipText("I'm a tooltip");
                font3 = new Font(Skin.Renderer, "Motorwerk", 20);
                ((Label)label.ToolTip).Font = font3;
            }
            {
                Label label = new Label(layout);
                label.Text = "Coloured Text With Alpha";
                label.TextColor = new Color(100, 0, 0, 255);
            }
            {
                // Note that when using a custom font, this font object has to stick around
                // for the lifetime of the label. Rethink, or is that ideal?
                font1 = new Font(Skin.Renderer);
                //font1.FaceName = "Comic Sans MS";
                font1.FaceName = "sans";
                font1.Size = 25;

                Label label = new Label(layout);
                label.Text = "Custom Font (Comic Sans 25)";
                label.Font = font1;
            }
            {
                font2 = new Font(Skin.Renderer, "sans"/*"French Script MT"*/, 35);

                Label label = new Label(layout);
                label.Font = font2;
                label.Text = "Custom Font (French Script MT 35)";
            }

            // alignment test
            {
                Label txt = new Label(layout);
                txt.Text = "Alignment test";

                GridLayout outer = new GridLayout(layout);
                outer.ColumnCount = 3;

                Label l11 = new Label(outer);
                l11.Size = new Size(50, 50);
                l11.Text = "TL";
                l11.Alignment = Alignment.Top | Alignment.Left;

                Label l12 = new Label(outer);
                l12.Size = new Size(50, 50);
                l12.Text = "T";
                l12.Alignment = Alignment.Top | Alignment.CenterH;

                Label l13 = new Label(outer);
                l13.Size = new Size(50, 50);
                l13.Text = "TR";
                l13.Alignment = Alignment.Top | Alignment.Right;

                Label l21 = new Label(outer);
                l21.Size = new Size(50, 50);
                l21.Text = "L";
                l21.Alignment = Alignment.Left | Alignment.CenterV;

                Label l22 = new Label(outer);
                l22.Size = new Size(50, 50);
                l22.Text = "C";
                l22.Alignment = Alignment.CenterH | Alignment.CenterV;

                Label l23 = new Label(outer);
                l23.Size = new Size(50, 50);
                l23.Text = "R";
                l23.Alignment = Alignment.Right | Alignment.CenterV;

                Label l31 = new Label(outer);
                l31.Size = new Size(50, 50);
                l31.Text = "BL";
                l31.Alignment = Alignment.Bottom | Alignment.Left;

                Label l32 = new Label(outer);
                l32.Size = new Size(50, 50);
                l32.Text = "B";
                l32.Alignment = Alignment.Bottom | Alignment.CenterH;

                Label l33 = new Label(outer);
                l33.Size = new Size(50, 50);
                l33.Text = "BR";
                l33.Alignment = Alignment.Bottom | Alignment.Right;

                outer.DrawDebugOutlines = true;
            }
        }

        protected override Size Measure(Size availableSize)
        {
            return base.Measure(availableSize);
        }

        public override void Dispose()
        {
            font1.Dispose();
            font2.Dispose();
            font3.Dispose();
            base.Dispose();
        }
    }
}