using OpenCvSharp.WpfExtensions;
using PersonDetectionApp.ViewModels;
using System;
using System.Windows;

namespace PersonDetectionApp
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _isClosing = false;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Đăng ký sự kiện để cập nhật hình ảnh camera
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Đăng ký sự kiện đóng cửa sổ
            Closed += MainWindow_Closed;
            StateChanged += MainWindow_StateChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentFrame))
            {
                UpdateCameraImage();
            }
        }

        private void UpdateCameraImage()
        {
            if (_viewModel.CurrentFrame == null) return;

            // Chuyển đổi từ Mat sang BitmapSource để hiển thị
            try
            {
                // Đảm bảo cập nhật UI trên thread UI
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() => UpdateCameraImage());
                    return;
                }

                CameraImage.Source = _viewModel.CurrentFrame.ToBitmapSource();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị hình ảnh: {ex.Message}");
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            // Ẩn cửa sổ khi thu nhỏ nhưng vẫn chạy ứng dụng
            if (WindowState == WindowState.Minimized)
            {
                // Không cần Hide() vì đã thu nhỏ cửa sổ
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isClosing)
            {
                e.Cancel = true;

                // Hiển thị hộp thoại xác nhận
                MessageBoxResult result = MessageBox.Show(
                    "Bạn có muốn thoát ứng dụng không?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Thoát ứng dụng
                    _isClosing = true;
                    Application.Current.Shutdown();
                }
                // Nếu No, không làm gì cả
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Giải phóng tài nguyên khi đóng ứng dụng
            _viewModel.Dispose();
        }

        private void MinimizeToTray_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}