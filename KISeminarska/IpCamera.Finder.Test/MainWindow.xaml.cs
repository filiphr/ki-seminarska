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
            BitmapSource bs = BitmapConversion.ToWpfBitmap(camera.TakePicture());
            picture.Source = bs;
            imgPicture.Source = bs;
        }

        private void cbCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCameras.SelectedIndex > 0)
            {
                selectedCameraIndex = cbCameras.SelectedIndex - 1;
                fillInfo();
                btnTakePicture.IsEnabled = true;
                btnEdit.IsEnabled = true;
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
                Canvas.SetLeft(image, ActiveCameras.cameras[i].X);
                Canvas.SetTop(image, ActiveCameras.cameras[i].Y);
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
            if (btnPlan.Content == UI_main.showPlan)
            {
                imgBorder.Visibility = System.Windows.Visibility.Collapsed;
                //imgPicture.Visibility = System.Windows.Visibility.Collapsed;
                canvasBorder.Visibility = System.Windows.Visibility.Visible;
                //cnvPlan.Visibility = System.Windows.Visibility.Visible;
                btnPlan.Content = UI_main.hidePlan;
            }
            else if (btnPlan.Content == UI_main.hidePlan)
            {
                canvasBorder.Visibility = System.Windows.Visibility.Collapsed;
                //cnvPlan.Visibility = System.Windows.Visibility.Collapsed;
                imgBorder.Visibility = System.Windows.Visibility.Visible;
                //imgPicture.Visibility = System.Windows.Visibility.Visible;
                btnPlan.Content = UI_main.showPlan;
            }
        }

        private void dragCamera(object sender, MouseButtonEventArgs e)
        {
            // Image image = e.Source as Image;
            Image image = sender as Image;
            DataObject data = new DataObject(typeof(Image), image);
            DragDrop.DoDragDrop(image, data, DragDropEffects.Move);
        }

        private void dropCamera(object sender, DragEventArgs e)
        {
            //ImageSource image = e.Data.GetData(typeof(ImageSource)) as ImageSource;
            //Image imageControl = new Image() { Width = image.Width, Height = image.Height, Source = image };

            Image image = e.Data.GetData(typeof(Image)) as Image;
            //Image imageControl = new Image() { Width = image.Width, Height = image.Height, Source = image };

            this.cnvPlan.Children.Remove(image);
            Canvas.SetLeft(image, e.GetPosition(this.cnvPlan).X);
            Canvas.SetTop(image, e.GetPosition(this.cnvPlan).Y);
            this.cnvPlan.Children.Add(image);

            foreach (INetworkCamera cam in ActiveCameras.cameras)
            {
                if (cam.ID == image.Tag)
                {
                    cam.X = e.GetPosition(this.cnvPlan).X;
                    cam.Y = e.GetPosition(this.cnvPlan).Y;
                }
            }
        }
    }
}