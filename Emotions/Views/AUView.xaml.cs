using System;
using System.Windows.Controls;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Frame = Emotions.Services.Engine.Frame;

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
        

        public void Update(Frame frame)
        {
            AU1.Value = frame.AU1;
            AU2.Value = frame.AU2;
            AU3.Value = frame.AU3;
            AU4.Value = frame.AU4;
            AU5.Value = frame.AU5;
            AU6.Value = frame.AU6;

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
