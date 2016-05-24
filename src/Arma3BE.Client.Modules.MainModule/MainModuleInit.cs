﻿using Arma3BE.Client.Infrastructure;
using Arma3BE.Client.Modules.MainModule.ViewModel;
using Arma3BE.Server.Abstract;
using Arma3BE.Server.ServerFactory;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Arma3BE.Client.Modules.MainModule
{
    public class MainModuleInit : IModule
    {
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;

        public MainModuleInit(IUnityContainer container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;

            _container.RegisterType<MainViewModel>();
            _container.RegisterType<MainWindow>();

            _container.RegisterType<IBattlEyeServerFactory, WatcherBEServerFactory>();
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegionRegion, typeof(MainWindow));
        }
    }
}