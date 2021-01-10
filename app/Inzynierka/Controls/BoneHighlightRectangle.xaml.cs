using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Toolkit.Uwp.UI.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Inzynierka.Controls
{
    public sealed partial class BoneHighlightRectangle : UserControl
    {
        public string DisplayedText
        {
            get { return (string)GetValue(DisplayedTextProperty); }
            set { SetValue(DisplayedTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayedTextProperty =
            DependencyProperty.Register("DisplayedText", typeof(string), typeof(BoneHighlightRectangle), new PropertyMetadata(0));



        public float X
        {
            get { return (float)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(float), typeof(BoneHighlightRectangle), new PropertyMetadata(0));



        public float Y
        {
            get { return (float)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(float), typeof(BoneHighlightRectangle), new PropertyMetadata(0));



        public SolidColorBrush RectangleFillColor
        {
            get { return (SolidColorBrush)GetValue(RectangleFillColorProperty); }
            set { SetValue(RectangleFillColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RectangleFillColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RectangleFillColorProperty =
            DependencyProperty.Register("RectangleFillColor", typeof(SolidColorBrush), typeof(BoneHighlightRectangle), new PropertyMetadata(0));



        public SolidColorBrush ForegroundColor
        {
            get { return (SolidColorBrush)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForegroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register("ForegroundColor", typeof(SolidColorBrush), typeof(BoneHighlightRectangle), new PropertyMetadata(0));




        public DragEventHandler DragOverHandler
        {
            get { return (DragEventHandler)GetValue(DragOverHandlerProperty); }
            set { SetValue(DragOverHandlerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragOverHandler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragOverHandlerProperty =
            DependencyProperty.Register("DragOverHandler", typeof(DragEventHandler), typeof(BoneHighlightRectangle), new PropertyMetadata(0));


        public TypedEventHandler<UIElement, DragStartingEventArgs> DragStartingHandler
        {
            get { return (TypedEventHandler<UIElement, DragStartingEventArgs>)GetValue(DragStartingHandlerProperty); }
            set { SetValue(DragStartingHandlerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragStartingHandler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragStartingHandlerProperty =
            DependencyProperty.Register("DragStartingHandler", typeof(TypedEventHandler<UIElement, DragStartingEventArgs>), typeof(BoneHighlightRectangle), new PropertyMetadata(0));

        private static Color fillColor = Color.FromArgb(50, 255, 255, 255);
        private static SolidColorBrush fillLight = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
        private static RadialGradientBrush fillFocus = new RadialGradientBrush()
        {
            RadiusX = 1,
            RadiusY = 1,
            GradientStops = new GradientStopCollection()
                    {
                        new GradientStop() {Color = fillColor, Offset = 0 },
                        new GradientStop() {Color = Color.FromArgb(0, 255, 0, 0), Offset = 0.5 }
                    },
            AlphaMode = AlphaMode.Premultiplied
        };
        private static SolidColorBrush foregroundLight = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
        private static SolidColorBrush foregroundFocus = new SolidColorBrush(Color.FromArgb(200, 255, 255, 250));



        public BoneHighlightRectangle()
        {
            this.InitializeComponent();
        }

        private void PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            rect.Fill = fillFocus;
            diplayedTextBlock.Foreground = foregroundFocus;
        }

        private void PointerExited(object sender, PointerRoutedEventArgs e)
        {
            rect.Fill = fillLight;
            diplayedTextBlock.Foreground = foregroundLight;
        }
    }
}
