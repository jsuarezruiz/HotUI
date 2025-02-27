﻿using Foundation;
using HotUI.iOS.Services;
using HotUI.Mac.Handlers;
using HotUI.Mac.Services;

namespace HotUI.Mac
{
    public static class UI
    {
        static bool _hasInitialized;
        static NSObject _invoker = new NSObject ();

        public static void Init()
        {
            if (_hasInitialized) return;
            _hasInitialized = true;

            // Controls
            Registrar.Handlers.Register<Button, ButtonHandler>();
            Registrar.Handlers.Register<Image, ImageHandler>();
            Registrar.Handlers.Register<TextField, TextFieldHandler>();
            Registrar.Handlers.Register<Text, TextHandler>();
            Registrar.Handlers.Register<SecureField, SecureFieldHandler>();
            Registrar.Handlers.Register<Slider, SliderHandler>();
            Registrar.Handlers.Register<ShapeView, ShapeViewHandler>();
            Registrar.Handlers.Register<Toggle, ToggleHandler>();
            //Registrar.Handlers.Register<WebView, WebViewHandler> ();

            // Containers
            Registrar.Handlers.Register<ScrollView, ScrollViewHandler>();
			Registrar.Handlers.Register<View, ViewHandler> ();
			Registrar.Handlers.Register<ContentView, ContentViewHandler> ();
            Registrar.Handlers.Register<ListView, ListViewHandler>();
            Registrar.Handlers.Register<ViewRepresentable, ViewRepresentableHandler>();

            // Managed Layout
            Registrar.Handlers.Register<HStack, HStackHandler>();
            Registrar.Handlers.Register<VStack, VStackHandler>();
            Registrar.Handlers.Register<ZStack, ZStackHandler>();
            Registrar.Handlers.Register<Spacer, SpacerHandler>();
            Registrar.Handlers.Register<Grid, GridHandler>();

            // Device Features
            Device.PerformInvokeOnMainThread = _invoker.BeginInvokeOnMainThread;
            Device.FontService = new MacFontService();
            Device.GraphicsService = new MacGraphicsService();
            Device.BitmapService = new MacBitmapService();
        }
    }
}