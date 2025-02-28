﻿using HotUI.Graphics;
using UIKit;

namespace HotUI.iOS.Graphics
{
    public class UIImageBitmap : Bitmap
    {
        private UIImage _image;

        public UIImageBitmap(UIImage image)
        {
            _image = image;
        }

        public override SizeF Size => _image?.Size.ToSizeF() ?? SizeF.Zero;

        public override object NativeBitmap => _image;

        protected override void DisposeNative()
        {
            _image?.Dispose();
            _image = null;
        }
    }
}
