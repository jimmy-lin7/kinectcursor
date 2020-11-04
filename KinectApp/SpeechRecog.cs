using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.WpfViewers;

namespace KinectApp
{
    class SpeechRecog
    {
        KinectSpeechCommander speechCommander;

        SpeechRecog(KinectSensor sensor)
        {
            speechCommander = new KinectSpeechCommander();
            speechCommander.Start(sensor);
        }
    }
}
