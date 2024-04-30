using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Point = System.Windows.Point;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media.Media3D;
using System.Windows.Media;
//using Color = System.Windows.Media.Color;
using Color = System.Drawing.Color;

namespace lab04
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string? fileName;
        private Mode mode;
        private Bitmap rgbBitMap;
        private Bitmap hslBitMap;

        private List<double> lastRGBvalues = [0, 0, 0];
        private List<double> lastHSLvalues = [0, 0, 0];

        private Point startPoint;
        private Point endPoint;

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPEG Files (*.jpg)|*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                fileName = openFileDialog.FileName;
                BitmapImage bitmapImage = new BitmapImage(new Uri(fileName));
                mainImage.Source = bitmapImage;

                rgbBitMap = ConvertToRGB();
                hslBitMap = ConvertToHSL();
                lastRGBvalues = [0, 0, 0];
                lastHSLvalues = [0, 0, 0];
                ChangeSlidersValues([255, 255, 255]);
                mode = Mode.RGB;
                ChangeLabels(["R: ", "G: ", "B: "]);
                ConvertButton.Content = "Convert to HSL";

            }
        }

        private void Convert(object sender, RoutedEventArgs e)
        {
            if (fileName == null) return;
            Bitmap bitmap;
            List<double> vals = [SchemeFirstLetterSlider.Value, SchemeSecondLetterSlider.Value, SchemeThirdLetterSlider.Value];

            if (mode == Mode.RGB)
            {
                bitmap = ConvertToHSL(rgbBitMap);
                hslBitMap = bitmap;
                mode = Mode.HSL;
                ChangeLabels(["H: ", "S: ", "L: "]);
                lastRGBvalues = vals;
                ChangeSlidersValues([360, 1, 1]);
                ConvertButton.Content = "Convert to RGB";
            }
            else
            {
                bitmap = ConvertToRGB(hslBitMap);
                rgbBitMap = bitmap;
                mode = Mode.RGB;
                ChangeLabels(["R: ", "G: ", "B: "]);
                lastHSLvalues = vals;
                ChangeSlidersValues([255, 255, 255]);
                ConvertButton.Content = "Convert to HSL";
            }

            mainImage.Source = ConvertBitmapToBitmapImage(bitmap);
        }

        private void ChangeLabels(string[] arr)
        {
            var labelsArr = new List<Label>() { SchemeFirstLetter, SchemeSecondLetter, SchemeThirdLetter };
            for(int i = 0; i < arr.Length; i++)
            {
                labelsArr[i].Content = arr[i];
            }
        }

        private void ChangeSlidersValues(double[] arr)
        {
            var slidersArr = new List<Slider>() { SchemeFirstLetterSlider, SchemeSecondLetterSlider, SchemeThirdLetterSlider };
            for(int i = 0; i < arr.Length; ++i)
            {
                slidersArr[i].Maximum = arr[i];
                slidersArr[i].Minimum = -arr[i];
            }
            ChangeSliderStartingPosition();
        }

        private void ChangeSliderStartingPosition()
        {
            var slidersArr = new List<Slider>() { SchemeFirstLetterSlider, SchemeSecondLetterSlider, SchemeThirdLetterSlider };
            for(int i = 0; i < lastHSLvalues.Count; ++i)
            {
                if (mode == Mode.RGB) slidersArr[i].Value = lastRGBvalues[i];
                else slidersArr[i].Value = lastHSLvalues[i];
            }

        }

        private Bitmap ConvertToHSL()
        {
            if (fileName == null) return null;

            Bitmap bitmap = new Bitmap(fileName);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    double hue, saturation, lightness;
                    RgbToHsl(pixelColor.R, pixelColor.G, pixelColor.B, out hue, out saturation, out lightness);

                    bitmap.SetPixel(x, y, ColorFromHsl(hue, saturation, lightness));
                }
            }

            return bitmap;
        }

        private Bitmap ConvertToHSL(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    double hue, saturation, lightness;
                    RgbToHsl(pixelColor.R, pixelColor.G, pixelColor.B, out hue, out saturation, out lightness);

                    bitmap.SetPixel(x, y, ColorFromHsl(hue, saturation, lightness));
                }
            }

            return bitmap;
        }

        private void RgbToHsl(int r, int g, int b, out double h, out double s, out double l)
        {
            double red = r / 255.0;
            double green = g / 255.0;
            double blue = b / 255.0;

            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));

            double hue = 0;
            if (max == min)
            {
                hue = 0;
            }
            else if (max == red)
            {
                hue = 60 * (0 + (green - blue) / (max - min));
            }
            else if (max == green)
            {
                hue = 60 * (2 + (blue - red) / (max - min));
            }
            else if (max == blue)
            {
                hue = 60 * (4 + (red - green) / (max - min));
            }
            if (hue < 0)
            {
                hue += 360;
            }

            double saturation = (max == 0) ? 0 : (max - min) / max;

            double lightness = (max + min) / 2;

            h = hue;
            s = saturation;
            l = lightness;
        }

        private Color ColorFromHsl(double h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double red = 0, green = 0, blue = 0;
            if (0 <= h && h < 60)
            {
                red = c;
                green = x;
            }
            else if (60 <= h && h < 120)
            {
                red = x;
                green = c;
            }
            else if (120 <= h && h < 180)
            {
                green = c;
                blue = x;
            }
            else if (180 <= h && h < 240)
            {
                green = x;
                blue = c;
            }
            else if (240 <= h && h < 300)
            {
                red = x;
                blue = c;
            }
            else if (300 <= h && h < 360)
            {
                red = c;
                blue = x;
            }

            byte r = (byte)((red + m) * 255);
            byte g = (byte)((green + m) * 255);
            byte b = (byte)((blue + m) * 255);

            return Color.FromArgb(255, r, g, b);

        }

        private Bitmap ConvertToRGB()
        {
            if (fileName == null) return null;

            Bitmap bitmap = new Bitmap(fileName);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    double red, green, blue;
                    HslToRgb(pixelColor.GetHue(), pixelColor.GetSaturation(), pixelColor.GetBrightness(), out red, out green, out blue);

                    bitmap.SetPixel(x, y, Color.FromArgb((int)(red * 255), (int)(green * 255), (int)(blue * 255)));
                }
            }

            return bitmap;
        }

        private Bitmap ConvertToRGB(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    double red, green, blue;
                    HslToRgb(pixelColor.GetHue(), pixelColor.GetSaturation(), pixelColor.GetBrightness(), out red, out green, out blue);

                    bitmap.SetPixel(x, y, Color.FromArgb((int)(red * 255), (int)(green * 255), (int)(blue * 255)));
                }
            }

            return bitmap;
        }

        private void HslToRgb(double h, double s, double l, out double r, out double g, out double b)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double red = 0, green = 0, blue = 0;
            if (0 <= h && h < 60)
            {
                red = c;
                green = x;
            }
            else if (60 <= h && h < 120)
            {
                red = x;
                green = c;
            }
            else if (120 <= h && h < 180)
            {
                green = c;
                blue = x;
            }
            else if (180 <= h && h < 240)
            {
                green = x;
                blue = c;
            }
            else if (240 <= h && h < 300)
            {
                red = x;
                blue = c;
            }
            else if (300 <= h && h < 360)
            {
                red = c;
                blue = x;
            }

            r = red + m;
            g = green + m;
            b = blue + m;
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            if (fileName == null) return;
            BitmapImage bitmapImage = new BitmapImage(new Uri(fileName));
            mainImage.Source = bitmapImage;

            rgbBitMap = ConvertToRGB();
            hslBitMap = ConvertToHSL();
            lastRGBvalues = [0, 0, 0];
            lastHSLvalues = [0, 0, 0];
            ChangeSlidersValues([255, 255, 255]);
            mode = Mode.RGB;
            ChangeLabels(["R: ", "G: ", "B: "]);
            ConvertButton.Content = "Convert to HSL";
        }

        private void FirstLetterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = SchemeFirstLetterSlider.Value;

            if (mode == Mode.HSL)
            {
                Bitmap updatedBitmap = hslBitMap;
                if (updatedBitmap == null) return;

                int height = updatedBitmap.Height;
                int width = updatedBitmap.Width;

                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixelColor;
                        lock (this)
                        {
                            pixelColor = updatedBitmap.GetPixel(x, y);
                        }

                        double h, s, l;
                        RgbToHsl(pixelColor.R, pixelColor.G, pixelColor.B, out h, out s, out l);

                        h += sliderValue;

                        if (h < 0)
                            h += 360;
                        else if (h > 360)
                            h -= 360;

                        Color newColor = ColorFromHsl(h, s, l);

                        lock (this)
                        {
                            updatedBitmap.SetPixel(x, y, newColor);
                        }
                    }
                });

                hslBitMap = updatedBitmap;
                mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
            }

            else
            {
                Bitmap updatedBitmap = rgbBitMap; // Зчитування зображення
                if (updatedBitmap == null) return;

                int width = updatedBitmap.Width;
                int height = updatedBitmap.Height;

                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixelColor;

                        lock (this)
                        {
                            pixelColor = updatedBitmap.GetPixel(x, y);
                        }

                        int newR = (int)(pixelColor.R + sliderValue);

                        newR = Math.Max(0, Math.Min(255, newR));

                        Color newColor = Color.FromArgb(pixelColor.A, newR, pixelColor.G, pixelColor.B);

                        lock (this)
                        {
                            updatedBitmap.SetPixel(x, y, newColor);
                        }
                    }
                });

                rgbBitMap = updatedBitmap;
                mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
            }
        }

        private void SecondLetterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = SchemeSecondLetterSlider.Value;

            if (mode == Mode.HSL)
            {
                Bitmap updatedBitmap = hslBitMap;
                if (updatedBitmap == null) return;

                int height = updatedBitmap.Height;
                int width = updatedBitmap.Width;

                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixelColor;
                        lock (this)
                        {
                            pixelColor = updatedBitmap.GetPixel(x, y);
                        }

                        double h, s, l;
                        RgbToHsl(pixelColor.R, pixelColor.G, pixelColor.B, out h, out s, out l);

                        s += sliderValue;

                        if (s < 0)
                            s += 1;
                        else if (h > 1)
                            h -= 1;

                        Color newColor = ColorFromHsl(h, s, l);

                        lock (this)
                        {
                            updatedBitmap.SetPixel(x, y, newColor);
                        }
                    }
                });

                hslBitMap = updatedBitmap;
                mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
            }

            else
            {
                Bitmap updatedBitmap = rgbBitMap; // Зчитування зображення
                if (updatedBitmap == null) return;

                int width = updatedBitmap.Width;
                int height = updatedBitmap.Height;

                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixelColor;

                        lock (this)
                        {
                            pixelColor = updatedBitmap.GetPixel(x, y);
                        }

                        int newG = (int)(pixelColor.G + sliderValue);

                        newG = Math.Max(0, Math.Min(255, newG));

                        Color newColor = Color.FromArgb(pixelColor.A, pixelColor.R, newG, pixelColor.B);

                        lock (this)
                        {
                            updatedBitmap.SetPixel(x, y, newColor);
                        }
                    }
                });

                rgbBitMap = updatedBitmap;
                mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
            }

        }

        private void ThirdLetterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = SchemeThirdLetterSlider.Value;

            if (mode == Mode.HSL)
            {
                Bitmap updatedBitmap = hslBitMap;
                if (updatedBitmap == null) return;

                int height = updatedBitmap.Height;
                int width = updatedBitmap.Width;

                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixelColor;
                        lock (this)
                        {
                            pixelColor = updatedBitmap.GetPixel(x, y);
                        }

                        double h, s, l;
                        RgbToHsl(pixelColor.R, pixelColor.G, pixelColor.B, out h, out s, out l);

                        l += sliderValue;

                        if (l < 0)
                            l += 1;
                        else if (l > 1)
                            l -= 1;

                        Color newColor = ColorFromHsl(h, s, l);

                        lock (this)
                        {
                            updatedBitmap.SetPixel(x, y, newColor);
                        }
                    }
                });

                hslBitMap = updatedBitmap;
                mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
            }

            else
            {
                Bitmap updatedBitmap = rgbBitMap; // Зчитування зображення
                if (updatedBitmap == null) return;

                int width = updatedBitmap.Width;
                int height = updatedBitmap.Height;

                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixelColor;

                        lock (this)
                        {
                            pixelColor = updatedBitmap.GetPixel(x, y);
                        }

                        int newB = (int)(pixelColor.B + sliderValue);

                        newB = Math.Max(0, Math.Min(255, newB));

                        Color newColor = Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G, newB);

                        lock (this)
                        {
                            updatedBitmap.SetPixel(x, y, newColor);
                        }
                    }
                });

                rgbBitMap = updatedBitmap;
                mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
            }
        }

        private void SetValuesButton(object sender, RoutedEventArgs e)
        {
            double[] sliderValues = {SchemeFirstLetterSlider.Value, SchemeSecondLetterSlider.Value, SchemeThirdLetterSlider.Value};
            if(mode == Mode.HSL)
            {
                Bitmap updatedBitmap = rgbBitMap;
                if (updatedBitmap == null) return;

                int height = updatedBitmap.Height;
                int width = updatedBitmap.Width;

                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixelColor;
                        lock (this)
                        {
                            pixelColor = updatedBitmap.GetPixel(x, y);
                        }

                        double h, s, l;
                        RgbToHsl(pixelColor.R, pixelColor.G, pixelColor.B, out h, out s, out l);

                        h += sliderValues[0];
                        s += sliderValues[1];
                        l += sliderValues[2];

                        if (h < 0)
                            h += 360;
                        else if (h > 360)
                            h -= 360;

                        if (s < 0)
                            s += 1;
                        else if (s > 1)
                            s -= 1;

                        if (l < 0)
                            l += 1;
                        else if (l > 1)
                            l -= 1;

                        Color newColor = ColorFromHsl(h, s, l);

                        lock (this)
                        {
                            updatedBitmap.SetPixel(x, y, newColor);
                        }
                    }
                });

                hslBitMap = updatedBitmap;
                mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
            }
            else
            {
                Bitmap updatedBitmap = hslBitMap;
                if (updatedBitmap == null) return;

                int width = updatedBitmap.Width;
                int height = updatedBitmap.Height;

                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixelColor;

                        lock (this)
                        {
                            pixelColor = updatedBitmap.GetPixel(x, y);
                        }

                        int newR = (int)(pixelColor.R + sliderValues[0]);
                        int newG = (int)(pixelColor.G + sliderValues[1]);
                        int newB = (int)(pixelColor.B + sliderValues[2]);

                        newR = Math.Max(0, Math.Min(255, newR));
                        newG = Math.Max(0, Math.Min(255, newG));
                        newB = Math.Max(0, Math.Min(255, newB));


                        Color newColor = Color.FromArgb(pixelColor.A, newR, newG, newB);

                        lock (this)
                        {
                            updatedBitmap.SetPixel(x, y, newColor);
                        }
                    }
                });

                rgbBitMap = updatedBitmap;
                mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
            }
        }

        private void myImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(mainImage);
            CreateRectangle(startPoint, endPoint);
        }

        private void myImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            endPoint = e.GetPosition(mainImage);
            CreateRectangle(startPoint, endPoint);
        }

        private void CreateRectangle(Point topLeft, Point bottomRight)
        {
            double x = Math.Min(topLeft.X, bottomRight.X);
            double y = Math.Min(topLeft.Y, bottomRight.Y);
            double width = Math.Abs(topLeft.X - bottomRight.X);
            double height = Math.Abs(topLeft.Y - bottomRight.Y);

            //selectionRect.Margin = new Thickness(x, y, 0, 0);
            Canvas.SetLeft(selectionRect, x);
            Canvas.SetTop(selectionRect, y);
            selectionRect.Width = width;
            selectionRect.Height = height;

            selectionRect.Visibility = Visibility.Visible;
        }

        private void ClearSelection(object sender, RoutedEventArgs e)
        {
            selectionRect.Visibility = Visibility.Collapsed;
        }

        private void SpecialRedSaturationClick(object sender, RoutedEventArgs e)
        {
            Bitmap updatedBitmap = rgbBitMap;

            int X1 = (int)(startPoint.X / mainImage.ActualWidth * rgbBitMap.Width);
            int Y1 = (int)(startPoint.Y / mainImage.ActualHeight * rgbBitMap.Height);
            int X2 = (int)(endPoint.X / mainImage.ActualWidth * rgbBitMap.Width);
            int Y2 = (int)(endPoint.Y / mainImage.ActualHeight * rgbBitMap.Height);

            for (int x = X1; x < X2; x++)
            {
                for (int y = Y1; y < Y2; y++)
                {
                    Color pixelColor = updatedBitmap.GetPixel(x, y);

                    if (pixelColor.R > 125)
                    {
                        double h, s, l;
                        RgbToHsl(pixelColor.R, pixelColor.G, pixelColor.B, out h, out s, out l);
                        s += RedSaturationSlider.Value;

                        Color newColor = ColorFromHsl(h, s, l);

                        updatedBitmap.SetPixel(x, y, newColor);

                    }
                }
            }

            rgbBitMap = updatedBitmap;
            mainImage.Source = ConvertBitmapToBitmapImage(updatedBitmap);
        }

        private void SaveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "Save picture as ";
            save.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            BitmapImage bi;
            if (mode == Mode.RGB) bi = ConvertBitmapToBitmapImage(rgbBitMap);
            else bi = ConvertBitmapToBitmapImage(hslBitMap);

            if (bi != null)
            {
                if (save.ShowDialog() == true)
                {
                    JpegBitmapEncoder jpg = new JpegBitmapEncoder();
                    jpg.Frames.Add(BitmapFrame.Create(bi));
                    using (Stream stm = File.Create(save.FileName))
                    {
                        jpg.Save(stm);
                    }
                }
            }
        }

        private void myImage_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(mainImage);

            if (position.X >= 0 && position.X < mainImage.ActualWidth && position.Y >= 0 && position.Y < mainImage.ActualHeight)
            {
                int x = (int)position.X;
                int y = (int)position.Y;

                Color pixelColor = rgbBitMap.GetPixel(x, y);

                RgbToHsl(pixelColor.R, pixelColor.G, pixelColor.B, out double h, out double s, out double l);

                R_TextBlock.Text = $"R: {pixelColor.R}";
                G_TextBlock.Text = $"G: {pixelColor.G}";
                B_TextBlock.Text = $"B: {pixelColor.B}";

                H_TextBlock.Text = $"H: {h}";
                S_TextBlock.Text = $"S: {s}";
                L_TextBlock.Text = $"L: {l}";

                X_TextBlock.Text = $"X: {x}";
                Y_TextBlock.Text = $"Y: {y}";
            }
        }

        private Color GetPixelColor(int x, int y)
        {
            // Отримуємо Source зображення
            BitmapSource bitmapSource = (BitmapSource)mainImage.Source;

            // Перевіряємо, чи отримали дійсне зображення
            if (bitmapSource == null)
            {
                return Color.Transparent;
            }

            // Створюємо WriteableBitmap для отримання значення кольору пікселя
            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapSource);

            // Отримуємо значення кольору пікселя
            Color pixelColor = new Color();
            writeableBitmap.CopyPixels(new Int32Rect(x, y, 1, 1), new byte[] { pixelColor.B, pixelColor.G, pixelColor.R, pixelColor.A }, 4, 0);

            return pixelColor;
        }
    }
}