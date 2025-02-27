﻿using System;
using Windows.UI.Xaml;
using HotUI.UWP.Handlers;
using WGrid = Windows.UI.Xaml.Controls.Grid;

namespace HotUI.UWP
{
    public class HotUIView : WGrid
    {
        private View _view;
        private UIElement _nativeView;
        private IViewHandler _handler;

        public HotUIView(View view = null)
        {
            View = view;
        }

        public View View
        {
            get => _view;
            set
            {
                if (value == _view)
                    return;

                if (_handler is ViewHandler oldViewHandler)
                    oldViewHandler.NativeViewChanged -= HandleNativeViewChanged;

                _view = value;
                _handler = _view?.ViewHandler;

                if (_handler is ViewHandler newViewHandler)
                    newViewHandler.NativeViewChanged += HandleNativeViewChanged;

                HandleNativeViewChanged(this, null);
            }
        }

        private void HandleNativeViewChanged(object sender, ViewChangedEventArgs e)
        {
            if (_nativeView != null)
            {
                Children.Remove(_nativeView);
                _nativeView = null;
            }

            _nativeView = _view?.ToView();
           
            if (_nativeView != null)
            {
                if (_nativeView is FrameworkElement frameworkElement)
                {
                    WGrid.SetRow(frameworkElement, 0);
                    WGrid.SetColumn(frameworkElement, 0);
                    WGrid.SetColumnSpan(frameworkElement, 1);
                    WGrid.SetRowSpan(frameworkElement, 1);
                }

                Children.Add(_nativeView);
            }
        }
    }
}
