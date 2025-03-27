using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using PersonDetectionApp.Services;
using System;
using System.Windows;
using Window = System.Windows.Window;
namespace PersonDetectionApp.Views
{
    public partial class RegisterFaceWindow : Window
    {
        private readonly CameraService _cameraService;
        private readonly FaceRecognitionService _faceRecognitionService;
        private Mat _currentFrame;
        private bool _isFaceCaptured = false;

        public RegisterFaceWindow(CameraService cameraService, FaceRecognitionService faceRecognitionService)
        {
            InitializeComponent();

            _cameraService = cameraService;
            _faceRecognitionService = faceRecognitionService;

            // Đăng ký sự kiện
            _cameraService.FrameCaptured += OnFrameCaptured;

            btnSave.IsEnabled = false;
        }

        private void OnFrameCaptured(object sender, Mat frame)
        {
            _currentFrame = frame.Clone();

            // Hiển thị khung hình
            Dispatcher.Invoke(() => {
                imgFacePreview.Source = _currentFrame.ToBitmapSource();
            });
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFrame != null && !_currentFrame.Empty())
            {
                _isFaceCaptured = true;
                btnSave.IsEnabled = true;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_isFaceCaptured && !string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                try
                {
                    _faceRecognitionService.SaveTrainingData(txtUserName.Text, _currentFrame);
                    MessageBox.Show("Đã lưu khuôn mặt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu khuôn mặt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chụp ảnh và nhập tên người dùng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Huỷ đăng ký sự kiện
            if (_cameraService != null)
            {
                _cameraService.FrameCaptured -= OnFrameCaptured;
            }
        }
    }
}