using OpenCvSharp;
using System;
using System.IO;
using Size = OpenCvSharp.Size;

namespace PersonDetectionApp.Services
{
    public class PersonDetectionService
    {
        private CascadeClassifier _personClassifier;
        private double _sensitivity = 5; // Giá trị mặc định

        public double Sensitivity
        {
            get { return _sensitivity; }
            set { _sensitivity = value; }
        }

        public event EventHandler<bool> PersonDetected;

        public PersonDetectionService()
        {
            // Sử dụng HOG để phát hiện người (nhanh hơn và chính xác hơn cho phát hiện toàn thân)
            _personClassifier = new CascadeClassifier();

            string cascadePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Haarcascades", "haarcascade_fullbody.xml");

            // Nếu file không tồn tại, thông báo lỗi
            if (!File.Exists(cascadePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file {cascadePath}. Vui lòng tải file và đặt trong thư mục Resources/Haarcascades.");
            }

            _personClassifier.Load(cascadePath);
        }

        public void DetectPerson(Mat frame)
        {
            if (frame == null || frame.Empty()) return;

            // Chuyển đổi sang ảnh xám để xử lý nhanh hơn
            using var grayFrame = new Mat();
            Cv2.CvtColor(frame, grayFrame, ColorConversionCodes.BGR2GRAY);

            // Cân bằng histogram để cải thiện độ tương phản
            Cv2.EqualizeHist(grayFrame, grayFrame);

            // Điều chỉnh các tham số dựa trên độ nhạy
            double scaleFactor = 1.1 - (_sensitivity * 0.005); // 1.05 (nhạy nhất) đến 1.1 (ít nhạy)
            int minNeighbors = Math.Max(1, 5 - (int)(_sensitivity / 2)); // 1 (nhạy nhất) đến 5 (ít nhạy)

            // Phát hiện người trong khung hình
            Rect[] persons = _personClassifier.DetectMultiScale(
                grayFrame,
                scaleFactor,
                minNeighbors,
                HaarDetectionTypes.DoCannyPruning,
                new Size(50, 100),
                new Size(400, 800)
            );

            bool isPersonDetected = persons.Length > 0;

            // Vẽ hình chữ nhật xung quanh người (để hiển thị)
            foreach (Rect person in persons)
            {
                Cv2.Rectangle(frame, person, Scalar.Green, 2);
            }

            // Thông báo kết quả
            PersonDetected?.Invoke(this, isPersonDetected);
        }
    }
}