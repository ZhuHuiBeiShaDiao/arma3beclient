﻿using Arma3BE.Client.Infrastructure;
using Arma3BE.Client.Infrastructure.Commands;
using Arma3BE.Client.Modules.MainModule.Contracts;
using Arma3BE.Client.Modules.MainModule.Models;
using Arma3BE.Server;
using Arma3BE.Server.Abstract;
using Arma3BE.Server.Models;
using Arma3BEClient.Common.Logging;
using Arma3BEClient.Libs.ModelCompact;
using Microsoft.Practices.Unity;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Arma3BE.Client.Modules.MainModule.ViewModel
{
    public class ServerMonitorModel : DisposableViewModelBase
    {
        private IBEServer _beServer;
        private readonly bool _console;
        private readonly ILog _log;
        private bool _isBusy;

        public ServerMonitorModel(ServerInfo currentServer, ILog log, IIpService ipService, IUnityContainer container, bool console = false)
        {
            CurrentServer = currentServer;
            _log = log;
            _console = console;

            IsBusy = true;

            Task.Factory.StartNew(() => InitModel(ipService, container, console))
                .ContinueWith(t => IsBusy = false);
        }

        private void InitModel(IIpService ipService, IUnityContainer container, bool console)
        {
            var host = ipService.GetIpAddress(CurrentServer.Host);

            if (string.IsNullOrEmpty(host))
            {
                var message = $"Host is incorrect for server {CurrentServer.Name}";
                _log.Error(message);
                throw new Exception(message);
            }

            SteamQueryViewModel =
                container.Resolve<ServerMonitorSteamQueryViewModel>(new ParameterOverride("host", host),
                    new ParameterOverride("port", CurrentServer.Port));

            _beServer = container.Resolve<BEServer>(new ParameterOverride("host", host),
                new ParameterOverride("port", CurrentServer.Port),
                new ParameterOverride("password", CurrentServer.Password));


            if (!console)
            {
                _beServer.BanHandler += (s, e) => BansViewModel.SetData(e.Data);
                _beServer.AdminHandler += (s, e) => AdminsViewModel.SetData(e.Data);
            }

            _beServer.ConnectHandler += BeServerConnectHandler;
            _beServer.DisconnectHandler += BeServerDisconnectHandler;

            if (!console)
            {
                _beServer.RConAdminLog += (s, e) => _beServer.SendCommand(CommandType.Admins);
            }

            _beServer.PlayerLog += (s, e) => _beServer.SendCommand(CommandType.Players);

            if (!console)
            {
                _beServer.BanLog += (s, e) =>
                {
                    _beServer.SendCommand(CommandType.Players);
                    _beServer.SendCommand(CommandType.Bans);
                };
            }

            _beServer.ConnectingHandler += (s, e) => RaisePropertyChanged(nameof(Connected));


            PlayersViewModel =
                container.Resolve<ServerMonitorPlayerViewModel>(new ParameterOverride("serverInfo", CurrentServer),
                    new ParameterOverride("beServer", _beServer));


            if (!console)
            {

                BansViewModel =
                    container.Resolve<ServerMonitorBansViewModel>(
                        new ParameterOverride("serverInfoId", CurrentServer.Id),
                        new ParameterOverride("beServer", _beServer));

                AdminsViewModel =
                    container.Resolve<ServerMonitorAdminsViewModel>(new ParameterOverride("serverInfo", CurrentServer),
                        new ParameterOverride("refreshCommand",
                            new ActionCommand(() => _beServer.SendCommand(CommandType.Admins))));

                ManageServerViewModel =
                    container.Resolve<ServerMonitorManageServerViewModel>(
                        new ParameterOverride("serverId", CurrentServer.Id),
                        new ParameterOverride("beServer", _beServer));

                PlayerListModelView =
                    container.Resolve<PlayerListModelView>(new ParameterOverride("serverId", CurrentServer.Id),
                        new ParameterOverride("beServer", _beServer));
            }

            ChatViewModel =
                container.Resolve<ServerMonitorChatViewModel>(new ParameterOverride("serverId", CurrentServer.Id),
                    new ParameterOverride("beServer", _beServer));

            Connect();
        }

        public ServerInfo CurrentServer { get; }

        public bool Connected => _beServer?.Connected ?? false;

        public static Color GetMessageColor(ChatMessage message)
        {
            var type = message.Type;

            var color = Colors.Black;

            switch (type)
            {
                case ChatMessage.MessageType.Command:
                    color = Color.FromRgb(212, 169, 24);
                    break;
                case ChatMessage.MessageType.Direct:
                    color = Color.FromRgb(146, 140, 150);
                    break;
                case ChatMessage.MessageType.Global:
                    color = Color.FromRgb(80, 112, 115);
                    break;
                case ChatMessage.MessageType.Group:
                    color = Color.FromRgb(156, 204, 118);
                    break;
                case ChatMessage.MessageType.RCon:
                    color = Color.FromRgb(252, 31, 23);
                    break;
                case ChatMessage.MessageType.Side:
                    color = Color.FromRgb(25, 181, 209);
                    break;
                case ChatMessage.MessageType.Vehicle:
                    color = Color.FromRgb(155, 115, 0);
                    break;
                default:
                    break;
            }

            return color;
        }

        private void BeServerDisconnectHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("Connected");
        }

        private void BeServerConnectHandler(object sender, EventArgs e)
        {
            _beServer.SendCommand(CommandType.Players);

            if (!_console)
            {
                _beServer.SendCommand(CommandType.Admins);
                _beServer.SendCommand(CommandType.Missions);
                _beServer.SendCommand(CommandType.Bans);
            }

            RaisePropertyChanged("Connected");
        }

        public void Connect()
        {
            try
            {
                if (!_beServer.Connected)
                    _beServer.Connect();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            _beServer.Disconnect();
            _beServer.Dispose();
        }

        #region ViewModels

        public ServerMonitorSteamQueryViewModel SteamQueryViewModel { get; set; }

        public ServerMonitorPlayerViewModel PlayersViewModel { get; set; }
        public ServerMonitorBansViewModel BansViewModel { get; set; }
        public ServerMonitorAdminsViewModel AdminsViewModel { get; set; }

        public ServerMonitorChatViewModel ChatViewModel { get; set; }
        public ServerMonitorManageServerViewModel ManageServerViewModel { get; set; }

        public PlayerListModelView PlayerListModelView { get; set; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged(nameof(IsBusy));
                }
            }
        }

        #endregion
    }
}