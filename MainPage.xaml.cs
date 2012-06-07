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
using System.Diagnostics;

namespace ImageProcessing
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static DispatcherTimer timer = new DispatcherTimer();
        PhotoCamera cam = new PhotoCamera();
        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true);
 //       private WriteableBitmap wb;
//        private bool pumpARGBFrames;
        public static Stopwatch st;

        public MainPage()
        {
            InitializeComponent();
            Globals.canvas2 = canvas1;
            Globals.textBlock2 = textBlock1;
            st = new Stopwatch();
            st.Start();

            canvas1.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
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

                //to rotate the camera (upright)
                viewfinderBrush.RelativeTransform = new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 90 };

            }
            else
            {
                // The camera is not supported on the device.
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

            if (!cam.IsFlashModeSupported(FlashMode.On))
                System.Diagnostics.Debug.WriteLine("No flash support!");
            else
                cam.FlashMode = FlashMode.On;
        }

        int[] ARGBPx = new int[640 * 480];
        double r = 0;
        private void callback(object sender, EventArgs e)
        {
              cam.FocusAtPoint(r, r);
              r = 1 - r;
              cam.Focus();
              textBlock1 = Globals.textBlock2;
              //if (Globals.tickCount % 50 == 0)
              //{
              //    System.Diagnostics.Debug.WriteLine(st.ElapsedMilliseconds/50);
              //    st.Reset();
              //    st.Start();
              //}

              PhotoCamera phCam = (PhotoCamera)cam;
            
              for (int i = Globals.n1 - 1; i > 0; i--)
                  Globals.x1[i] = Globals.x1[i - 1];

              //pauseFramesEvent.WaitOne();
              phCam.GetPreviewBufferArgb32(ARGBPx);
              int tempVal = 0;
              for (int i = 0; i < 5; i++)
                  for (int j = 0; j < 5; j++)
                      tempVal += (ARGBPx[j * (int)rect1.Width + i]>>16)&0xFF;
           //  tempVal+= (ARGBPx[(int)rect1.Width*(((int)rect1.Height+1)/2 - 50+i)]>>16)&0xFF;
              tempVal /= 25;
              Globals.x1[0] = tempVal;
              textBlock1.Text = Globals.x1[0]+"";
              Globals.lpf();
          }

         public static void graph()
         {
             Globals.canvas2.Children.Clear();
             for (int i = (int)Globals.canvas2.Width/5/*Globals.n2 - 2*/; i >= 0; i--)
             {
                 Line line = new Line()
                 {
                     X1 = 5*(i + 1),
                     Y1 = 255 - Globals.y2[(i + 1)]/2,
                     X2 = 5*i,
                     Y2 = 255 - Globals.y2[i]/2
                 };
                 line.Stroke = new SolidColorBrush(Colors.Red);
                 line.StrokeThickness = 4;
                 line.StrokeStartLineCap = PenLineCap.Round;
                 Globals.canvas2.Children.Add(line);
             }
             
         }
    }
}