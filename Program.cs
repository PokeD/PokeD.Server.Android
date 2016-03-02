using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Aragas.Core.Wrappers;

using NDesk.Options;

using PCLStorage;

using PokeD.Core.Extensions;

using PokeD.Server.Android.WrapperInstances;

namespace PokeD.Server.Android
{
    public static class TaskExtension
    {
        public static TResult Wait<TResult>(this Task<TResult> task, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                task.Wait(cancellationTokenSource.Token);
                return task.Result;
            }
            catch(Exception ex) { throw task?.Exception ?? ex; }
        }
    }

    public static partial class Program
    {
        private struct FBReport
        {
            public string Description;
            public string ErrorCode;
            public DateTime Date;
        }


        private const string REPORTURL = "http://poked.github.io/report/";
        private const string FBURL = "https://poked.firebaseio.com/";
        private static Server Server { get; set; }

        private static bool NATForwardingEnabled { get; set; }


        static Program()
        {
            AppDomainWrapper.Instance = new AppDomainWrapperInstance();
            FileSystemWrapper.Instance = new FileSystemWrapperInstance();
            InputWrapper.Instance = new InputWrapperInstance();

            ConfigWrapper.Instance = new YamlConfigFactoryInstance();
            
            LuaWrapper.Instance = new MoonLuaWrapperInstance();

			DatabaseWrapper.Instance = new SQLiteDatabase();

            //LuaWrapper.Instance = new MoonLuaWrapperInstance();

            //NancyWrapper.Instance = new NancyWrapperInstance();

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

            ParseArgs(args);

            Start();
        }

        private static void ParseArgs(IEnumerable<string> args)
        {
            var options = new OptionSet();
            try
            {
                options = new OptionSet()
                    .Add("db|database=", "used {DATABASE_WRAPPER}.", ParseDatabase)
                    .Add("cf|config=", "used {CONFIG_WRAPPER}.", ParseConfig)
                    .Add("n|nat", "enables NAT port forwarding.", str => NATForwardingEnabled = true)
                    .Add("h|help", "show help.", str => ShowHelp(options));

                options.Parse(args);
            }
            catch (Exception ex) when (ex is OptionException || ex is FormatException)
            {
                Console.Write("PokeD.Server.Desktop: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try `PokeD.Server.Desktop --help' for more information.");

                ShowHelp(options, true);

                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.Read();
                Environment.Exit((int) ExitCodes.Success);
            }
        }
        private static void ShowHelp(OptionSet options, bool direct = false)
        {
            if (direct)
            {
                Console.WriteLine("Usage: PokeD.Server.Desktop [OPTIONS]");
                Console.WriteLine();
                Console.WriteLine("Options:");

                options.WriteOptionDescriptions(Console.Out);
            }
            else
            {
            }
        }
        private static void ParseDatabase(string database)
        {
            switch (database.ToLowerInvariant())
            {
                case "sql":
                case "sqldb":
                case "sqlite":
                case "sqlitedb":
                    DatabaseWrapper.Instance = new SQLiteDatabase();
                    break;

                default:
                    throw new FormatException("DATABASE_WRAPPER not correct.");
            }
        }
        private static void ParseConfig(string config)
        {
            switch (config.ToLowerInvariant())
            {
                case "json":
                    ConfigWrapper.Instance = new JsonConfigFactoryInstance();
                    break;

                case "yml":
                case "yaml":
                    ConfigWrapper.Instance = new YamlConfigFactoryInstance();
                    break;

                default:
                    throw new FormatException("CONFIG_WRAPPER not correct.");
            }
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
