using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;

namespace PersonDetectionApp.Services
{
    public class AutoStartService
    {
        private const string RUN_LOCATION = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "PersonDetectionApp";

        public bool IsAutoStartEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_LOCATION))
            {
                if (key == null) return false;

                object value = key.GetValue(APP_NAME);
                return value != null;
            }
        }

        public void EnableAutoStart()
        {
            try
            {
                // Sửa lại phần này để tránh lỗi gán giá trị cho biến key
                RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_LOCATION, true);

                if (key == null)
                {
                    // Tạo key nếu không tồn tại
                    Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
                    key = Registry.CurrentUser.OpenSubKey(RUN_LOCATION, true);
                }

                if (key != null) // Kiểm tra thêm để tránh null reference
                {
                    try
                    {
                        string appPath = Assembly.GetExecutingAssembly().Location;
                        key.SetValue(APP_NAME, appPath);
                    }
                    finally
                    {
                        key.Close(); // Đảm bảo đóng key
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể thiết lập tự động khởi động: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DisableAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_LOCATION, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(APP_NAME, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể huỷ tự động khởi động: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}