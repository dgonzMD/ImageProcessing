using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;

namespace ImageProcessing
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static DispatcherTimer timer = new DispatcherTimer();
        PhotoCamera cam = new PhotoCamera();
        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true);
 //       private WriteableBitmap wb;
//        private bool pumpARGBFrames;
        public MainPage()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
           
            // Check to see if the camera is available on the device.
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                 (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                // Initialize the default camera.
                cam = new Microsoft.Devices.PhotoCamera();
                
                //Event is fired when the PhotoCamera object has been initialized
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                //Set the VideoBrush source to the camera
                viewfinderBrush.SetSource(cam);
             
            }
            else
            {
                // The camera is not supported on the device.
    //          txtDebug.Text = "A Camera is not available on this device.";


                // Disable UI.
 //               GrayscaleOnButton.IsEnabled = false;
 //               GrayscaleOffButton.IsEnabled = false;
            }
           
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose of the camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
            }
        }

        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                timer.Interval = TimeSpan.FromMilliseconds(5.0);
                timer.Tick += new EventHandler(callback);
                timer.Start();
            });

        }

          
          internal int ColorToGray(int color)
          {
              int gray = 0;

              int a = color >> 24;
              int r = (color & 0x00ff0000) >> 16;
              int g = (color & 0x0000ff00) >> 8;
              int b = (color & 0x000000ff);

              if ((r == g) && (g == b))
              {
                  gray = color;
              }
              else
              {
                  // Calculate for the illumination.
                  // I =(int)(0.109375*R + 0.59375*G + 0.296875*B + 0.5)
                  int i = (7 * r + 38 * g + 19 * b + 32) >> 6;

                  gray = ((a & 0xFF) << 24) | ((i & 0xFF) << 16) | ((i & 0xFF) << 8) | (i & 0xFF);
              }
              return gray;
          }

          // Start ARGB to grayscale pump.
/*          private void GrayOn_Clicked(object sender, RoutedEventArgs e)
          {
              MainImage.Visibility = Visibility.Visible;
              pumpARGBFrames = true;
              ARGBFramesThread = new System.Threading.Thread(PumpARGBFrames);

              wb = new WriteableBitmap((int)cam.PreviewResolution.Width, (int)cam.PreviewResolution.Height);
              this.MainImage.Source = wb;

              // Start pump.
              ARGBFramesThread.Start();
              this.Dispatcher.BeginInvoke(delegate()
              {
                  txtDebug.Text = "ARGB to Grayscale";
              });
          }
*/
          // Stop ARGB to grayscale pump.
 /*         private void GrayOff_Clicked(object sender, RoutedEventArgs e)
          {
              MainImage.Visibility = Visibility.Collapsed;
              pumpARGBFrames = false;

              this.Dispatcher.BeginInvoke(delegate()
              {
                  txtDebug.Text = "";
              });
          }
  */
         private void callback(object sender, EventArgs e)
          {
              Globals.tickCount += 5 ;
              PhotoCamera phCam = (PhotoCamera)cam;
              int[] ARGBPx = new int[(int)cam.PreviewResolution.Width * (int)cam.PreviewResolution.Height];
           //   int[] ARGBPx = new int[640*480];

             for (int i = Globals.n1 - 1; i > 0; i--)
                  Globals.x1[i] = Globals.x1[i - 1];

              pauseFramesEvent.WaitOne();
              phCam.GetPreviewBufferArgb32(ARGBPx);
              Globals.x1[0] = (ARGBPx[(int)rect1.Width*(((int)rect1.Height+1)/2)]>>16)&0xFF;
              canvas1.Children.Clear();
              for (int i = Globals.n1 - 2; i >= 0; i--)
              {
                  Line line = new Line() { X1 = i+1, Y1 = 255-Globals.x1[i+1], 
                                           X2 = i, Y2 = 255-Globals.x1[i] };
                  line.Stroke = new SolidColorBrush(Colors.Black);
                  line.StrokeThickness = 4;
                  line.StrokeStartLineCap = PenLineCap.Round;
                  this.canvas1.Children.Add(line);
              }
              Globals.lpf();
          }
    }
}