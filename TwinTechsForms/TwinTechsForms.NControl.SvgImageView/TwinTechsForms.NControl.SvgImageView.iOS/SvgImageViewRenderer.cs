using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using NGraphics;
using TwinTechsForms.NControl;
using TwinTechsForms.NControl.iOS;
using Size = NGraphics.Size;
using System.Runtime.InteropServices;
using System.Drawing;

[assembly: ExportRenderer(typeof(SvgImageView), typeof(SvgImageViewRenderer))]
namespace TwinTechsForms.NControl.iOS {
    [Preserve(AllMembers = true)]
    public class SvgImageViewRenderer : ImageRenderer {
        /// <summary>
        ///   Used for registration with dependency service
        /// </summary>
        public new static void Init() {
            var temp = DateTime.Now;
        }

        SvgImageView FormsControl {
            get {
                return Element as SvgImageView;
            }
        }

        static Func<Size, double, IImageCanvas> CreatePlatformImageCanvas = (size, scale) => new ApplePlatform().CreateImageCanvas(size, scale);

        public override void Draw(CGRect rect) {
            base.Draw(rect);

            if (FormsControl != null) {
                using (CGContext context = UIGraphics.GetCurrentContext()) {
                    context.SetAllowsAntialiasing(true);
                    context.SetShouldAntialias(true);
                    context.SetShouldSmoothFonts(true);

                    var finalCanvas = FormsControl.RenderSvgToCanvas(new Size(rect.Width, rect.Height), UIScreen.MainScreen.Scale, CreatePlatformImageCanvas);
                    var image = finalCanvas.GetImage();
                    var uiImage = image.GetUIImage();
					 
					FormsControl.BitmapImage = image;
					FormsControl.Canvas = finalCanvas;

                    Control.Image = uiImage;
                }
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e) {
            base.OnElementChanged(e);

           	if (e.OldElement != null)
			{
				(e.OldElement as SvgImageView).OnInvalidate -= HandleInvalidate; 
				(e.OldElement as SvgImageView).GetColorHandler -= GetColorHandler;
			}

			if (e.NewElement != null)
			{
				(e.NewElement as SvgImageView).OnInvalidate += HandleInvalidate;
				(e.NewElement as SvgImageView).GetColorHandler += GetColorHandler;
			}

            SetNeedsDisplay();
        }

		Xamarin.Forms.Color GetColorHandler(SvgImageView svgImageView, double x, double y)
		{
			if (FormsControl != svgImageView)
			{
				Console.WriteLine("Cross-linked renderer and view");
			}

			var bitmapImage = FormsControl.BitmapImage as CGImage;
			if (bitmapImage != null)
			{
				if (x >= 0 && x < bitmapImage.Width &&
					y >= 0 && y < bitmapImage.Height)
				{
					return GetPixelColor(bitmapImage, (float)x, (float)y);
				}
			}
			return Xamarin.Forms.Color.Transparent;
		}

		private Xamarin.Forms.Color GetPixelColor(CGImage myImage, float x, float y)
		{
			var rawData = new byte[4];
			var handle = GCHandle.Alloc(rawData); 
			try
			{
				using (var colorSpace = CGColorSpace.CreateDeviceRGB())
				{
					using (var context = new CGBitmapContext(rawData, 1, 1, 8, 4, colorSpace, CGImageAlphaInfo.PremultipliedLast))
					{
						context.DrawImage(new RectangleF(-x, y - myImage.Height, myImage.Width, myImage.Height), myImage);
					 
						return Xamarin.Forms.Color.FromRgba(rawData[0], rawData[1], rawData[2], rawData[3]);
					}
				}
			}
			finally
			{
				handle.Free();
			} 
		}

        /// <summary>
        /// Handles view invalidate.
        /// </summary>
        void HandleInvalidate(object sender, EventArgs args) {
            SetNeedsDisplay();
        }
    }
}
