﻿using Arma3BE.Client.Modules.MainModule.Boxes;
using Arma3BE.Client.Modules.MainModule.Dialogs;
using Arma3BE.Client.Modules.MainModule.Extensions;
using Arma3BE.Client.Modules.MainModule.Models;
using Microsoft.Practices.Unity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PlayerView = Arma3BE.Client.Modules.MainModule.Helpers.Views.PlayerView;

namespace Arma3BE.Client.Modules.MainModule.Grids
{
    /// <summary>
    ///     Interaction logic for OnlinePlayers.xaml
    /// </summary>
    public partial class OnlinePlayers : UserControl
    {
        public OnlinePlayers()
        {
            InitializeComponent();


            var menu = dg.Generate<PlayerView>();

            foreach (var menuItem in menu.Items.OfType<MenuItem>().ToList())
            {
                menu.Items.Remove(menuItem);
                dg.ContextMenu.Items.Add(menuItem);
            }

            foreach (var generateColumn in GridHelper.GenerateColumns<PlayerView>())
            {
                dg.Columns.Add(generateColumn);
            }
            _playerViewService = MainModuleInit.Current.Resolve<IPlayerViewService>();
        }

        private IPlayerViewService _playerViewService;

        private ServerMonitorPlayerViewModel Model
        {
            get { return DataContext as ServerMonitorPlayerViewModel; }
        }

        private void KickClick(object sender, RoutedEventArgs e)
        {
            var si = dg.SelectedItem as PlayerView;

            if (si != null)
            {
                var w = new KickPlayerWindow(Model.PlayerHelper, si);
                w.ShowDialog();
            }
        }

        private void BanClick(object sender, RoutedEventArgs e)
        {
            var si = dg.SelectedItem as PlayerView;

            if (si != null)
            {
                var w = new BanPlayerWindow(Model.PlayerHelper, si.Guid, true, si.Name, si.Num.ToString());
                w.ShowDialog();
            }
        }

        private void PlayerInfo_Click(object sender, RoutedEventArgs e)
        {
            var si = dg.SelectedItem as PlayerView;

            if (si != null)
            {
                _playerViewService.ShowDialog(si.Guid);
            }
        }
    }
}