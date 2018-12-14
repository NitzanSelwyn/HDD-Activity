using System;
using System.Drawing;
using System.Management;
using System.Threading;
using System.Windows.Forms;

namespace HDDActivity
{
    public partial class Form1 : Form
    {
        NotifyIcon hddIcon;
        Icon activeIcon;
        Icon idleIcon;
        Thread hddLedWorker;

        public Form1()
        {
            InitializeComponent();

            activeIcon = new Icon("Icons/HDD_Busy.ico");
            idleIcon = new Icon("Icons/HDD_Idle.ico");
            hddIcon = new NotifyIcon();
            hddIcon.Icon = idleIcon;
            hddIcon.Visible = true;

            MenuItem quitMenuItem = new MenuItem("Quit");
            MenuItem programeNameMenuItem = new MenuItem("HDD LED v0.1");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(programeNameMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            hddIcon.ContextMenu = contextMenu;

            quitMenuItem.Click += QuitMenuItem_Click;

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            hddLedWorker = new Thread(new ThreadStart(HardDriveActivityThread));
            hddLedWorker.Start();
        }

        private void QuitMenuItem_Click(object sender, System.EventArgs e)
        {
            hddLedWorker.Abort();
            hddIcon.Dispose();
            this.Close();
        }

        private void HardDriveActivityThread()
        {
            var driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            try
            {
                while (true)
                {
                    var driveDataClassCollection = driveDataClass.GetInstances();
                    foreach (var item in driveDataClassCollection)
                    {
                        if (item["Name"].ToString() == "_Total")
                        {
                            if (Convert.ToUInt64(item["DiskBytesPersec"]) > 0)
                            {
                                //Show Busy Icon
                                hddIcon.Icon = activeIcon;
                            }
                            else
                            {
                                // Show Idle Icon
                                hddIcon.Icon = idleIcon;
                            }
                        }
                    }

                    Thread.Sleep(500);
                }
            }
            catch (ThreadAbortException ex)
            {
                driveDataClass.Dispose();
                MessageBox.Show(ex.Message);
            }
        }
    }
}
