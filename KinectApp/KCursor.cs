using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;

namespace KinectApp
{
    class KCursor
    {

        private KinectRegion kinectRegion;
        private CoordinateMapper coordMapper;
        private int hand;

        public KCursor(ref KinectSensorChooser sensorChooser, KinectSensorManager kinectSensorManager)
        {
            coordMapper = new CoordinateMapper(kinectSensorManager.KinectSensor);

            kinectRegion = new KinectRegion();
            var regionSensorBinding = new Binding("Kinect") { Source = sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            kinectRegion.HandPointersUpdated += KinectRegion_HandPointersUpdated;
            this.hand = 1; // Track right hand by default
        }

        public void setHand(int hand)
        {
            this.hand = hand;
        }

        public void moveHandCursor(Skeleton skeleton)
        {
            if (this.hand == 1)
            {
                handToCameraPoint(skeleton.Joints[JointType.HandRight].Position, 80);
                //handToElbowCameraPoint(skeleton.Joints[JointType.ElbowRight].Position, skeleton.Joints[JointType.HandRight].Position);
            }
            else
            {
                handToCameraPoint(skeleton.Joints[JointType.HandLeft].Position, -80);
                //handToElbowCameraPoint(skeleton.Joints[JointType.ElbowLeft].Position, skeleton.Joints[JointType.HandLeft].Position);
            }
        }

        private void KinectRegion_HandPointersUpdated(object sender, EventArgs e)
        {
            //Console.WriteLine(kinectRegion.HandPointers.Count);
            if (kinectRegion.HandPointers.Count < 2) // Ensure there are hands to track
            {
                return;
            }
            if (kinectRegion.HandPointers[this.hand].HandEventType == HandEventType.Grip)
            {
                Mouse.MouseClick("left", 0);
                //this.pointA.Content = "B";
            }
            else if (kinectRegion.HandPointers[this.hand].HandEventType == HandEventType.GripRelease)
            {
                Mouse.MouseClick("left", 1);
                //this.pointA.Content = "A";
            }
        }

        // Use hand position for cursor movement
        void handToCameraPoint(SkeletonPoint skeletonPointHand, int xOffset)
        {
            DepthImagePoint handDepthPoint = coordMapper.MapSkeletonPointToDepthPoint(skeletonPointHand, DepthImageFormat.Resolution640x480Fps30);
            ColorImagePoint handColorPoint = coordMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, handDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);

            int scaledHandDistanceX = (handColorPoint.X - (320 + xOffset)) * 12; // scale movement
            int scaledHandDistanceY = (handColorPoint.Y - 160) * 10; // scale movement

            int X = 1280 + scaledHandDistanceX;
            int Y = 720 + scaledHandDistanceY;

            Mouse.SetCursorPos(Math.Max(0, Math.Min(X, 2559)), Math.Max(0, Math.Min(Y, 1440)));

            //moveElement(pointA, X, Y);

            //Console.WriteLine(X + " " + Y);
        }

        // Use hand position relative to elbow position for cursor movement
        void handToElbowCameraPoint(SkeletonPoint skeletonPointElbow, SkeletonPoint skeletonPointHand)
        {
            DepthImagePoint elbowDepthPoint = coordMapper.MapSkeletonPointToDepthPoint(skeletonPointElbow, DepthImageFormat.Resolution640x480Fps30);
            ColorImagePoint elbowColorPoint = coordMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, elbowDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);

            DepthImagePoint handDepthPoint = coordMapper.MapSkeletonPointToDepthPoint(skeletonPointHand, DepthImageFormat.Resolution640x480Fps30);
            ColorImagePoint handColorPoint = coordMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, handDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);

            int scaledHandElbowDistanceX = (handColorPoint.X - elbowColorPoint.X) * 12; // scale movement
            int scaledHandElbowDistanceY = (handColorPoint.Y - elbowColorPoint.Y) * 8; // scale movement

            int X = 1280 + scaledHandElbowDistanceX;
            int Y = 720 + scaledHandElbowDistanceY;

            Mouse.SetCursorPos(Math.Max(0, Math.Min(X, 2559)), Math.Max(0, Math.Min(Y, 1440)));

            //moveElement(pointA, X, Y);

            //Console.WriteLine(X + " " + Y);
        }

        void moveElement(FrameworkElement element, int X, int Y)
        {
            Canvas.SetLeft(element, X);
            Canvas.SetTop(element, Y);
            //Console.WriteLine(point.X + " " + point.Y);
        }

    }
}
