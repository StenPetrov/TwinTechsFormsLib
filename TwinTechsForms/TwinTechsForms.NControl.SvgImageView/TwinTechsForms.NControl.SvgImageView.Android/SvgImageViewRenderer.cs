using System;
using Android.Graphics;
using Android.Runtime;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using NGraphics;
using TwinTechsForms.NControl;
using TwinTechsForms.NControl.Android;
using Size = NGraphics.Size;
using System.Runtime.InteropServices;

[assembly: ExportRenderer(typeof(SvgImageView), typeof(SvgImageViewRenderer))]
namespace TwinTechsForms.NControl.Android
{
	[Preserve(AllMembers = true)]
	public class SvgImageViewRenderer : ImageRenderer
	{
		/// <summary>
		/// Used for registration with dependency service
		/// </summary>
		public static void Init()
		{
			var temp = DateTime.Now;
		}

		public SvgImageViewRenderer()
		{
			// Offer to do our own drawing so Android will actually call `Draw`.
			SetWillNotDraw(willNotDraw: false);
		}

		SvgImageView FormsControl
		{
			get
			{
				return Element as SvgImageView;
			}
		}

		static Func<Size, double, IImageCanvas> CreatePlatformImageCanvas = (size, scale) => new AndroidPlatform().CreateImageCanvas(size, scale);

		public override void Draw(Canvas canvas)
		{
			base.Draw(canvas);

			if (FormsControl != null)
			{
				const double screenScale = 1.0; // Don't need to deal with screen scaling on Android.

				var finalCanvas = FormsControl.RenderSvgToCanvas(new Size(canvas.Width, canvas.Height), screenScale, CreatePlatformImageCanvas);
				var image = (BitmapImage)finalCanvas.GetImage();

				FormsControl.BitmapImage = image;
				FormsControl.Canvas = finalCanvas;
				Control.SetImageBitmap(image.Bitmap);
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
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

			Invalidate();
		}

		Xamarin.Forms.Color GetColorHandler(SvgImageView svgImageView, double x, double y)
		{
			if (FormsControl != svgImageView)
			{
				Console.WriteLine("Cross-linked renderer and view");
			}

			var bitmapImage = FormsControl.BitmapImage as BitmapImage;
			if (bitmapImage != null)
			{
				if (x >= 0 && x < bitmapImage.Bitmap.Width &&
					y >= 0 && y < bitmapImage.Bitmap.Height)
				{
					int color = bitmapImage.Bitmap.GetPixel((int)x, (int)y);
					var intColor = new IntColor { IntVal = color };
					if (intColor.R > 0 || intColor.G > 0 || intColor.B > 0)
					{
						var xColor = Xamarin.Forms.Color.FromUint(intColor.UintVal);
						return xColor;
					}
				}
			}
			return Xamarin.Forms.Color.Transparent;
		}

		/// <summary>
		/// Handles view invalidate.
		/// </summary>
		void HandleInvalidate(object sender, System.EventArgs args)
		{
			Invalidate();
		}

		[StructLayout(LayoutKind.Explicit)]
		struct IntColor
		{
			[FieldOffset(0)]
			public int IntVal;
			[FieldOffset(0)]
			public uint UintVal;

			[FieldOffset(0)]
			public byte R;
			[FieldOffset(1)]
			public byte G;
			[FieldOffset(2)]
			public byte B;
			[FieldOffset(3)]
			public byte A;
		}
	}
}
