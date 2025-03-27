using System;
using System.IO;
using System.Windows;

namespace PersonDetectionApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Tạo các thư mục cần thiết khi khởi động ứng dụng
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Thư mục lưu trữ dữ liệu huấn luyện
            string trainingDataDir = Path.Combine(baseDir, "TrainingData");
            if (!Directory.Exists(trainingDataDir))
            {
                Directory.CreateDirectory(trainingDataDir);
            }

            // Thư mục lưu trữ các file cascade
            string resourcesDir = Path.Combine(baseDir, "Resources");
            string haarcascadesDir = Path.Combine(resourcesDir, "Haarcascades");

            if (!Directory.Exists(resourcesDir))
            {
                Directory.CreateDirectory(resourcesDir);
            }

            if (!Directory.Exists(haarcascadesDir))
            {
                Directory.CreateDirectory(haarcascadesDir);
            }

            // Kiểm tra xem các file cascade có tồn tại không
            string faceCascadePath = Path.Combine(haarcascadesDir, "haarcascade_frontalface_default.xml");
            string bodyCascadePath = Path.Combine(haarcascadesDir, "haarcascade_fullbody.xml");

            bool needDownload = false;

            if (!File.Exists(faceCascadePath) || !File.Exists(bodyCascadePath))
            {
                needDownload = true;
                MessageBox.Show("Cần tải các file cascade cho nhận diện. Ứng dụng sẽ tự động tải xuống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (needDownload)
            {
                try
                {
                    using (var client = new System.Net.WebClient())
                    {
                        if (!File.Exists(faceCascadePath))
                        {
                            client.DownloadFile(
                                "https://raw.githubusercontent.com/opencv/opencv/master/data/haarcascades/haarcascade_frontalface_default.xml",
                                faceCascadePath);
                        }

                        if (!File.Exists(bodyCascadePath))
                        {
                            client.DownloadFile(
                                "https://raw.githubusercontent.com/opencv/opencv/master/data/haarcascades/haarcascade_fullbody.xml",
                                bodyCascadePath);
                        }
                    }

                    MessageBox.Show("Đã tải xuống các file cascade cần thiết.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải xuống các file cascade: {ex.Message}\nVui lòng tải thủ công và đặt trong thư mục Resources/Haarcascades.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}