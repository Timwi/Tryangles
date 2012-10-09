using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tryangles
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F2 && !mainBoard.IsEmpty)
			{
				if (MessageBox.Show("Are you sure you want to start a new game?", "New game", MessageBoxButtons.YesNo) == DialogResult.Yes)
					mainBoard.ResetBoard();
				e.Handled = true;
			}
		}
	}
}
