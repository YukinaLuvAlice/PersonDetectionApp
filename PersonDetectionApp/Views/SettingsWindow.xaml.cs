using PersonDetectionApp.Services;
using System;
using System.Windows;

namespace PersonDetectionApp.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly AutoStartService _autoStartService;

        public double PersonSensitivity { get; private set; }
        public double FaceSensitivity { get; private set; }
        public bool IsAutoStartEnabled { get; private set; }

        public SettingsWindow(double personSensitivity, double faceSensitivity)
        {
            InitializeComponent();

            _autoStartService = new AutoStartService();

            // Đặt giá trị ban đầu
            sliderPersonSensitivity.Value = personSensitivity;
            sliderFaceSensitivity.Value = faceSensitivity;
            chkAutoStart.IsChecked = _autoStartService.IsAutoStartEnabled();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Lưu các cài đặt
            PersonSensitivity = sliderPersonSensitivity.Value;
            FaceSensitivity = sliderFaceSensitivity.Value;
            IsAutoStartEnabled = chkAutoStart.IsChecked ?? false;

            // Cập nhật tự động khởi động
            if (IsAutoStartEnabled)
            {
                _autoStartService.EnableAutoStart();
            }
            else
            {
                _autoStartService.DisableAutoStart();
            }

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}