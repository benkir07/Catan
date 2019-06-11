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
            this.Result1 = new System.Windows.Forms.NumericUpDown();
            this.Result2 = new System.Windows.Forms.NumericUpDown();
            this.ResultsButton = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ChoosePlayersPerGame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Result1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Result2)).BeginInit();
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
            2,
            0,
            0,
            0});
            this.ChoosePlayersPerGame.Name = "ChoosePlayersPerGame";
            this.ChoosePlayersPerGame.Size = new System.Drawing.Size(120, 20);
            this.ChoosePlayersPerGame.TabIndex = 6;
            this.ChoosePlayersPerGame.Value = new decimal(new int[] {
            2,
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
            // Result1
            // 
            this.Result1.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.Result1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Result1.Location = new System.Drawing.Point(442, 43);
            this.Result1.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.Result1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.Result1.Name = "Result1";
            this.Result1.Size = new System.Drawing.Size(120, 20);
            this.Result1.TabIndex = 8;
            this.Result1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // Result2
            // 
            this.Result2.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.Result2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Result2.Location = new System.Drawing.Point(568, 43);
            this.Result2.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.Result2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.Result2.Name = "Result2";
            this.Result2.Size = new System.Drawing.Size(120, 20);
            this.Result2.TabIndex = 9;
            this.Result2.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ResultsButton
            // 
            this.ResultsButton.Location = new System.Drawing.Point(516, 69);
            this.ResultsButton.Name = "ResultsButton";
            this.ResultsButton.Size = new System.Drawing.Size(88, 23);
            this.ResultsButton.TabIndex = 10;
            this.ResultsButton.Text = "Set up Results";
            this.ResultsButton.UseVisualStyleBackColor = true;
            this.ResultsButton.Click += new System.EventHandler(this.ResultsButton_Click);
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(504, 98);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(114, 23);
            this.Reset.TabIndex = 11;
            this.Reset.Text = "Randomize Results";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // ServerGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Reset);
            this.Controls.Add(this.ResultsButton);
            this.Controls.Add(this.Result2);
            this.Controls.Add(this.Result1);
            this.Controls.Add(this.PlayersAmountLabel);
            this.Controls.Add(this.ChoosePlayersPerGame);
            this.Controls.Add(this.LogLabel);
            this.Controls.Add(this.Log);
            this.Name = "ServerGUI";
            this.Text = "ServerGUI";
            this.Load += new System.EventHandler(this.ServerGUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ChoosePlayersPerGame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Result1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Result2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox Log;
        private System.Windows.Forms.Label LogLabel;
        private System.Windows.Forms.NumericUpDown ChoosePlayersPerGame;
        private System.Windows.Forms.Label PlayersAmountLabel;
        private System.Windows.Forms.NumericUpDown Result1;
        private System.Windows.Forms.NumericUpDown Result2;
        private System.Windows.Forms.Button ResultsButton;
        private System.Windows.Forms.Button Reset;
    }
}