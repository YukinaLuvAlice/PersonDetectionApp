using OpenCvSharp;
using OpenCvSharp.Face;
using System;
using System.Collections.Generic;
using System.IO;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace PersonDetectionApp.Services
{
    public class FaceRecognitionService
    {
        private CascadeClassifier _faceDetector;
        private EigenFaceRecognizer _recognizer;
        private List<Mat> _trainingImages = new List<Mat>();
        private List<int> _personLabels = new List<int>();
        private List<string> _personNames = new List<string>();
        private double _sensitivity = 5; // Giá trị mặc định

        public double Sensitivity
        {
            get { return _sensitivity; }
            set { _sensitivity = value; }
        }

        public event EventHandler<bool> AuthorizedPersonDetected;

        public FaceRecognitionService()
        {
            string faceCascadePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Haarcascades", "haarcascade_frontalface_default.xml");

            // Nếu file không tồn tại, thông báo lỗi
            if (!File.Exists(faceCascadePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file {faceCascadePath}. Vui lòng tải file và đặt trong thư mục Resources/Haarcascades.");
            }

            _faceDetector = new CascadeClassifier(faceCascadePath);
            _recognizer = EigenFaceRecognizer.Create(80, double.PositiveInfinity);

            // Tạo thư mục TrainingData nếu chưa tồn tại
            string trainingDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrainingData");
            if (!Directory.Exists(trainingDir))
            {
                Directory.CreateDirectory(trainingDir);
            }

            // Tải dữ liệu huấn luyện nếu có
            LoadTrainingData();
        }

        private void LoadTrainingData()
        {
            string trainingDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrainingData");

            if (Directory.Exists(trainingDir))
            {
                string[] personDirs = Directory.GetDirectories(trainingDir);

                foreach (string personDir in personDirs)
                {
                    string personName = new DirectoryInfo(personDir).Name;
                    int personLabel = _personNames.Count;
                    _personNames.Add(personName);

                    string[] images = Directory.GetFiles(personDir, "*.jpg");
                    foreach (string image in images)
                    {
                        Mat trainImage = Cv2.ImRead(image, ImreadModes.Grayscale);
                        Cv2.Resize(trainImage, trainImage, new Size(100, 100));
                        _trainingImages.Add(trainImage);
                        _personLabels.Add(personLabel);
                    }
                }

                if (_trainingImages.Count > 0)
                {
                    // Chuyển đổi danh sách thành các mảng cần thiết cho huấn luyện
                    Mat[] trainImages = _trainingImages.ToArray();
                    int[] labels = _personLabels.ToArray();

                    _recognizer.Train(trainImages, labels);
                }
            }
        }

        public void SaveTrainingData(string personName, Mat faceImage)
        {
            string trainingDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrainingData");
            string personDir = Path.Combine(trainingDir, personName);

            if (!Directory.Exists(trainingDir))
                Directory.CreateDirectory(trainingDir);

            if (!Directory.Exists(personDir))
                Directory.CreateDirectory(personDir);

            int imageCount = Directory.GetFiles(personDir, "*.jpg").Length;
            string imagePath = Path.Combine(personDir, $"face_{imageCount + 1}.jpg");

            // Đảm bảo faceImage là ảnh xám
            using (Mat grayFace = new Mat())
            {
                if (faceImage.Channels() > 1)
                    Cv2.CvtColor(faceImage, grayFace, ColorConversionCodes.BGR2GRAY);
                else
                    faceImage.CopyTo(grayFace);

                // Điều chỉnh kích thước và lưu
                Mat resizedFace = new Mat();
                Cv2.Resize(grayFace, resizedFace, new Size(100, 100));
                Cv2.ImWrite(imagePath, resizedFace);

                // Cập nhật dữ liệu huấn luyện
                if (!_personNames.Contains(personName))
                {
                    _personNames.Add(personName);
                }

                int personLabel = _personNames.IndexOf(personName);
                _trainingImages.Add(resizedFace.Clone());
                _personLabels.Add(personLabel);

                // Huấn luyện lại bộ nhận diện
                if (_trainingImages.Count > 0)
                {
                    Mat[] trainImages = _trainingImages.ToArray();
                    int[] labels = _personLabels.ToArray();

                    _recognizer.Train(trainImages, labels);
                }
            }
        }

        public void DetectAndRecognizeFaces(Mat frame)
        {
            if (frame == null || frame.Empty()) return;

            using (var grayFrame = new Mat())
            {
                // Chuyển sang ảnh xám
                Cv2.CvtColor(frame, grayFrame, ColorConversionCodes.BGR2GRAY);
                Cv2.EqualizeHist(grayFrame, grayFrame);

                // Điều chỉnh tham số dựa trên độ nhạy
                double scaleFactor = 1.1 - (_sensitivity * 0.005); // 1.05 (nhạy nhất) đến 1.1 (ít nhạy)
                int minNeighbors = Math.Max(1, 5 - (int)(_sensitivity / 2)); // 1 (nhạy nhất) đến 5 (ít nhạy)

                // Phát hiện khuôn mặt
                Rect[] faces = _faceDetector.DetectMultiScale(
                    grayFrame,
                    scaleFactor,
                    minNeighbors,
                    HaarDetectionTypes.DoCannyPruning,
                    new Size(30, 30)
                );

                bool authorizedPersonDetected = false;

                foreach (Rect face in faces)
                {
                    // Vẽ hình chữ nhật xung quanh khuôn mặt
                    Cv2.Rectangle(frame, face, Scalar.Red, 2);

                    // Chỉ nhận diện nếu đã có dữ liệu huấn luyện
                    if (_trainingImages.Count > 0)
                    {
                        using (Mat faceROI = new Mat(grayFrame, face))
                        {
                            Mat resizedFace = new Mat();
                            Cv2.Resize(faceROI, resizedFace, new Size(100, 100));

                            int label = -1;
                            double confidence = 0;

                            _recognizer.Predict(resizedFace, out label, out confidence);

                            // Điều chỉnh ngưỡng tin cậy dựa trên độ nhạy
                            // Ngưỡng thấp hơn = nhạy hơn (dễ nhận diện hơn)
                            double threshold = 3000 - (_sensitivity * 200);

                            // Nhận diện thành công nếu độ tin cậy thấp (khoảng cách nhỏ)
                            if (label != -1 && confidence < threshold)
                            {
                                authorizedPersonDetected = true;
                                string name = _personNames[label];

                                Cv2.PutText(
                                    frame,
                                    name,
                                    new Point(face.X, face.Y - 10),
                                    HersheyFonts.HersheyComplex,
                                    1.0,
                                    Scalar.Green,
                                    2
                                );
                            }
                            else
                            {
                                Cv2.PutText(
                                    frame,
                                    "Unknown",
                                    new Point(face.X, face.Y - 10),
                                    HersheyFonts.HersheyComplex,
                                    1.0,
                                    Scalar.Red,
                                    2
                                );
                            }
                        }
                    }
                }

                AuthorizedPersonDetected?.Invoke(this, authorizedPersonDetected);
            }
        }
    }
}