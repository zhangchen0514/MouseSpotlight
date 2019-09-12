using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseSpotLight
{
    public partial class FrmHelp : Form
    {
        public FrmHelp()
        {
            InitializeComponent();
            //Console.WriteLine(System.Threading.Thread.CurrentThread.CurrentUICulture);
            //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("zh-CN");
            //Console.WriteLine(Properties.Strings.appName);
        }

        private void FrmHelp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void linkLblHome_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/zhangchen0514/MouseSpotLight");
        }

        private void FrmHelp_Load(object sender, EventArgs e)
        {
            linkLblHome.Text = Properties.Strings.appName;
            this.Text = Properties.Strings.help;
            lblQ.Text = Properties.Strings.onOff;
            lblA.Text = Properties.Strings.aspectP;
            lblS.Text = Properties.Strings.aspectM;
            lblZ.Text = Properties.Strings.sizeP;
            lblX.Text = Properties.Strings.sizeM;
            lblW.Text = Properties.Strings.reset;
            btnOK.Text = Properties.Strings.ok;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
