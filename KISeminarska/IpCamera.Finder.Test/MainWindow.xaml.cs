using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using IpCamera.Controler;
using Microsoft.Win32;

namespace IpCamera.Finder.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Cameras ActiveCameras = new Cameras();
        public static int selectedCameraIndex = -1;

        private static string dir = Directory.GetCurrentDirectory();
        private static string inactiveCameraPath = Path.Combine(dir, "inactiveCamera.png");
        private static string activeCameraPath = Path.Combine(dir, "activeCamera.png");
        private static string PlanPath = Path.Combine(dir, "activeCamera.png");

        public MainWindow()
        {
            InitializeComponent();
            this.Title = UI_main.Title;
            btnTakePicture.Content = UI_main.btnTakePicture;
            btnDetectCameras.Content = UI_main.btnDetectCameras;
            stcCameraInfo.Content = UI_main.stcCameraInfo + ":";
            stcModel.Content = UI_main.stcModel + ":";
            stcIP.Content = UI_main.stcIP + ":";
            stcResolution.Content = UI_main.stcResolution + ":";
            stcDescription.Content = UI_main.stcDescription + ":";
            btnEdit.Content = UI_main.btnEdit;
            btnSave.Content = UI_main.btnSave;
            btnLoad.Content = UI_main.btnLoad;
            btnPlan.Content = UI_main.showPlan;
            btnLoadPlan.Content = UI_main.btnLoadPlan;
            btnSavePic.Content = UI_main.btnSavePic;
        }

        private void btnDetectCameras_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                detectCameras();
            }
            finally
            {
                Mouse.OverrideCursor = null;
                Cursor = Cursors.Arrow;
            }
        }

        private void btnTakePicture_Click(object sender, RoutedEventArgs e)
        {
            INetworkCamera camera = ActiveCameras.cameras[selectedCameraIndex];
            Image picture = new Image();
            //BitmapSource bs = BitmapConversion.ToWpfBitmap(camera.TakePicture());
            BitmapSource bs = (camera.TakePicture()).ToWpfBitmap();
            picture.Source = bs;
            imgPicture.Source = bs;
        }

        private void cbCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            foreach (UIElement element in cnvPlan.Children)
            {
                if (element is Image)
                {
                    Image img = element as Image;
                    BitmapImage bmpInactive = new BitmapImage(new Uri(inactiveCameraPath));
                    img.Source = bmpInactive;
                }
            }


            if (cbCameras.SelectedIndex > 0)
            {
                selectedCameraIndex = cbCameras.SelectedIndex - 1;
                fillInfo();
                btnTakePicture.IsEnabled = true;
                btnEdit.IsEnabled = true;
                foreach (UIElement element in cnvPlan.Children)
                {
                    if (element is Image)
                    {
                        Image img = element as Image;
                        if (img.Tag.Equals(ActiveCameras.cameras[selectedCameraIndex].ID))
                        {
                            BitmapImage bmpActive = new BitmapImage(new Uri(activeCameraPath));
                            img.Source = bmpActive;
                        }
                    }
                }
            }
            else
            {
                selectedCameraIndex = -1;
                fillInfo();
                btnTakePicture.IsEnabled = false;
                btnEdit.IsEnabled = false;
            }
            imgPicture.Source = null;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var editCamera = new EditCamera();
            editCamera.ShowDialog();
            fillCameras();
        }

        private void fillCameras()
        {
            if (ActiveCameras.cameras.Count != 0)
            {
                int oldIndex = cbCameras.SelectedIndex;
                if (oldIndex < 0) { oldIndex = 0; }

                cbCameras.IsEnabled = true;
                cbCameras.Items.Clear();
                cbCameras.Items.Add(UI_main.cbCameras);
                foreach (INetworkCamera cam in ActiveCameras.cameras)
                {
                    if (cam.name == string.Empty)
                    {
                        cbCameras.Items.Add(cam.model);
                    }
                    else
                    {
                        cbCameras.Items.Add(cam.name);
                    }
                }

                cbCameras.SelectedIndex = oldIndex;
            }
            fillInfo();
        }

        private void drawCameras()
        {
            BitmapImage bmpInactive = new BitmapImage(new Uri(inactiveCameraPath));
            BitmapImage bmpActive = new BitmapImage(new Uri(activeCameraPath));

            double offset = bmpInactive.Width;

            for (int i = 0; i < ActiveCameras.cameras.Count; i++)
            {
                Image image = new Image();
                if (cbCameras.SelectedIndex == i + 1)
                {
                    image.Source = bmpActive;
                }
                else
                {
                    image.Source = bmpInactive;
                }
                image.Tag = ActiveCameras.cameras[i].ID;
                image.MouseLeftButtonDown += new MouseButtonEventHandler(dragCamera);
                Canvas.SetLeft(image, ActiveCameras.cameras[i].X - image.ActualHeight / 2);
                Canvas.SetTop(image, ActiveCameras.cameras[i].Y - image.ActualWidth / 2);
                this.cnvPlan.Children.Add(image);
            }
        }

        private void fillInfo()
        {
            if (cbCameras.SelectedIndex > 0)
            {
                INetworkCamera camera = ActiveCameras.cameras[selectedCameraIndex];
                lblResolution.Content = camera.width + "x" + camera.height;
                tbDescription.Text = camera.description;
                lblIP.Content = camera.IP;
                lblModel.Content = camera.model;
            }
            else
            {
                lblResolution.Content = "";
                tbDescription.Text = "";
                lblIP.Content = "";
                lblModel.Content = "";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveCams = new Microsoft.Win32.SaveFileDialog();
            saveCams.FileName = "cameraSettings";
            saveCams.DefaultExt = ".ipcams";
            saveCams.Filter = "IP Camera Settings (.ipcams)|*.ipcams";

            Nullable<bool> result = saveCams.ShowDialog();
            if (result == true)
            {
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(saveCams.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    try
                    {
                        formatter.Serialize(stream, ActiveCameras);
                    }
                    catch (Exception exp) { }
                }
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog loadCams = new Microsoft.Win32.OpenFileDialog();
            loadCams.FileName = "cameraSettings";
            loadCams.DefaultExt = ".ipcams";
            loadCams.Filter = "IP Camera Settings (.ipcams)|*.ipcams";
            Nullable<bool> result = loadCams.ShowDialog();

            if (result == true)
            {
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(loadCams.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    try
                    {
                        ActiveCameras = (Cameras)formatter.Deserialize(stream);
                    }
                    catch (Exception exp) { }
                }
                if (ActiveCameras.cameras.Count != 0)
                {
                    selectedCameraIndex = -1;
                    fillCameras();
                    List<UIElement> tmp = new List<UIElement>();
                    foreach (UIElement element in cnvPlan.Children)
                    {
                        if (element is Image)
                        {
                            tmp.Add(element);
                        }
                    }
                    foreach (UIElement element in tmp)
                    {
                        cnvPlan.Children.Remove(element);
                    }
                    drawCameras();
                    btnTakePicture.IsEnabled = false;
                    btnEdit.IsEnabled = false;
                }
            }
        }

        private void detectCameras()
        {
            List<IPAddress> list = NetworkCamScanner.GetActiveIPs();
            while (list == null)
            {
            }
            List<INetworkCamera> CameraList = NetworkCamScanner.Scan(list);
            foreach (INetworkCamera cam in CameraList)
            {
                cam.setImageSize();
                cam.setModel();
                if (!ActiveCameras.ExistsInCameras(cam))
                {
                    ActiveCameras.cameras.Add(cam);
                }
            }
            fillCameras();
            drawCameras();
        }

        private void btnPlan_Click(object sender, RoutedEventArgs e)
        {
            if (btnPlan.Content.Equals(UI_main.showPlan))
            {
                imgBorder.Visibility = System.Windows.Visibility.Collapsed;
                canvasBorder.Visibility = System.Windows.Visibility.Visible;
                btnPlan.Content = UI_main.hidePlan;
            }
            else if (btnPlan.Content.Equals(UI_main.hidePlan))
            {
                canvasBorder.Visibility = System.Windows.Visibility.Collapsed;
                imgBorder.Visibility = System.Windows.Visibility.Visible;
                btnPlan.Content = UI_main.showPlan;
            }
        }

        private void dragCamera(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            DataObject data = new DataObject(typeof(Image), image);
            DragDrop.DoDragDrop(image, data, DragDropEffects.Move);
        }

        private void dropCamera(object sender, DragEventArgs e)
        {
            Image image = e.Data.GetData(typeof(Image)) as Image;
            this.cnvPlan.Children.Remove(image);
            Canvas.SetLeft(image, e.GetPosition(this.cnvPlan).X);
            Canvas.SetTop(image, e.GetPosition(this.cnvPlan).Y);
            this.cnvPlan.Children.Add(image);

            foreach (INetworkCamera cam in ActiveCameras.cameras)
            {
                if (cam.ID.Equals((string)image.Tag))
                {
                    cam.X = e.GetPosition(this.cnvPlan).X;
                    cam.Y = e.GetPosition(this.cnvPlan).Y;
                }
            }
        }

        private void btnLoadPlan_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Images (.jpg)|*.jpg";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                byte[] buffer = new byte[100000];
                int read, total = 0;

                using (Stream stream = dlg.OpenFile())
                {
                    StreamReader reader = new StreamReader(stream);
                    while ((read = stream.Read(buffer, total, 1000)) != 0)
                    {
                        total += read;
                    }

                    System.Drawing.Bitmap plan2;
                    plan2 = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new MemoryStream(buffer, 0, total));

                    ImageBrush imgBr = new ImageBrush(plan2.ToWpfBitmap());
                    imgBr.Stretch = Stretch.Fill;
                    cnvPlan.Background = imgBr;

                    stream.Close();
                    reader.Close();
                }

            }
        }

        private void btnSavePic_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            Stream stream;

            dlg.Filter = "jpg files (*.jpg)|*.jpg|All files (*.*)|*.*";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;

            Nullable<bool> result = dlg.ShowDialog();

            result = dlg.ShowDialog();

            if (result == true)
            {
                if ((stream = dlg.OpenFile()) != null)
                {
                    // Code to write the stream goes here.
                    //(imgPicture.Source).
                    BitmapSource image = (BitmapSource) imgPicture.Source;
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                    stream.Close();
                }
            }
        }
    }
}