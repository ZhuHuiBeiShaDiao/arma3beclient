﻿using Arma3BE.Client.Infrastructure;
using Arma3BE.Client.Infrastructure.Commands;
using Arma3BE.Client.Infrastructure.Models;
using Arma3BEClient.Libs.ModelCompact;
using Arma3BEClient.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Arma3BE.Client.Modules.SteamModule.Models
{
    public class ServerMonitorSteamQueryViewModel : ViewModelBase
    {
        private readonly IIpService _ipService;
        private bool _isBisy;


        public ServerMonitorSteamQueryViewModel(ServerInfo serverInfo, IIpService ipService)
        {
            _ipService = ipService;
            Host = serverInfo.Host;
            Port = serverInfo.SteamPort;

            OnPropertyChanged(nameof(Host));
            OnPropertyChanged(nameof(Port));

            ExcecuteCommand = new ActionCommand(() => Task.Run(() =>
            {
                try
                {
                    IsBisy = true;
                    GetStat();
                }
                finally
                {
                    IsBisy = false;
                }
            }),
                () =>
                {
                    var iphost = Host;

                    if (string.IsNullOrEmpty(iphost))
                    {
                        return false;
                    }
                    return true;
                });
        }

        private void GetStat()
        {
            var iphost = _ipService.GetIpAddress(Host);
            var server = new Arma3BEClient.Steam.Server(new IPEndPoint(IPAddress.Parse(iphost), Port));

            var settings = new GetServerInfoSettings();
            var rules = server.GetServerRulesSync(settings);
            ServerRules =
                rules.Select(
                    x =>
                        new Tuple<string, string>(x.Key,
                            x.Value)).ToList();

            var serverInfoR =
                server.GetServerInfoSync(settings);

            var props = serverInfoR.GetType().GetProperties();

            ServerInfo =
                props.Select(
                    x =>
                        new Tuple<string, string>(x.Name,
                            x.GetValue(serverInfoR)
                                .ToString())).ToList();

            ServerPlayers =
                server.GetServerChallengeSync(settings);


            OnPropertyChanged(nameof(ServerRules));
            OnPropertyChanged(nameof(ServerInfo));
            OnPropertyChanged(nameof(ServerPlayers));
        }

        public string Title { get { return "Steam Query"; } }

        public bool IsBisy
        {
            get { return _isBisy; }
            set
            {
                _isBisy = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExcecuteCommand { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public IEnumerable<Tuple<string, string>> ServerRules { get; private set; }
        public IEnumerable<Tuple<string, string>> ServerInfo { get; private set; }
        public ServerPlayers ServerPlayers { get; private set; }
    }
}