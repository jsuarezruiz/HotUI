﻿using HotUI.Blazor.Components;

namespace HotUI.Blazor.Handlers
{
    internal class ImageHandler : BlazorHandler<Image, BImage>
    {
        public static readonly PropertyMapper<Image> Mapper = new PropertyMapper<Image>
        {
            { nameof(Image.Bitmap), MapBitmapProperty },
        };

        public ImageHandler()
            : base(Mapper)
        {
        }

        public static void MapBitmapProperty(IViewHandler viewHandler, Image virtualView)
        {
            var nativeView = (BImage)viewHandler.NativeView;

            nativeView.Url = (string)virtualView.Bitmap.NativeBitmap;
        }
    }
}
