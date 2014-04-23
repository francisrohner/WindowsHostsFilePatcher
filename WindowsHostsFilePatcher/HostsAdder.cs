using System;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace WindowsHostsFilePatcher
{
    public partial class HostsAdder : UserControl
    {
        private PatcherForm mainForm;
        public HostsAdder(PatcherForm mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
        }       

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (checkedListBox1.GetItemChecked(0))
                for (int i = 1; i < checkedListBox1.Items.Count; i++)
                    checkedListBox1.SetItemChecked(i, true);
           for(int i = 1; i < checkedListBox1.Items.Count; i++)
           {
               //Add Items
           }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mainForm.addHosts(listBox1.Items.Cast<String>().ToList());
        }
        private List<String> readHostsConfigFile(String fileName)
        {
            List<String> hosts = new List<String>();
            StreamReader hostsConfigReader = new StreamReader(fileName);
            String line = hostsConfigReader.ReadLine();
            bool isHostsConfigFile = line.Equals("#Hosts Config#");
            if(isHostsConfigFile)
            {
                while (!hostsConfigReader.EndOfStream)
                    hosts.Add(hostsConfigReader.ReadLine());
            }
            else//Read hosts file and add things that aren't comments and exclude redirects
            {
                while (!hostsConfigReader.EndOfStream)
                    if ((line = hostsConfigReader.ReadLine()) .IndexOf("#") != 0)//Check if line is comment
                        Console.WriteLine("Unimplemented Code");//Todo finish

            }
            hostsConfigReader.Close();
            return hosts;
        }
    }
}
