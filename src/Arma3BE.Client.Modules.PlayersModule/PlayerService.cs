﻿using Arma3BE.Client.Infrastructure.Events;
using Arma3BE.Client.Infrastructure.Models;
using Arma3BE.Client.Modules.PlayersModule.Boxes;
using Arma3BE.Client.Modules.PlayersModule.Grids;
using Arma3BE.Client.Modules.PlayersModule.Models;
using Microsoft.Practices.Unity;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;

namespace Arma3BE.Client.Modules.PlayersModule
{
    public class PlayerService
    {
        private readonly IUnityContainer _container;

        public PlayerService(IUnityContainer container)
        {
            _container = container;

            var eventAggregator = _container.Resolve<IEventAggregator>();

            eventAggregator.GetEvent<ShowUserInfoEvent>().Subscribe(model =>
            {
                ShowDialog(model.UserGuid);
            });
        }

        private void ShowDialog(string userGuid)
        {
            var model = _container.Resolve<PlayerViewModel>(new ParameterOverride("userGuid", userGuid));

            if (model.Player != null)
            {
                var window = _container.Resolve<PlayerViewWindow>(new ParameterOverride("model", model));
                window.Show();
            }
        }
    }
}