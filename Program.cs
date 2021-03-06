﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using Aragas.Core.Wrappers;

using PCLStorage;

using PokeD.Core.Extensions;

using PokeD.Server.Android.WrapperInstances;

namespace PokeD.Server.Android
{
    public static partial class Program
    {
        private const string REPORTURL = "http://poked.github.io/report/";
        private static Server Server { get; set; }


        static Program()
        {
            AppDomainWrapper.Instance = new AppDomainWrapperInstance();
            DatabaseWrapper.Instance = new SQLiteDatabase();
            FileSystemWrapper.Instance = new FileSystemWrapperInstance();
            InputWrapper.Instance = new InputWrapperInstance();
            ConfigWrapper.Instance = new YamlConfigFactoryInstance();
            LuaWrapper.Instance = new MoonLuaWrapperInstance();
            NancyWrapper.Instance = new NancyWrapperInstance();
            TCPClientWrapper.Instance = new TCPClientFactoryInstance();
            TCPListenerWrapper.Instance = new TCPServerWrapperInstance();
            ThreadWrapper.Instance = new ThreadWrapperInstance();

            PacketExtensions.Init();
        }

        public static void Main(params string[] args)
        {
            try { AppDomain.CurrentDomain.UnhandledException += (sender, e) => CatchErrorObject(e.ExceptionObject); }
            catch (Exception exception)
            {
                var exceptionText = CatchError(exception);
                ReportErrorLocal(exceptionText);
                Stop();
            }

            Start();
        }
        private static void ReportErrorLocal(string exception)
        {
            var crashFile = FileSystemWrapper.CrashLogFolder.CreateFileAsync($"{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.log", CreationCollisionOption.OpenIfExists).Result;
            using (var stream = crashFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite).Result)
            using (var writer = new StreamWriter(stream))
                writer.Write(exception);
        }

        public static void Start()
        {
            Server = new Server();
            Server.Start();

            Update();
        }
        public static void Stop()
        {
            Server?.Stop();
            //Environment.Exit((int) ExitCodes.UnknownError);
        }



        public static long MainThreadTime { get; private set; }
        private static void Update()
        {
            var watch = Stopwatch.StartNew();
            while (true)
            {
                if(Server == null || (Server != null && Server.IsDisposing))
                    break;

                Server.Update();



                if (watch.ElapsedMilliseconds < 10)
                {
                    MainThreadTime = watch.ElapsedMilliseconds;

                    var time = (int) (10 - watch.ElapsedMilliseconds);
                    if (time < 0) time = 0;
                    Thread.Sleep(time);
                }

                watch.Reset();
                watch.Start();
            }
        }


        private static void CatchErrorObject(object exceptionObject)
        {
            var exception = exceptionObject as Exception ?? new NotSupportedException("Unhandled exception doesn't derive from System.Exception: " + exceptionObject);

            var exceptionText = CatchError(exception);
            ReportErrorLocal(exceptionText);
            Stop();
        }
        private static string CatchError(Exception ex)
        {
            var errorLog = 
$@"[CODE]
PokeD.Server.Desktop Crash Log v {Assembly.GetExecutingAssembly().GetName().Version}

System specifications:
Operating system: {Environment.OSVersion} [{(Type.GetType("Mono.Runtime") != null ? "Mono" : ".NET")}]
Core architecture: {(Environment.Is64BitOperatingSystem ? "64 Bit" : "32 Bit")}
System language: {CultureInfo.CurrentCulture.EnglishName}
Logical processors: {Environment.ProcessorCount}

{BuildErrorStringRecursive(ex)}

You should report this error if it is reproduceable or you could not solve it by yourself.
Go To: {REPORTURL} to report this crash there.
[/CODE]";

            return errorLog;
        }
        private static string BuildErrorStringRecursive(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(
$@"Error information:
Type: {ex.GetType().FullName}
Message: {ex.Message}
HelpLink: {(string.IsNullOrWhiteSpace(ex.HelpLink) ? "Empty" : ex.HelpLink)}
Source: {ex.Source}
TargetSite : {ex.TargetSite}
CallStack:
{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                sb.AppendFormat($@"
--------------------------------------------------
InnerException:
{BuildErrorStringRecursive(ex.InnerException)}");
            }

            return sb.ToString();
        }
    }
}
