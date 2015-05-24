using System;
using System.Windows.Controls;
using Emotions.KinectTools.Tracking;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.Modules.Engine.Views
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
        

        public void Update(EngineInputFrame engineInputFrame)
        {
            AU1.Value = engineInputFrame.LipRaiser;
            AU2.Value = engineInputFrame.JawLowerer;
            AU3.Value = engineInputFrame.LipStretcher;
            AU4.Value = engineInputFrame.BrowLowerer;
            AU5.Value = engineInputFrame.LipCornerDepressor;
            AU6.Value = engineInputFrame.BrowRaiser;
        }
    }
}
