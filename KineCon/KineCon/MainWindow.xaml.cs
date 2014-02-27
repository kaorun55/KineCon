using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
using Microsoft.Kinect;
using NaturalSoftware.Kinect;
using Ventuz.OSC;

namespace KineCon
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensorWrapper kinect;

        UdpWriter udpWriter;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded( object sender, RoutedEventArgs e )
        {
            try {
                udpWriter = new UdpWriter( ConfigurationManager.AppSettings["DestinationAddress"],
                                           int.Parse( ConfigurationManager.AppSettings["DestinationPort"] ) );

                kinect = new KinectSensorWrapper();
                kinect.AllFrameReady += kinect_AllFrameReady;
                kinect.Start();
            }
            catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
                Close();
            }
        }

        void kinect_AllFrameReady( KinectUpdateFrameData e )
        {
            try {
                TextMessage.Text = "";

                if ( !e.IsAllUpdated ) {
                    return;
                }

                ImageColor.Source = e.ColorFrame.ToBitmapSource();

                var skeletons = e.SkeletonFrame.GetTrackedSkeleton();
                if ( skeletons.Count() != 2 ) {
                    return;
                }

                var player1 = skeletons.First().Joints[JointType.Head];
                var player2 = skeletons.Last().Joints[JointType.Head];
                if ( !player1.IsTrackingOrInferred() || !player2.IsTrackingOrInferred() ) {
                    return;
                }

                var distance = (int)(Math.Sqrt( Math.Pow( player2.Position.X - player1.Position.X, 2 ) + 
                                                Math.Pow( player2.Position.Y - player1.Position.Y, 2 ) +
                                                Math.Pow( player2.Position.Z - player1.Position.Z, 2 ) ) * 1000);

                TextMessage.Text = distance.ToString();

                udpWriter.Send( new OscBundle( 0, new OscElement( "/distance", distance ) ) );
            }
            catch ( Exception ex ) {
                Trace.WriteLine( ex.Message );
            }
        }
    }
}
