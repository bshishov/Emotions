using System;
using System.Windows.Controls;
using Emotions.KinectTools.Tracking;
using Emotions.Services.Engine;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.Views
{
    /// <summary>
    /// Interaction logic for AUView.xaml
    /// </summary>
    public partial class AUView : UserControl
    {
        public AUView()
        {
            InitializeComponent();

            AU1.Caption = String.Format("AU 1 {0}", Enum.GetName(typeof(AnimationUnit), 0));
            AU2.Caption = String.Format("AU 2 {0}", Enum.GetName(typeof(AnimationUnit), 1));
            AU3.Caption = String.Format("AU 3 {0}", Enum.GetName(typeof(AnimationUnit), 2));
            AU4.Caption = String.Format("AU 4 {0}", Enum.GetName(typeof(AnimationUnit), 3));
            AU5.Caption = String.Format("AU 5 {0}", Enum.GetName(typeof(AnimationUnit), 4));
            AU6.Caption = String.Format("AU 6 {0}", Enum.GetName(typeof(AnimationUnit), 5));
        }
        

        public void Update(EngineFrame engineFrame)
        {
            AU1.Value = engineFrame.LipRaiser;
            AU2.Value = engineFrame.JawLowerer;
            AU3.Value = engineFrame.LipStretcher;
            AU4.Value = engineFrame.BrowLowerer;
            AU5.Value = engineFrame.LipCornerDepressor;
            AU6.Value = engineFrame.BrowRaiser;

            /*
            PosXLabel.Content = buffer.FacePosition.X;
            PosYLabel.Content = buffer.FacePosition.Y;
            PosZLabel.Content = buffer.FacePosition.Z;

            RotXLabel.Content = buffer.FaceRotation.X;
            RotYLabel.Content = buffer.FaceRotation.Y;
            RotZLabel.Content = buffer.FaceRotation.Z;*/
        }
    }
}
