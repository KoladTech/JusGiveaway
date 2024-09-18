using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using Color = Microsoft.Maui.Graphics.Color;

namespace JusGiveaway
{
    [TypeConverter(typeof(CustomBarGraphDrawableConverter))]
    public class CustomBarGraphDrawable : View, IDrawable
    {
        public static readonly BindableProperty EvenPercentageProperty = BindableProperty.Create(
            nameof(EvenPercentage),
            typeof(float),
            typeof(CustomBarGraphDrawable),
            0.5f);

        public float EvenPercentage
        {
            get { return (float)GetValue(EvenPercentageProperty); }
            set { SetValue(EvenPercentageProperty, value); }
        }

        public static readonly BindableProperty OddPercentageProperty = BindableProperty.Create(
            nameof(OddPercentage),
            typeof(float),
            typeof(CustomBarGraphDrawable),
            0.5f);

        public float OddPercentage
        {
            get { return (float)GetValue(OddPercentageProperty); }
            set { SetValue(OddPercentageProperty, value); }
        }

        // Default constructor
        public CustomBarGraphDrawable()
        {
            // Initialize properties with default values
            EvenPercentage = 0.5f; // Initialize to 50%
            OddPercentage = 0.5f; // Initialize to 50%
        }

        public CustomBarGraphDrawable(float evenPercentage, float oddPercentage)
        {
            EvenPercentage = evenPercentage;
            OddPercentage = oddPercentage;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)//, RectF headsBar, RectF tailsBar
        {

            var totalWidth = 200;
            var totalHeight = 10;
            var screenWidth = (float)DeviceDisplay.MainDisplayInfo.Width;

            //var evenWidth = 60;
            //var oddWidth = 60;

            //var totalWidth = dirtyRect.Width;
            //var totalHeight = dirtyRect.Height;
            var evenWidth = totalWidth * EvenPercentage;
            var oddWidth = totalWidth * OddPercentage;

            var startX = (screenWidth/4) - totalWidth;
            //var startX = (totalWidth - evenWidth - oddWidth) / 2;

            // Draw green portion (heads)
            var blueRect = new RectF(startX, 0, evenWidth, totalHeight);
            canvas.FillColor = Color.FromRgb(0, 255, 0);
            canvas.FillRectangle(blueRect);

            // Draw orange portion (tails)
            var redRect = new RectF(startX+evenWidth, 0, oddWidth, totalHeight);
            canvas.FillColor = Color.FromRgb(255, 165, 0);
            canvas.FillRectangle(redRect);
        }
    }

    public class CustomBarGraphDrawableConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string strValue)
            {
                // Parse the string and create CustomBarGraphDrawable object
                // Example: return new CustomBarGraphDrawable(strValue);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }


}

