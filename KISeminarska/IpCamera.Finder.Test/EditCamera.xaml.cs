using System.Windows;
using IpCamera.Controler;

namespace IpCamera.Finder.Test
{
    /// <summary>
    /// Interaction logic for EditCamera.xaml
    /// </summary>
    public partial class EditCamera : Window
    {
        #region Parametars
        /// <summary>
        /// The camera that is being edited
        /// </summary>
        private INetworkCamera camera { get; set; }
        #endregion

        public EditCamera()
        {
            InitializeComponent();
            this.Title = UI_edit.title;
            stcName.Content = UI_edit.stcName + ":";
            stcUsername.Content = UI_edit.stcUsername + ":";
            stcPassword.Content = UI_edit.stcPassword + ":";
            stcModel.Content = UI_edit.stcModel + ":";
            stcIP.Content = UI_edit.stcIP + ":";
            stcResolution.Content = UI_edit.stcResolution + ":";
            stcDescription.Content = UI_edit.stcDescription + ":";
            btnSave.Content = UI_edit.btnSave;

            camera = MainWindow.ActiveCameras.cameras[MainWindow.selectedCameraIndex];
            txtCameraName.Text = camera.name;
            txtUsername.Text = camera.credentials.UserName;
            txtPassword.Text = camera.credentials.Password;
            lblModel.Content = camera.model;
            lblIP.Content = camera.IP;
            lblResolution.Content = camera.width + "x" + camera.height;
            txtDescription.Text = camera.description;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ActiveCameras.cameras[MainWindow.selectedCameraIndex].name = txtCameraName.Text;
            MainWindow.ActiveCameras.cameras[MainWindow.selectedCameraIndex].credentials.UserName = txtUsername.Text;
            MainWindow.ActiveCameras.cameras[MainWindow.selectedCameraIndex].credentials.Password = txtPassword.Text;
            MainWindow.ActiveCameras.cameras[MainWindow.selectedCameraIndex].description = txtDescription.Text;
            this.Close();
        }
    }
}
