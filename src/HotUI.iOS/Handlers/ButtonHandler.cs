﻿using System;
using UIKit;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace HotUI.iOS.Handlers
{
    public class ButtonHandler : AbstractControlHandler<Button, UIButton>
    {
        public static readonly PropertyMapper<Button> Mapper = new PropertyMapper<Button>(ViewHandler.Mapper)
        {
            [nameof(Button.Text)] = MapTextProperty,
            [EnvironmentKeys.Colors.Color] = MapColorProperty,
        };
        
        public ButtonHandler() : base(Mapper)
        {

        }

        private static FontAttributes DefaultFont;
        private static Color DefaultColor;
        protected override UIButton CreateView()
        {
            var button = new UIButton(UIButtonType.System);

            if (DefaultColor == null)
            {
                DefaultFont = button.Font.ToFont();
                DefaultColor = button.TitleColor(UIControlState.Normal).ToColor() ;
            }

            button.TouchUpInside += HandleTouchUpInside;
            button.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            /*Layer.BorderColor = UIColor.Blue.CGColor;
            Layer.BorderWidth = .5f;
            Layer.CornerRadius = 3f;*/

            return button;
        }

        protected override void DisposeView(UIButton button)
        {
            button.TouchUpInside -= HandleTouchUpInside;
        }

        private void HandleTouchUpInside(object sender, EventArgs e)
        {
            VirtualView?.OnClick?.Invoke();
        }

        public static void MapTextProperty(IViewHandler viewHandler, Button virtualView)
        {
            var nativeView = (UIButton) viewHandler.NativeView;
            nativeView.SetTitle(virtualView.Text, UIControlState.Normal);
            virtualView.InvalidateMeasurement();
        }

        public static void MapColorProperty(IViewHandler viewHandler, Button virtualView)
        {
            var nativeView = (UIButton)viewHandler.NativeView;
            nativeView.SetTitleColor(virtualView.GetColor(DefaultColor).ToUIColor(), UIControlState.Normal);
        }
    }
}