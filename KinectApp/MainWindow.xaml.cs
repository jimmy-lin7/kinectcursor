using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace KinectApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml; Kinect init and management
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private KinectSensorManager kinectSensorManager { get; set; }

        private Skeleton[] skeletonData;

        private KCursor kCursor;

        public MainWindow()
        {
            kinectSensorManager = new KinectSensorManager(); 
            kinectSensorManager.KinectSensorChanged += KinectSensorChanged;
            this.DataContext = this.kinectSensorManager;

            InitializeComponent();

            this.SensorChooserUI.KinectSensorChooser = sensorChooser;
            sensorChooser.Start();

            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

            kCursor = new KCursor(ref sensorChooser, kinectSensorManager);
        }

        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> e)
        {
            if (null != e.NewValue)
            {
                this.InitializeKinectServices(this.kinectSensorManager, e.NewValue);
            }

            if (null != e.OldValue)
            {
                StopKinect(e.OldValue);
            }
        }

        // Kinect enabled apps should customize which Kinect services it initializes here.
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            // Application should enable all streams first.
            kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            kinectSensorManager.ColorStreamEnabled = true;

            kinectSensorManager.DepthFormat = DepthImageFormat.Resolution640x480Fps30;
            kinectSensorManager.DepthStreamEnabled = true;

            // Smoothing
            var parameters = new TransformSmoothParameters()
            {
                Smoothing = 0.9f,
                Correction = 0.0f,
                Prediction = 0.0f,
                MaxDeviationRadius = 0.3f
            };
            kinectSensorManager.TransformSmoothParameters = parameters;
            kinectSensorManager.SkeletonStreamEnabled = true;
            kinectSensorManager.SkeletonTrackingMode = SkeletonTrackingMode.Seated;

            // Enable Kinect
            kinectSensorManager.KinectSensorEnabled = true;

            sensor.AllFramesReady += NewSensor_AllFramesReady;
        }

        private void NewSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Use skeleton frame to do stuff
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                    foreach (Skeleton skeleton in this.skeletonData)
                    {
                        if (SkeletonTrackingState.Tracked == skeleton.TrackingState)
                        {
                            kCursor.moveHandCursor(skeleton);
                        }
                    }
                }
            }

        }

        void StopKinect(KinectSensor sensor)
        {
            //Console.WriteLine(sensor);
            if (sensor != null) {
                sensor.Stop();
            }
        }


        #region WPF
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sensorChooser.Stop();
            kinectSensorManager.KinectSensorEnabled = false;
            StopKinect(sensorChooser.Kinect);
        }

        // Change elevation
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            kinectSensorManager.ElevationAngle = (int) slider.Value;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            button.Content = (int) slider.Value;
        }

        private void leftHandedRadioClick(object sender, RoutedEventArgs e)
        {
            kCursor.setHand(0);
        }

        private void rightHandedRadioClick(object sender, RoutedEventArgs e)
        {
            kCursor.setHand(1);
        }
        #endregion
    }
}
