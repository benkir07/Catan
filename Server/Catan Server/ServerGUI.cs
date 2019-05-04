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
        /// Initializes the GUI.
        /// </summary>
        public ServerGUI()
        {
            InitializeComponent();
            this.Text = "Catan Server";
        }

        /// <summary>
        /// Runs on GUI startup.
        /// Generated and Called by Windows forms.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerGUI_Load(object sender, EventArgs e)
        {
            EnterLog("Server Start");
        }

        /// <summary>
        /// Runs on GUI closing.
        /// Generated and Called by windows forms.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Server.Close();
        }

        /// <summary>
        /// Enters a log to the Log on GUI.
        /// </summary>
        /// <param name="message">The log to enter</param>
        public void EnterLog(string message)
        {
            try
            {
                this.Invoke(new Action(delegate { this.Log.Items.Add(DateTime.Now.ToLongTimeString() + " " + message); }));
            }
            catch
            { }
        }

        /// <summary>
        /// Makes all dice results in all games from now on a specific number, set by the gui.
        /// Generated and Called by Windows forms components.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultsButton_Click(object sender, EventArgs e)
        {
            Game.results = ((int)Result1.Value, (int)Result2.Value);
            Result1.Value = 1;
            Result2.Value = 1;
        }

        /// <summary>
        /// Lets the dice results in all games be random again.
        /// Generated and Called by Windows forms components.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset_Click(object sender, EventArgs e)
        {
            Game.results = (0, 0);
        }
    }
}
