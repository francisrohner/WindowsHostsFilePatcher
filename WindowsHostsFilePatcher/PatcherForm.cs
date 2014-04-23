using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Principal;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
//using System.Runtime.InteropServices;
namespace WindowsHostsFilePatcher
{
    public partial class PatcherForm : Form
    {

        public string launchFile;
        public readonly string HOSTS_CONFIG = "BlockedHosts.hst";
        public readonly string HOSTS_FILE = "C:\\Windows\\System32\\drivers\\etc\\hosts";
        public readonly string HOSTS_FILE_BACKUP = "C:\\Windows\\System32\\drivers\\etc\\hostsBackup";
        public readonly string[] DOMAINS = new string[] { ".com", ".net", ".gov", ".org", ".us", ".cx", ".tv" };
        public readonly string[] BROWSERS = new string[] { "Google Chrome", "Opera", "Firefox", "Internet Explorer" };

        private const uint WM_DROPFILES = 0x233;
        private const uint WM_COPYDATA = 0x004A;
        private const uint WM_COPYGLOBALDATA = 0x0049;
        private const uint MSGFLT_ADD = 1;

        ArrayList hostsFileLines;
        public PatcherForm()
        {
            InitializeComponent();
        }
        public PatcherForm(string[] args)
        {
            //try
            //{
            //    MessageBox.Show(args[0]);
            //}
           // catch (Exception ex) { }
            if (args.Count() > 0 && !String.IsNullOrEmpty(args[0]) && File.Exists(args[0]))
            {
                launchFile = args[0];
                
                //if(File.Exists(launchFile))
                StreamReader reader = new StreamReader(launchFile);
                MessageBox.Show(launchFile + "\n" + reader.ReadLine());
                reader.Close();
            }
            InitializeComponent();
            
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void btnPatch_Click(object sender, EventArgs e)
        {
            if (!File.Exists(HOSTS_FILE_BACKUP) && File.Exists(HOSTS_FILE))
                File.Copy(HOSTS_FILE, HOSTS_FILE_BACKUP);

            if (File.Exists(HOSTS_FILE))
                File.Delete(HOSTS_FILE);

            StreamWriter hostsWriter = new StreamWriter(HOSTS_FILE, true);
            foreach (String line in hostsFileLines)
                hostsWriter.WriteLine(line);
            foreach (String site in listBox1.Items)
            {
                string redirect = radHome.Checked ? "127.0.0.1" : "0.0.0.0";
                hostsWriter.WriteLine(redirect + "\t" + site);
                hostsWriter.WriteLine(redirect + "\t" + site.Replace("www.", ""));
            }
            hostsWriter.Close();
            flushDNS();
        }
        private void flushDNS()
        {
            if (checkBox1.Checked)
                if (MessageBox.Show("This will close all Web Browsers, are you sure you'd like to continue?", "WindowsHostsFilePatcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {

                    foreach (Process process in Process.GetProcesses())
                        foreach (String browserName in BROWSERS)
                            if (process.ProcessName.Contains(browserName) || process.MainWindowTitle.Contains(browserName))
                                process.Kill();
                    Process.Start("ipconfig", "/flushdns");
                }
        }
        private void btnRestore_Click(object sender, EventArgs e)
        {
            File.Delete(HOSTS_FILE);
            File.Copy(HOSTS_FILE_BACKUP, HOSTS_FILE);
            flushDNS();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            addSite(textBox1.Text);
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text.Equals("<Type site here, press enter to append to blocked>"))
                textBox1.Clear();
        }
        private bool isHostString(string str)//Todo check
        {
            //Uri myUri;
            //if (Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out myUri))
            //    return myUri.IsWellFormedOriginalString();
            //else
            //    return false;
          //  }
          //  catch (Exception ex)
          //  {
           //     return false;
           // }           

            var urlCheck = new Regex("([a-zA-Z\\d]+://)?(\\w+:\\w+@)?([a-zA-Z\\d.-]+\\.[A-Za-z]{2,4})(:\\d+)?(/.*)?");
            return urlCheck.IsMatch(str, 0);

            //bool isEmpty = String.IsNullOrEmpty(str) && String.IsNullOrWhiteSpace(str);
            //if (isEmpty) return false;

            //foreach (string type in DOMAINS)
            //    if (str.Contains(type))
            //        return true;
            //return false;
        }
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                addSite(textBox1.Text);
            }
        }

        private void addSite(String site)
        {
            if (!isHostString(site)) return;

            foreach (string s in listBox1.Items)
                if (textBox1.Text.Equals(s))
                    return;

            listBox1.Items.Add(textBox1.Text);
            textBox1.Clear();
        }
        private void deleteSite()
        {
            int offset = 0;
            int[] selectedIndices = new int[listBox1.SelectedIndices.Count];
            listBox1.SelectedIndices.CopyTo(selectedIndices, 0);
            foreach (int i in selectedIndices)
                listBox1.Items.RemoveAt(i - offset++);
        }

        private void PatcherForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists(HOSTS_CONFIG))
                File.Delete(HOSTS_CONFIG);
            StreamWriter sitesWriter = new StreamWriter(HOSTS_CONFIG);

            foreach (string s in listBox1.Items)
                sitesWriter.WriteLine(s);
            sitesWriter.Close();
        }
        public void addHosts(List<String> hosts)
        {
            foreach (string s in hosts)
                if (!listBox1.Items.Contains(s))
                    listBox1.Items.Add(s);
        }

        private void PatcherForm_Load(object sender, EventArgs e)
        {
            //String testUrl = "google";
            //MessageBox.Show(testUrl + " " + isSiteString(testUrl));
            //String hostsFileLineEx = "102.54.94.97     rhino.acme.com          # source server";
            //String[] linePieces = hostsFileLineEx.Split(' ');
            //foreach (string piece in linePieces)
            //    if (isSiteString(piece))
            //        MessageBox.Show(piece);

            hostsFileLines = new ArrayList();
            if (!IsAdministrator())
            {
                MessageBox.Show("Please run as Administrator!", "WindowsHostsFilePatcher Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }

            if (File.Exists(HOSTS_FILE_BACKUP))
            {
                StreamReader hostsReader = new StreamReader(HOSTS_FILE_BACKUP);
                while (!hostsReader.EndOfStream)
                    hostsFileLines.Add(hostsReader.ReadLine());
                hostsReader.Close();
            }
            else if (File.Exists(HOSTS_FILE))
            {
                StreamReader hostsReader = new StreamReader(HOSTS_FILE);
                while (!hostsReader.EndOfStream)
                    hostsFileLines.Add(hostsReader.ReadLine());
                hostsReader.Close();
            }

            readHosts(HOSTS_CONFIG);

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            deleteSite();
        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete) deleteSite();
        }

        private void PatcherForm_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show(e.Data.ToString());
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show(e.Data.ToString());
        }

        private void readHosts(string fileName)
        {
            if (File.Exists(fileName))
            {
                listBox1.Items.Clear();
                StreamReader sitesReader = new StreamReader(fileName);
                while (!sitesReader.EndOfStream)
                    listBox1.Items.Add(sitesReader.ReadLine());
                sitesReader.Close();
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            UserControl hostsAdder = new HostsAdder(this);
            this.Controls.Add(hostsAdder);
            hostsAdder.BringToFront();
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Hosts files (*.hst)|*.hst|*.txtText files (*.txt)|*.txt|All files (*.*)|*.*";
            //openFileDialog.ShowDialog();
            //readHosts(openFileDialog.FileName);
            //openFileDialog.Dispose();
        }
    }
}
