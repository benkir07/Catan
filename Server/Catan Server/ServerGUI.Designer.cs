namespace Catan_Server
{
    partial class ServerGUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Log = new System.Windows.Forms.ListBox();
            this.LogLabel = new System.Windows.Forms.Label();
            this.ChoosePlayersPerGame = new System.Windows.Forms.NumericUpDown();
            this.PlayersAmountLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ChoosePlayersPerGame)).BeginInit();
            this.SuspendLayout();
            // 
            // Log
            // 
            this.Log.FormattingEnabled = true;
            this.Log.HorizontalScrollbar = true;
            this.Log.Location = new System.Drawing.Point(12, 43);
            this.Log.Name = "Log";
            this.Log.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.Log.Size = new System.Drawing.Size(206, 368);
            this.Log.TabIndex = 3;
            // 
            // LogLabel
            // 
            this.LogLabel.AutoSize = true;
            this.LogLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.LogLabel.Location = new System.Drawing.Point(12, 13);
            this.LogLabel.Name = "LogLabel";
            this.LogLabel.Size = new System.Drawing.Size(124, 26);
            this.LogLabel.TabIndex = 5;
            this.LogLabel.Text = "Server Log:";
            // 
            // ChoosePlayersPerGame
            // 
            this.ChoosePlayersPerGame.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.ChoosePlayersPerGame.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ChoosePlayersPerGame.Location = new System.Drawing.Point(268, 43);
            this.ChoosePlayersPerGame.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.ChoosePlayersPerGame.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ChoosePlayersPerGame.Name = "ChoosePlayersPerGame";
            this.ChoosePlayersPerGame.Size = new System.Drawing.Size(120, 20);
            this.ChoosePlayersPerGame.TabIndex = 6;
            this.ChoosePlayersPerGame.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // PlayersAmountLabel
            // 
            this.PlayersAmountLabel.AutoSize = true;
            this.PlayersAmountLabel.Location = new System.Drawing.Point(268, 24);
            this.PlayersAmountLabel.Name = "PlayersAmountLabel";
            this.PlayersAmountLabel.Size = new System.Drawing.Size(94, 13);
            this.PlayersAmountLabel.TabIndex = 7;
            this.PlayersAmountLabel.Text = "Players Per Game:";
            // 
            // ServerGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.PlayersAmountLabel);
            this.Controls.Add(this.ChoosePlayersPerGame);
            this.Controls.Add(this.LogLabel);
            this.Controls.Add(this.Log);
            this.Name = "ServerGUI";
            this.Text = "ServerGUI";
            this.Load += new System.EventHandler(this.ServerGUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ChoosePlayersPerGame)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox Log;
        private System.Windows.Forms.Label LogLabel;
        private System.Windows.Forms.NumericUpDown ChoosePlayersPerGame;
        private System.Windows.Forms.Label PlayersAmountLabel;
    }
}