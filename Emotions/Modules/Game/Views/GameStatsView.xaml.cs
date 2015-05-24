
using System.Windows.Controls;

namespace Emotions.Modules.Game.Views
{
    /// <summary>
    /// Interaction logic for GameStatsView.xaml
    /// </summary>
    public partial class GameStatsView : UserControl
    {
        public GameStatsView()
        {
            InitializeComponent();
        }

        public void Update(GameFrame gameFrame)
        {
            this.ReactionTime.Value = gameFrame.ReactionTime;
            this.Failed.Value = gameFrame.Failed;
            this.Missclicks.Value = gameFrame.Missclicks;
            this.Missed.Value = gameFrame.Missed;
        }
    }
}
