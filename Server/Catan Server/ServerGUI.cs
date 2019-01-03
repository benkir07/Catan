using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Threading;

namespace Catan_Server
{
    public partial class ServerGUI : Form
    {
        public int PlayersPerGame
        {
            get
            {
                return (int)ChoosePlayersPerGame.Value;
            }
        }

        public ServerGUI()
        {
            InitializeComponent();
            this.Text = "Catan Server";
        }

        private void ServerGUI_Load(object sender, EventArgs e)
        {
            EnterLog("Server Start");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Server.Close();
        }

        public void EnterLog(string message)
        {
            this.BeginInvoke(new Action(() => { this.Log.Items.Add(DateTime.Now.ToLongTimeString() + " " + message); }));
        }
    }
}
