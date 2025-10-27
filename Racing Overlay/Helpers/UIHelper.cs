using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml.Linq;
using static iRacingSDK.SessionData._DriverInfo;

namespace RacingOverlay.Helpers
{
    public class UIHelper
    {
        public static void SetCellFormat(UIElement cell, int columnIndex, int columnWidth, int rowIndex)
        {
            Grid.SetColumnSpan(cell, columnWidth);
            Grid.SetColumn(cell, columnIndex);
            Grid.SetRow(cell, rowIndex);
        }

        public static TextBlock CreateTextBlock(
            Thickness? padding,
            TextAlignment textAlignment = TextAlignment.Center,
            HorizontalAlignment horizalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            double fontSize = 16)
        {
            var textBlock = new TextBlock();
            textBlock.FontSize = fontSize;
            textBlock.TextAlignment = textAlignment;
            textBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Padding = padding ?? new Thickness(3);
            return textBlock;
        }

        public static void AddOrInsertChild(Grid grid, UIElement element, int cellIndex)
        {
            if (cellIndex >= grid.Children.Count)
                grid.Children.Add(element);
            else
                grid.Children.Insert(cellIndex, element);
        }

        public static Border DesignSafetyRating(int rowIndex, Driver driver, Thickness? thickness, int fontSize)
        {
            var outerBorder = new Border();
            Color myShadowColor = Color.FromArgb(0, 0, 0, 0);
            if (rowIndex % 2 == 1)
                outerBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#521439");
            else
                outerBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#280f1d");

            outerBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            outerBorder.VerticalAlignment = VerticalAlignment.Stretch;
            outerBorder.Padding = thickness ?? new Thickness(3);

            var licenseShadowEffect = new DropShadowEffect();
            licenseShadowEffect.Color = myShadowColor;
            licenseShadowEffect.Direction = 330;
            licenseShadowEffect.ShadowDepth = 2;
            licenseShadowEffect.Opacity = 0.8;

            var innerBorder = new Border();
            innerBorder.VerticalAlignment = VerticalAlignment.Stretch;
            innerBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            innerBorder.CornerRadius = new CornerRadius(5);
            innerBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(driver.SafetyRating.Item2.Replace("0x", "#"));
            innerBorder.Effect = licenseShadowEffect;

            var textDropShadowEffect = new DropShadowEffect();
            textDropShadowEffect.Color = myShadowColor;
            textDropShadowEffect.Direction = 315;
            textDropShadowEffect.ShadowDepth = 1;
            textDropShadowEffect.BlurRadius = 3;
            textDropShadowEffect.Opacity = 1;

            var safetyRating = new TextBlock();
            safetyRating.Tag = "SafetyRating";
            safetyRating.Text = driver.SafetyRating.Item1.Substring(2, driver.SafetyRating.Item1.Length - 3);
            safetyRating.HorizontalAlignment = HorizontalAlignment.Center;
            safetyRating.VerticalAlignment = VerticalAlignment.Center;
            safetyRating.Effect = textDropShadowEffect;
            FormatSafetyRatingTextBlock(safetyRating, driver, fontSize);

            outerBorder.Child = innerBorder;
            innerBorder.Child = safetyRating;
            return outerBorder;
        }

        private static void FormatSafetyRatingTextBlock(TextBlock textBlock, Driver driver, int fontSize)
        {
            textBlock.FontSize = fontSize;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Foreground = Brushes.White;
            textBlock.TextTrimming = TextTrimming.None;
            textBlock.Background = Brushes.Transparent;

        }
    }
}
