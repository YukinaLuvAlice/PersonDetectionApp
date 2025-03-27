using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonDetectionApp.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using PersonDetectionApp.Views;

namespace PersonDetectionApp.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly CameraService _cameraService;
        private readonly PersonDetectionService _detectionService;
        private readonly FaceRecognitionService _faceRecognitionService;
        private readonly ScreenService _screenService;

        [ObservableProperty]
        private Mat _currentFrame;

        [ObservableProperty]
        private bool _isDetectionEnabled = true;

        [ObservableProperty]
        private bool _isPersonDetected = false;

        [ObservableProperty]
        private bool _isFaceRecognitionEnabled = false;

        [ObservableProperty]
        private bool _isAuthorizedPersonDetected = false;

        [ObservableProperty]
        private double _personSensitivity = 5;

        [ObservableProperty]
        private double _faceSensitivity = 5;

        public MainViewModel()
        {
            _cameraService = new CameraService();
            _detectionService = new PersonDetectionService();
            _faceRecognitionService = new FaceRecognitionService();
            _screenService = new ScreenService();

            // Đăng ký sự kiện
            _cameraService.FrameCaptured += OnFrameCaptured;
            _detectionService.PersonDetected += OnPersonDetected;
            _faceRecognitionService.AuthorizedPersonDetected += OnAuthorizedPersonDetected;
        }

        private void OnFrameCaptured(object sender, Mat frame)
        {
            // Clone frame để đảm bảo không có vấn đề về đồng bộ hóa
            var clonedFrame = frame.Clone();

            // Cập nhật CurrentFrame trên thread UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentFrame = clonedFrame;
            });

            if (IsDetectionEnabled)
            {
                // Xử lý nhận diện người
                _detectionService.DetectPerson(clonedFrame);

                if (IsFaceRecognitionEnabled)
                {
                    // Xử lý nhận diện khuôn mặt
                    _faceRecognitionService.DetectAndRecognizeFaces(clonedFrame);
                }
            }
        }

        private void OnPersonDetected(object sender, bool isDetected)
        {
            IsPersonDetected = isDetected;

            if (isDetected && IsDetectionEnabled && !IsFaceRecognitionEnabled)
            {
                _screenService.TurnScreenOn();
            }
        }

        private void OnAuthorizedPersonDetected(object sender, bool isDetected)
        {
            IsAuthorizedPersonDetected = isDetected;

            if (isDetected && IsDetectionEnabled && IsFaceRecognitionEnabled)
            {
                _screenService.TurnScreenOn();
            }
        }

        [RelayCommand]
        private async Task StartCameraAsync()
        {
            try
            {
                await _cameraService.StartCameraAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể khởi động camera: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void StopCamera()
        {
            _cameraService.StopCamera();
        }

        [RelayCommand]
        private void ToggleDetection()
        {
            IsDetectionEnabled = !IsDetectionEnabled;
        }

        [RelayCommand]
        private void ToggleFaceRecognition()
        {
            IsFaceRecognitionEnabled = !IsFaceRecognitionEnabled;
        }

        [RelayCommand]
        private void RegisterFace()
        {
            var registerWindow = new RegisterFaceWindow(_cameraService, _faceRecognitionService);
            registerWindow.ShowDialog();
        }
        [RelayCommand]
        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow(PersonSensitivity, FaceSensitivity);

            if (settingsWindow.ShowDialog() == true)
            {
                PersonSensitivity = settingsWindow.PersonSensitivity;
                FaceSensitivity = settingsWindow.FaceSensitivity;

                // Cập nhật độ nhạy trong các dịch vụ
                if (_detectionService is PersonDetectionService personService)
                {
                    personService.Sensitivity = PersonSensitivity;
                }

                if (_faceRecognitionService is FaceRecognitionService faceService)
                {
                    faceService.Sensitivity = FaceSensitivity;
                }
            }
        }

        public void Dispose()
        {
            // Huỷ đăng ký sự kiện
            if (_cameraService != null)
            {
                _cameraService.FrameCaptured -= OnFrameCaptured;
                _cameraService.Dispose();
            }

            if (_detectionService != null)
            {
                _detectionService.PersonDetected -= OnPersonDetected;
            }

            if (_faceRecognitionService != null)
            {
                _faceRecognitionService.AuthorizedPersonDetected -= OnAuthorizedPersonDetected;
            }
        }
        // Thêm thuộc tính độ nhạy
        
    }
}