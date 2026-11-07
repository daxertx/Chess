using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;

namespace chess_project.files.windows
{
    public partial class Window1 : Window
    {
        private ObservableCollection<string> waiting_players = new ObservableCollection<string>();
        private List<string> players_playing;

        public Window1()
        {
            InitializeComponent();

            MyListView.ItemsSource = this.waiting_players;
            this.players_playing = new List<string>();

            ServiceHost s = new ServiceHost(typeof(chess_project.ChessService));
            s.Open();

            StartPlayerListRefresh();
        }
        
        //refresh every 100 ms
        private void StartPlayerListRefresh()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (sender, e) => RefreshPlayerList();
            timer.Start();
        }

        private void RefreshPlayerList()
        {
            string[] names = ChessService.GetWaitingPlayers();

            this.waiting_players.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                this.waiting_players.Add(names[i]);
            }
        }
    }
}
