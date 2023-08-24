using LibraryManagement.Helpers;
using LibraryManagement.Helpers.Serializers;
using LibraryManagement.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LibraryManagement
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }


        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                Window.Current.Content = rootFrame;

            }

            if (e.PrelaunchActivated == false)
            {
                // Try to load data from storage
                LibraryManager libraryManager = new LibraryManager();
                try
                {
                    await LibraryDataSerializer.LoadDataAsync(libraryManager);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                rootFrame.Navigate(typeof(MainPage),libraryManager);
                

            }
            Window.Current.Activate();

        }


        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {


            var deferral = e.SuspendingOperation.GetDeferral();

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.Content is MainPage mainPage)
            {
                await mainPage.SaveDataBeforeAppClose();
            }

            //mainPage?.SaveDataBeforeAppClose();
            deferral.Complete();
        }
    }
}
