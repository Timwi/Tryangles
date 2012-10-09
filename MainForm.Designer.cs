namespace Tryangles
{
	partial class MainForm
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
			this.mainBoard = new Tryangles.Board();
			this.SuspendLayout();
			// 
			// mainBoard
			// 
			this.mainBoard.BackColor = System.Drawing.Color.White;
			this.mainBoard.BoardHeight = 7;
			this.mainBoard.BoardSize = new System.Drawing.Size(9, 7);
			this.mainBoard.BoardWidth = 9;
			this.mainBoard.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainBoard.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.mainBoard.Location = new System.Drawing.Point(0, 0);
			this.mainBoard.Name = "mainBoard";
			this.mainBoard.Size = new System.Drawing.Size(562, 441);
			this.mainBoard.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(562, 441);
			this.Controls.Add(this.mainBoard);
			this.KeyPreview = true;
			this.Name = "MainForm";
			this.Text = "Tryangles";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			this.ResumeLayout(false);

		}

		#endregion

		private Board mainBoard;
	}
}

