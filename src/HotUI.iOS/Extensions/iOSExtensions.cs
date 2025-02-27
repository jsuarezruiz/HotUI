﻿using System;
using System.Linq;
using CoreGraphics;
using CoreText;
using UIKit;
namespace HotUI.iOS {
	public static partial class iOSExtensions {


		static iOSExtensions()
		{
			HotUI.iOS.UI.Init ();
		}
		public static UIViewController ToViewController (this View view, bool allowNav = true)
		{
			var vc = new HotUIViewController 
			{
				CurrentView = view,
			};
			
			if (vc.View != null)
			{
				if ((view.BuiltView ?? view) is NavigationView nav && allowNav)
				{
					var navController = new HUINavigationController();
					nav.PerformNavigate = (toView) =>
					{
						//Since iOS doesn't allow nested navigations, pass the navigate along
						if (toView is NavigationView newNav)
						{
							newNav.PerformNavigate = nav.PerformNavigate;
						}

						toView.Navigation = nav;
						navController.PushViewController(toView.ToViewController(false), true);
					};
					navController.PushViewController(vc, false);
					return navController;
				}
			}

			return vc;
		}
		public static iOSViewHandler GetOrCreateViewHandler(this View view)
		{
			if (view == null)
				return null;
			var handler = view.ViewHandler;
			if (handler == null) {

				handler = Registrar.Handlers.GetHandler (view.GetType ()) as IViewHandler;
				view.ViewHandler = handler;
			}
			var iUIView = handler as iOSViewHandler;
			return iUIView;
		}
		public static UIView ToView (this View view)
		{
			var handler = view.GetOrCreateViewHandler ();
			return handler?.View;
		}

        public static Color ToColor(this UIColor color)
        {
            if (color == null)
                return null;

            color.GetRGBA(out var red, out var green, out var blue, out var alpha);
            return new Color((float)red, (float)green, (float)blue, (float)alpha);
        }

        public static UIColor ToUIColor(this Color color)
        {
            if (color == null)
                return null;

            return new UIColor(color.R, color.G, color.B, color.A);
        }

        public static CGColor ToCGColor(this Color color)
        {
	        if (color == null)
		        return null;

	        return new CGColor(color.R, color.G, color.B, color.A);
        }
        
        public static FontAttributes ToFont(this UIFont font)
        {
            //TODO: Add set a default;
            if (font == null)
                throw new ArgumentNullException("font");

            // todo: implement support for attributes other than name and size.
            return new FontAttributes
            {
                Family = font.Name,
                Size = (float)font.PointSize,
            };
        }

        public static UIFont ToUIFont(this FontAttributes attributes)
        {
			if (attributes == null)
				return UIFont.SystemFontOfSize(12);

			if (attributes.Family == Device.FontService.SystemFontName)
			{
				var weight = (int) attributes.Weight;
				if (weight > (int)Weight.Regular)
					return UIFont.BoldSystemFontOfSize(attributes.Size);
				
				return UIFont.SystemFontOfSize(attributes.Size);
			}
			
			return UIFont.FromName(attributes.Family, attributes.Size);
        }
        
        public static UIViewController GetViewController( this UIView view)
        {
            if(view.NextResponder is UIViewController vc)
                return vc;
            if (view.NextResponder is UIView uIView)
                return uIView.GetViewController();
            return null;
        }


        public static UIGestureRecognizer ToGestureRecognizer(this Gesture gesture)
        {
            if (gesture is TapGesture tapGesture)
            {
                return new HUITapGesture (tapGesture);
            }
            throw new NotImplementedException();
        }
    }
}
