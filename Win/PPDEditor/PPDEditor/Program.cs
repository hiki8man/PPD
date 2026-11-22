using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDEditor.Models;
using PPDFramework;
using System;
using System.IO;
using System.Windows;

namespace PPDEditor
{
    static class Program
    {
        public static string AppDir
        {
            get
            {
                return AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        public static string PPDExePath
        {
            get
            {
                string dir = AppDir;
#if DEBUG
                for (int i = 0; i < 5; i++)
                {
                    dir = Directory.GetParent(dir).FullName;
                }
                dir = Path.Combine(dir, @"PPD\PPD\bin\x64\Debug");
#endif
                return Path.Combine(dir, "PPD.exe");
            }
        }

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            System.Environment.CurrentDirectory = AppDir;
            var errorHandler = new ErrorHandlerPPD();
            errorHandler.Initialize();
            var application = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            // 画面の操作の中で起きた例外は、以前と同じく知らせてから続ける（ゲームのフレームの中の例外は Game が止めて、下で処理する）
            application.DispatcherUnhandledException += (sender, e) =>
            {
                errorHandler.ProcessError(e.Exception);
                e.Handled = true;
            };
            if (!TrySelectStartupMode(args))
            {
                return;
            }
            application.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Game game = null;
            try
            {
                PPDSetting.Initialize();
                game = new MyGame(new PPDExecuteArg([]));
                game.Run(application);
                if (game.FatalException != null)
                {
                    throw new InvalidOperationException("An error occurred in the game loop.", game.FatalException);
                }
            }
            catch (Exception e)
            {
                SplashWindow.CloseSplash();
                errorHandler.ProcessError(e.InnerException ?? e);
                if (game != null)
                {
                    try
                    {
                        game.Window.ViewModel.RescueData();
                    }
                    catch
                    {

                    }
                    game.Window.ForceClose();
                }
            }
            finally
            {
                game?.Dispose();
                game?.Window.Dispose();
                GameView.DestroyRenderWindow();
            }
        }

        private static bool TrySelectStartupMode(string[] args)
        {
            if (args != null)
            {
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if ((args[i].Equals("--mode", StringComparison.OrdinalIgnoreCase)
                            || args[i].Equals("-m", StringComparison.OrdinalIgnoreCase))
                        && Enum.TryParse(args[i + 1], true, out EditorScriptMode cliMode))
                    {
                        PPDStaticSetting.ScriptMode = cliMode;
                        return true;
                    }
                }
            }
            if (PPDStaticSetting.RememberScriptMode)
            {
                return true;
            }
            var dialog = new ModeSelectWindow(PPDStaticSetting.ScriptMode, PPDStaticSetting.RememberScriptMode);
            if (dialog.ShowDialog() != true)
            {
                return false;
            }
            PPDStaticSetting.ScriptMode = dialog.SelectedMode;
            PPDStaticSetting.RememberScriptMode = dialog.RememberChoice;
            return true;
        }
    }
}
