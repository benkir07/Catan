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
    /// <summary>
    /// The Windows Forms window class, responsible for showing and updating the Server's GUI.
    /// </summary>
    public partial class ServerGUI : Form
    {
        public int PlayersPerGame
        {
            get
            {
                return (int)ChoosePlayersPerGame.Value;
            }
        }
        
        /// <summary>
        /// Initializes the GUI
        /// </summary>
        public ServerGUI()
        {
            InitializeComponent();
            this.Text = "Catan Server";
        }

        /// <summary>
        /// Runs on GUI startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerGUI_Load(object sender, EventArgs e)
        {
            EnterLog("Server Start");
        }

        /// <summary>
        /// Runs on GUI closing
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Server.Close();
        }

        /// <summary>
        /// Enters a log to the Log on GUI
        /// </summary>
        /// <param name="message">The log to enter</param>
        public void EnterLog(string message)
        {
            this.Invoke(new Action(delegate { this.Log.Items.Add(DateTime.Now.ToLongTimeString() + " " + message); }));
        }
    }
}
