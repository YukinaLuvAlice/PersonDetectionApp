using OpenCvSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PersonDetectionApp.Services
{
    public class CameraService : IDisposable
    {
        private VideoCapture _capture;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;

        public event EventHandler<Mat> FrameCaptured;

        public CameraService()
        {
            _capture = new VideoCapture();
        }

        public async Task StartCameraAsync()
        {
            if (_isRunning) return;

            if (!_capture.IsOpened())
            {
                _capture.Open(0, VideoCaptureAPIs.ANY); // Mở camera mặc định (id = 0)

                if (!_capture.IsOpened())
                    throw new Exception("Không thể kết nối với camera.");
            }

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => CaptureFrames(), _cancellationTokenSource.Token);
        }

        private void CaptureFrames()
        {
            using var frame = new Mat();

            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_capture.Read(frame) && !frame.Empty())
                {
                    FrameCaptured?.Invoke(this, frame.Clone());
                }

                Thread.Sleep(60); // ~30 FPS
            }
        }

        public void StopCamera()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            StopCamera();
            _capture?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}