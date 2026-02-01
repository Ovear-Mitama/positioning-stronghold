using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace positioning_stronghold.Pages
{
    public class GifImage : Image
    {
        private BitmapDecoder _decoder;
        private DispatcherTimer _timer;
        private int _currentFrame = 0;
        private bool _isInitialized = false;
        private bool _isAnimated = false;

        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register("GifSource", typeof(Uri), typeof(GifImage), new UIPropertyMetadata(null, OnGifSourceChanged));

        public Uri GifSource
        {
            get { return (Uri)GetValue(GifSourceProperty); }
            set { SetValue(GifSourceProperty, value); }
        }

        private static void OnGifSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var gifImage = (GifImage)d;
            gifImage.InitializeGif((Uri)e.NewValue);
        }

        private void InitializeGif(Uri source)
        {
            if (source == null)
                return;

            try
            {
                var bitmapStream = Application.GetResourceStream(source);
                if (bitmapStream == null)
                    return;

                var stream = bitmapStream.Stream;
                stream.Position = 0;

                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                stream.Position = 0;

                string fileType = System.Text.Encoding.ASCII.GetString(buffer);

                if (fileType.StartsWith("GIF"))
                {
                    _isAnimated = true;
                    _decoder = new GifBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    if (_decoder.Frames.Count > 1)
                    {
                        _timer = new DispatcherTimer();
                        _timer.Interval = TimeSpan.FromMilliseconds(30);
                        _timer.Tick += Timer_Tick;
                        _timer.Start();
                    }

                    Source = _decoder.Frames[0];
                    _isInitialized = true;
                }
                else
                {
                    var bitmapDecoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    Source = bitmapDecoder.Frames[0];
                    _isInitialized = true;
                }
            }
            catch
            {
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isAnimated || _decoder == null)
                return;

            _currentFrame = (_currentFrame + 1) % _decoder.Frames.Count;
            Source = _decoder.Frames[_currentFrame];
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
    }
}