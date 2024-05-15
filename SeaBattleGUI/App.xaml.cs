using SeaBattleApp;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SeaBattleGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Game TheGame { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            TheGame = new Game();
            base.OnStartup(e);
        }
    }

}
