using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.Win32;
using System.Diagnostics;


namespace AutoRunWB
{
    public partial class Form1 : Form
    {
        public class cfg
        {
            public string browser { get; set; }
            public string url_ { get; set; }
            public string run_ { get; set; }
        }

        public List<cfg> setting = new List<cfg>();
        public List<string> IE_url = new List<string>();
        public List<string> Chrome_url = new List<string>();

        public Form1()
        {
            InitializeComponent();
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AutoGenerateColumns = false;
            LoadConfig();
            foreach (cfg xy in setting)
            {
                if (xy.run_ == "F")
                {
                    dataGridView1.Rows.Add(null, xy.url_, false);
                }
                else
                {
                    dataGridView1.Rows.Add(null, xy.url_, true);
                }
                if (xy.browser == "Chrome")
                {
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value = "Chrome";
                }
                else
                {
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value = "Internet Explorer";
                }
             }     
        }




        //------------------------------------------------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        //------------------------------------------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            IE_url.Clear();
            Chrome_url.Clear();
            //IE
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if ((Convert.ToBoolean(row.Cells[2].Value) == true) && (Convert.ToString(row.Cells[0].Value) == "Internet Explorer"))
                {
                    IE_url.Add(Convert.ToString(row.Cells[1].Value));
                }
            }

            //Chrome
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if ((Convert.ToBoolean(row.Cells[2].Value) == true) && (Convert.ToString(row.Cells[0].Value) == "Chrome"))
                {

                   Chrome_url.Add(Convert.ToString(row.Cells[1].Value));
                }
            }

            //WYKONYWANIE
            //IE
            char znaczek = (char)39;
            int i = 0;

            string skrypt = "";

         
            foreach (string x in IE_url)
            {
                if (i > 0)
                {
                    skrypt = skrypt + "$ie.Navigate2(" + znaczek + x + znaczek + ", 0x1000)  " + "\r\n";
                }
                else
                {
                    skrypt = skrypt + "$ie.Navigate2(" + znaczek + x + znaczek + ") " + "\r\n";
                }
                i++;
            }
            if (i > 0)
            {
                RunScript("$ie = New-Object -ComObject InternetExplorer.Application \r\n" + skrypt + "\r\n $ie.Visible = $true");
            }

            skrypt = "";
            i = 0;
            foreach (string x in Chrome_url)
            {
                //"google.com" + " --new-window " + "google.com" + " --new-window --incognito";
                skrypt = skrypt + x + " --new-window ";
                i++;
            }
            if (i > 0)
            {
                Process process = new Process();
                string chromeAppFileName = ChromeAppFileName;
                if (string.IsNullOrEmpty(chromeAppFileName))
                {
                    throw new Exception("Could not find chrome.exe!");
                }

                process.StartInfo.FileName = chromeAppFileName; // @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                process.StartInfo.Arguments = skrypt;//"google.com" + " --new-window " + "google.com" + " --new-window --incognito";
                process.Start();
            }

        }
        //------------------------------------------------------------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add(null, "", true);         
            dataGridView1.Rows[dataGridView1.Rows.Count-1].Cells[0].Value = "Internet Explorer";
        }

        //------------------------------------------------------------------------------------------------------
        private string RunScript(string scriptText)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                Runspace runspace = RunspaceFactory.CreateRunspace();

                runspace.Open();
                Pipeline pipeline = runspace.CreatePipeline();
                pipeline.Commands.AddScript(scriptText);
                pipeline.Commands.Add("Out-String");

                Collection<PSObject> results = pipeline.Invoke();
                runspace.Close();
             
                foreach (PSObject obj in results)
                {
                    stringBuilder.AppendLine(obj.ToString());
                }
            }
            catch
            { }

            return stringBuilder.ToString();
        }



        private const string ChromeAppKey = @"\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";

        private static string ChromeAppFileName
        {
            get
            {
                return (string)(Registry.GetValue("HKEY_LOCAL_MACHINE" + ChromeAppKey, "", null) ??
                                Registry.GetValue("HKEY_CURRENT_USER" + ChromeAppKey, "", null));
            }
        }

        public static void OpenLink(string url)
        {
            string chromeAppFileName = ChromeAppFileName;
            if (string.IsNullOrEmpty(chromeAppFileName))
            {
                throw new Exception("Could not find chrome.exe!");
            }
            Process.Start(chromeAppFileName, url);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //  OpenLink("www.google.com --newwindow");
            string aktualny_cfg = "";
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[2].Value.ToString() == "True")
                {
                    aktualny_cfg += row.Cells[0].Value.ToString() + "<M>" + row.Cells[1].Value.ToString() + "<M>" + "T" + "\r\n";
                }
                else
                {
                    aktualny_cfg += row.Cells[0].Value.ToString() + "<M>" + row.Cells[1].Value.ToString() + "<M>" + "F" + "\r\n";
                }
            }
            System.IO.File.WriteAllText("config.cfg", aktualny_cfg);

            //Process process = new Process();
            // string chromeAppFileName = ChromeAppFileName;
            // if (string.IsNullOrEmpty(chromeAppFileName))
            // {
            //     throw new Exception("Could not find chrome.exe!");
            // }

            // process.StartInfo.FileName = chromeAppFileName; // @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            // process.StartInfo.Arguments = "google.com" + " --new-window " + "google.com" + " --new-window --incognito";
            // process.Start();
        }


        public void LoadConfig()
        {
            try
            {
                string[] stringSeparators = new string[] { "<M>" };
                string[] lines = System.IO.File.ReadAllLines("config.cfg");
                setting.Clear();
                int kolejny = 0;
                int licz = 0;
                string result = "";
                foreach (string line in lines)//ŁADOWANIE NAGŁÓWKA
                {
                    string[] xxx = line.Split(stringSeparators, StringSplitOptions.None);

                    setting.Add(new cfg()
                    {
                        browser = xxx[0],
                        url_ = xxx[1],
                        run_ = xxx[2]
                    });
                }
            }
            catch
            { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcesses();
            try
            {
                foreach (var process in processes)
                {
                    if (process.ProcessName == "chrome")
                    {
                        process.Kill();
                    }
                }
            }
            catch 
            { }
        
       
        }
    }
}
