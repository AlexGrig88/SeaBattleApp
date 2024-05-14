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
        protected override void OnStartup(StartupEventArgs e)
        {
            Game Thegame = new Game();
            base.OnStartup(e);
        }
    }

}
