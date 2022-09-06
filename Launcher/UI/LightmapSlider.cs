using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToolkitLauncher.UI
{
    public class LightmapSlider : Slider
    {
        /*
         * The Slider class currently updates the contents of the tooltip in two functions
         * OnThumbDragDelta and OnThumbDragDelta
         * The contents is given by GetAutoToolTipNumber, but this is a private function so we can't trivially override it.
         * https://github.com/dotnet/wpf/blob/89d172db0b7a192de720c6cfba5e28a1e7d46123/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Controls/Slider.cs#L903
         * So instead we use reflection to get _autoToolTip and override the functions that update it
         * This is fragile, if it breaks too much just copy the code into the launcher, it's MIT licensed.
         * 
         * Idea taken from https://joshsmithonwpf.wordpress.com/2007/09/14/modifying-the-auto-tooltip-of-a-slider/
         */

        protected override void OnThumbDragDelta(System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            UpdateAutoTooltip();
        }

        protected override void OnThumbDragStarted(System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            UpdateAutoTooltip();
        }

        /// <summary>
        /// Value converted to a lightmap threshold, will change the value of `Value`
        /// </summary>
        [Bindable(true)]
        [Category("Behavior")]
        public double ConvertedValue
        {
            get
            {
                return LinearToThreshold(Value);
            }

            set
            {
                Value = ThresholdToLinear(value);
            }
        }

        /// <summary>
        /// Ticks in lightmap threshold units, will change the value of `Ticks` on assignment
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public DoubleCollection ConvertedTicks
        {
            get
            {
                DoubleCollection converted = new();
                foreach (double tick in Ticks)
                {
                    converted.Add(LinearToThreshold(tick));
                }
                return converted;
            }

            set
            {
                Ticks.Clear();
                foreach (double tick in value)
                {
                    Ticks.Add(ThresholdToLinear(tick));
                }
            }
        }

        public double ScaleBase = 10.0;

        private double LinearToThreshold(double x)
        {
            return (Math.Pow(ScaleBase, x) - 1.0) / (ScaleBase - 1.0);
        }

        private double ThresholdToLinear(double y)
        {
            return Math.Log(y * (ScaleBase - 1.0) + 1.0, ScaleBase);
        }

        private void UpdateAutoTooltip()
        {
            if (_autoToolTip != null)
            {
                _autoToolTip.Content = GetAutoToolTipNumber();
            }
        }

        // START copied from WPF source (Value replaced with convertedValue)
        private string GetAutoToolTipNumber()
        {
            NumberFormatInfo format = (NumberFormatInfo)(NumberFormatInfo.CurrentInfo.Clone());
            format.NumberDecimalDigits = this.AutoToolTipPrecision;
            return this.ConvertedValue.ToString("N", format);
        }
        // END copied

        private ToolTip _autoToolTip => typeof(Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this) as ToolTip;
    }
}
