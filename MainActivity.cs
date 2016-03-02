using System;
using System.Threading;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

using PokeD.Server.Android.WrapperInstances;

namespace PokeD.Server.Android
{
    [Activity(Label = "PokeD Server", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.SensorPortrait, LaunchMode = LaunchMode.SingleInstance)]
    public class MainActivity : Activity
    {
        private Thread _serverMainThread;
        private Thread _serverStopThread;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            InputWrapperInstance.TextView = FindViewById<TextView>(Resource.Id.textView1);
            
            FindViewById<Button>(Resource.Id.Button02).Click += MainActivity_02_Click;
            FindViewById<Button>(Resource.Id.Button03).Click += MainActivity_03_Click;

            InputWrapperInstance.RunOnUI += RunOnUiThread;
        }

        private void MainActivity_02_Click(object sender, EventArgs e)
        {
            if (_serverStopThread != null && _serverStopThread.IsAlive)
                while (_serverStopThread.IsAlive) { Thread.SpinWait(200); }
            
            if (_serverMainThread != null && _serverMainThread.IsAlive)
            {
                Program.Stop();
                while (_serverMainThread.IsAlive) { Thread.SpinWait(200); }
            }

            _serverMainThread = new Thread(() => Program.Main());
            _serverMainThread.Start();
        }
        private void MainActivity_03_Click(object sender, EventArgs e)
        {
            if (_serverMainThread != null && _serverMainThread.IsAlive)
            {
                _serverStopThread = new Thread(() => Program.Stop());
                _serverStopThread.Start();

                while (_serverMainThread.IsAlive) { Thread.SpinWait(200); }
            }
        }
    }
}

