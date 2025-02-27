﻿using System;
using CoreAnimation;
using CoreGraphics;
using HotUI.Graphics;
using UIKit;

// ReSharper disable MemberCanBePrivate.Global

namespace HotUI.iOS.Handlers
{
    public class ViewHandler : AbstractHandler<View, UIView>
    {
        public static readonly PropertyMapper<View> Mapper = new PropertyMapper<View>()
        {
            [nameof(EnvironmentKeys.Colors.BackgroundColor)] = MapBackgroundColorProperty,
            [nameof(EnvironmentKeys.View.Border)] = MapBorderProperty,
            [nameof(EnvironmentKeys.View.Shadow)] = MapShadowProperty,
            [nameof(EnvironmentKeys.View.ClipShape)] = MapClipShapeProperty,
            [nameof(EnvironmentKeys.View.Overlay)] = MapOverlayProperty,
            [nameof(EnvironmentKeys.Animations.Animation)] = MapAnimationProperty,
        };

        protected override UIView CreateView()
        {
            var viewHandler = VirtualView?.GetOrCreateViewHandler();
            if (viewHandler?.GetType() == typeof(ViewHandler) && NativeView == null)
            {
                // this is recursive.
                System.Diagnostics.Debug.WriteLine($"There is no ViewHandler for {VirtualView.GetType()}");
                return null;
            }

            return viewHandler?.View ?? new UIView();
        }

        public override void SetView(View view)
        {
            var previousView = TypedNativeView;
            base.SetView(view);
            BroadcastNativeViewChanged(previousView, TypedNativeView);

        }
        public override void Remove(View view)
        {
            base.Remove(view);
        }

        public override void UpdateValue(string property, object value)
        {
            base.UpdateValue(property, value);
        }

        public static void AddGestures(IViewHandler handler, View view)
        {
            foreach (var g in view.Gestures)
                AddGesture(handler, g);
        }

        public static void AddGesture(IViewHandler handler, Gesture gesture)
        {
            var nativeView = (UIView)handler.NativeView;
            nativeView.AddGestureRecognizer(gesture.ToGestureRecognizer());
        }

        public static void RemoveGestures(IViewHandler handler, View view)
        {
            foreach (var g in view.Gestures)
                RemoveGesture(handler, g);
        }

        public static void RemoveGesture(IViewHandler handler, Gesture gesture)
        {
            var nativeView = (UIView)handler.NativeView;
            var nativeGesture = gesture.NativeGesture as UIGestureRecognizer;
            if (nativeGesture != null)
                nativeView.RemoveGestureRecognizer(nativeGesture);
        }

        public static void MapBackgroundColorProperty(IViewHandler handler, View virtualView)
        {
            var nativeView = (UIView)handler.NativeView;
            var color = virtualView.GetBackgroundColor();
            if (color != null)
                nativeView.BackgroundColor = color.ToUIColor();
        }

        public static void MapBorderProperty(IViewHandler handler, View virtualView)
        {
            var nativeView = (UIView)handler.NativeView;
            var borderShape = virtualView.GetBorder();
            if (borderShape != null)
            {
                var layer = nativeView.Layer;
                var color = borderShape.GetStrokeColor(virtualView, Color.Black);
                var lineWidth = borderShape.GetLineWidth(virtualView, 1);
                var style = borderShape.GetDrawingStyle(virtualView);

                if (style == DrawingStyle.Stroke || style == DrawingStyle.StrokeFill)
                {
                    layer.BorderColor = color.ToCGColor();
                    layer.BorderWidth = lineWidth;

                    if (borderShape is RoundedRectangle roundedRectangle)
                    {
                        var cornerRadius = roundedRectangle.CornerRadius;
                        layer.CornerRadius = cornerRadius;
                    }
                    else if (borderShape is Capsule)
                    {
                        var size = Math.Min(virtualView.Frame.Height, virtualView.Frame.Width);
                        layer.CornerRadius = size / 2;
                    }
                    else if (borderShape is Rectangle)
                    {
                        layer.CornerRadius = 0;
                    }
                }
            }
        }

        public static bool NeedsContainer(View virtualView)
        {
            var overlay = virtualView.GetOverlay();
            if (overlay != null)
                return true;

            var mask = virtualView.GetClipShape();
            if (mask != null)
                return true;

            return false;
        }

        public static void MapShadowProperty(IViewHandler handler, View virtualView)
        {
            var nativeView = (UIView)handler.NativeView;
            var shadow = virtualView.GetShadow();
            var clipShape = virtualView.GetClipShape();

            // If there is a clip shape, then the shadow should be applied to the clip layer, not the view layer
            if (shadow != null && clipShape == null)
            {
                handler.HasContainer = false;
                ApplyShadowToLayer(shadow, nativeView.Layer);
            }
            else
            {
                ClearShadowFromLayer(nativeView.Layer);
                handler.HasContainer = NeedsContainer(virtualView);
            }
        }

        public static void MapOverlayProperty(IViewHandler handler, View virtualView)
        {
            var nativeView = (UIView)handler.NativeView;
            var overlay = virtualView.GetOverlay();

            var viewHandler = handler as iOSViewHandler;


            // If there is a clip shape, then the shadow should be applied to the clip layer, not the view layer
            if (overlay != null)
            {
                handler.HasContainer = true;
                if (viewHandler?.ContainerView != null)
                    viewHandler.ContainerView.OverlayView = overlay.ToView();
            }
            else
            {
                if (viewHandler?.ContainerView != null)
                    viewHandler.ContainerView.OverlayView = null;
                handler.HasContainer = NeedsContainer(virtualView);
            }
        }

        private static void ApplyShadowToLayer(Shadow shadow, CALayer layer)
        {
            layer.ShadowColor = shadow.Color.ToCGColor();
            layer.ShadowRadius = (nfloat)shadow.Radius;
            layer.ShadowOffset = shadow.Offset.ToCGSize();
            layer.ShadowOpacity = shadow.Opacity;
        }

        private static void ClearShadowFromLayer(CALayer layer)
        {
            layer.ShadowColor = new CGColor(0, 0, 0, 0);
            layer.ShadowRadius = 0;
            layer.ShadowOffset = new CGSize();
            layer.ShadowOpacity = 0;
        }

        public static void MapClipShapeProperty(IViewHandler handler, View virtualView)
        {
            var nativeView = (UIView)handler.NativeView;
            var clipShape = virtualView.GetClipShape();
            if (clipShape != null)
            {
                handler.HasContainer = true;
                var bounds = nativeView.Bounds;

                var layer = new CAShapeLayer
                {
                    Frame = bounds
                };

                var path = clipShape.PathForBounds(bounds.ToRectangleF());
                layer.Path = path.ToCGPath();

                var viewHandler = handler as iOSViewHandler;
                if (viewHandler?.ContainerView != null)
                {
                    viewHandler.ContainerView.MaskLayer = layer;
                    viewHandler.ContainerView.ClipShape = clipShape;
                }

                var shadow = virtualView.GetShadow();
                if (shadow != null)
                {
                    var shadowLayer = new CAShapeLayer();
                    shadowLayer.Name = "shadow";
                    shadowLayer.FillColor = new CGColor(0, 0, 0, 1);
                    shadowLayer.Path = layer.Path;
                    shadowLayer.Frame = layer.Frame;

                    ApplyShadowToLayer(shadow, shadowLayer);

                    if (viewHandler?.ContainerView != null)
                        viewHandler.ContainerView.ShadowLayer = shadowLayer;
                }
            }
            else
            {
                var shadow = virtualView.GetShadow();
                if (shadow == null)
                    ClearShadowFromLayer(nativeView.Layer);
                nativeView.Layer.Mask = null;
                handler.HasContainer = NeedsContainer(virtualView);
            }
        }

        public static void MapAnimationProperty(IViewHandler handler, View virtualView)
        {
            var nativeView = (UIView)handler.NativeView;
            var animation = virtualView.GetAnimation();
            if (animation != null)
            {
                System.Diagnostics.Debug.WriteLine($"Starting animation [{animation}] on [{virtualView.GetType().Name}/{nativeView.GetType().Name}]");

                var duration = (animation.Duration ?? 1000.0) / 1000.0;
                var delay = (animation.Delay ?? 0.0) / 1000.0;
                var options = animation.Options.ToAnimationOptions();

                UIView.Animate(
                    duration,
                    delay,
                    options,
                    () =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Animation [{animation}] has been started");

                        var transform = CGAffineTransform.MakeIdentity();

                        if (animation.TranslateTo != null)
                        {
                            var translateTransform = CGAffineTransform.MakeTranslation(animation.TranslateTo.Value.X, animation.TranslateTo.Value.Y);
                            transform = CGAffineTransform.Multiply(transform, translateTransform);
                        }
                        if (animation.RotateTo != null)
                        {
                            var angle = Convert.ToSingle(animation.RotateTo.Value * Math.PI / 180);
                            var rotateTransform = CGAffineTransform.MakeRotation(angle);
                            transform = CGAffineTransform.Multiply(transform, rotateTransform);
                        }
                        if (animation.ScaleTo != null)
                        {
                            var scaleTransform = CGAffineTransform.MakeScale(animation.ScaleTo.Value.X, animation.ScaleTo.Value.Y);
                            transform = CGAffineTransform.Multiply(transform, scaleTransform);
                        }
                        // TODO: implement other animations

                        nativeView.Transform = transform;
                    },
                    () =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Animation [{animation}] has been completed");
                        // Do nothing
                    });
            }
        }
    }
}
