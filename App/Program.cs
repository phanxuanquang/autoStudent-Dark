﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            LoadingWindow loading = new LoadingWindow();
            Application.Run(loading);

            (bool, List<Package>, List<Package>) checkLastRun = Startup.ReadSchedule();
            if (checkLastRun.Item1)
            {
                installSchedule = checkLastRun.Item2;
                uninstallSchedule = checkLastRun.Item3;
            }

            if (loading.isDone)
            {
                mainUI = new MainUI();
                Application.Run(mainUI);
            }    
        }

        #region Declaration
        static public List<Package> software_Database;
        static public List<Package> software_System;
        static public MainUI mainUI;
        static public Setting setting;
        public enum ExitRunBackground
        {
            Startup,
            Waiting,
            None
        };
        public static ExitRunBackground SetStartup = ExitRunBackground.None;
        public static List<string> installName;
        public static List<string> uninstallName;
        public static List<Package> installSchedule;
        public static List<Package> uninstallSchedule;
        #endregion

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if ((setting.isSetTime && SetStartup == ExitRunBackground.Startup && DateTime.Now.Subtract(setting.timeSetter).TotalSeconds <= 0)
                || (Program.installName != null && Program.installName.Count > 0) || (Program.uninstallName != null && Program.uninstallName.Count > 0))
            {
                Startup.WriteSchedule(installName, uninstallName);
            }
        }

        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            //https://stackoverflow.com/questions/76993/how-to-double-buffer-net-controls-on-a-form/77233#77233
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }
    }
}
