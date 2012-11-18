using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace Tryangles
{
	public class Board : Control
	{
		public Board()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
			ResetBoard();
		}

		private int boardWidth = 10, boardHeight = 10;

		/// <summary>
		/// Gets or sets the board width.
		/// </summary>
		/// <remarks>Changing the board size will reset the game.</remarks>
		[Description("The width of the board, in number of columns.")]
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue(10)]
		public int BoardWidth
		{
			get { return boardWidth; }
			set
			{
				if (value < MinWidth || value > MaxWidth)
					throw new ArgumentOutOfRangeException("value");
				if (value != boardWidth)
				{
					boardWidth = value;
					ResetBoard();
				}
			}
		}
		/// <summary>
		/// Gets or sets the board height.
		/// </summary>
		/// <remarks>Changing the board size will reset the game.</remarks>
		[Description("The height of the board, in number of rows.")]
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue(10)]
		public int BoardHeight
		{
			get { return boardHeight; }
			set
			{
				if (value < MinHeight || value > MaxHeight)
					throw new ArgumentOutOfRangeException("value");
				if (value != boardHeight)
				{
					boardHeight = value;
					ResetBoard();
				}
			}
		}
		/// <summary>
		/// Gets or sets the board size.
		/// </summary>
		/// <remarks>Changing the board size will reset the game.</remarks>
		[Description("The size of the board, in number of columns and rows.")]
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue(typeof(Size), "10,10")]
		public Size BoardSize
		{
			get { return new Size(boardWidth, boardHeight); }
			set
			{
				if (value.Width < MinWidth || value.Width > MaxWidth)
					throw new ArgumentOutOfRangeException("value.Width");
				else if (value.Height < MinHeight || value.Height > MaxHeight)
					throw new ArgumentOutOfRangeException("value.Height");
				if (value.Width != boardWidth || value.Height != boardHeight)
				{
					boardWidth = value.Width;
					boardHeight = value.Height;
					ResetBoard();
				}
			}
		}
		/// <summary>
		/// Gets a bool that determines whether the board is empty.
		/// </summary>
		[Browsable(false)]
		public bool IsEmpty { get { return lines.Count == 0; } }

		private List<Line> lines = new List<Line>();
		private List<List<Line>> triangles = null;
		private short[] points = new short[0];

		[Browsable(false)]
		public bool HasTriangles { get { return triangles != null && triangles.Count > 0; } }

		private Point? firstPoint = null;
		private int highlightRow = -1, highlightColumn = -1;

		public void ResetBoard()
		{
			lines.Clear();
			points = new short[boardWidth * boardHeight];
			highlightColumn = highlightRow = -1;
			firstPoint = null;
			this.Refresh();
		}

		private void MakeMove(int x, int y)
		{
			if (firstPoint == null)
			{
				firstPoint = new Point(x, y);
				highlightColumn = highlightRow = -1;
			}
			else
			{
				var fp = firstPoint.Value;
				firstPoint = null;
				highlightColumn = highlightRow = -1;
				if (fp.X == x && fp.Y == y)
					return;
				var newLine = new Line(fp.X, fp.Y, x, y);
				// broken check for moves that intersect a junction:
				// newLine.GetGridPoints().Skip(1).SkipLast(1).Any(p => lines.Count(line => p == line.Start || p == line.End) > 1)
				if (lines.Any(line => line.IntersectsWith(newLine)))
					MessageBox.Show("You cannot play intersecting lines.");
				else
					AddLine(newLine);
			}
			this.Refresh();
		}

		private void AddLine(Line newLine)
		{
			lines.Add(newLine);
			MarkPoints(newLine);
			CheckForTriangles();
		}

		private void MarkPoints(Line line, short delta = 1)
		{
			foreach (var point in line.GetGridPoints())
				points[point.X + point.Y * boardWidth] += delta;
		}

		private void CheckForTriangles()
		{
			var lineSegments = lines.SelectMany(line =>
				line.GetGridPoints().ConsecutivePairs()
					.Select(pair => new Line(pair.Item1, pair.Item2))
			).ToList();
			triangles = FindTriangles(lineSegments);
		}

		private List<List<Line>> FindTriangles(List<Line> segments)
		{
			var list = new List<List<Line>>();
			FindTriangles(segments, list, new List<Line>(), 0, default(Line));
			return list;
		}

		private void FindTriangles(List<Line> segments, List<List<Line>> list, List<Line> cur, int vertices, Line lastLine)
		{
			if (vertices == 3)
				return;
			if (vertices == 2 && lastLine.X2 == cur[0].X1 && lastLine.Y2 == cur[0].Y1)
			{
				list.Add(cur.ToList());
				return;
			}
			foreach (var segment in segments)
			{
				if (cur.Contains(segment))
					continue;
				// Try to extend the current unfinished triangle in “cur”
				int targetX = segment.X2, targetY = segment.Y2;
				Line resultLine = segment;
				bool isCollinear = true;
				if (cur.Count == 0 || segment.JoinsUpWith(lastLine, ref targetX, ref targetY, ref resultLine, ref isCollinear))
				{
					cur.Add(segment);
					FindTriangles(segments, list, cur, isCollinear ? vertices : vertices + 1, resultLine);
					cur.RemoveAt(cur.Count - 1);
				}
			}
		}

		private void RemoveLastLine()
		{
			var line = lines[lines.Count - 1];
			lines.RemoveAt(lines.Count - 1); // remove last
			MarkPoints(line, -1);
			CheckForTriangles();
		}

		private void Draw(Graphics g)
		{
			var width = (boardWidth - 2) * Spacing;
			var height = (boardHeight - 2) * Spacing;

			var x = (Width - width) / 2;
			var y = (Height - height) / 2;

			g.SmoothingMode = SmoothingMode.AntiAlias;

			if (highlightRow != -1)
			{
				var rect = new Rectangle(x - 8, y + highlightRow * Spacing - 8, (boardWidth - 1) * Spacing + 16, 16);
				g.DrawRectangle(HighlightPen, rect);
				g.FillRectangle(HighlightBrush, rect);
			}
			if (highlightColumn != -1)
			{
				var rect = new Rectangle(x + highlightColumn * Spacing - 8, y - 8, 16, (boardHeight - 1) * Spacing + 16);
				g.DrawRectangle(HighlightPen, rect);
				g.FillRectangle(HighlightBrush, rect);
			}

			DrawBoard(g, x, y);

			if (firstPoint != null)
			{
				var fp = firstPoint.Value;
				g.FillEllipse(Brushes.Green, x + fp.X * Spacing - 4.0f, y + fp.Y * Spacing - 4.0f, 8.0f, 8.0f);
				g.DrawEllipse(HighlightPen, x + fp.X * Spacing - 7.0f, y + fp.Y * Spacing - 7.0f, 14.0f, 14.0f);
			}

			g.SmoothingMode = SmoothingMode.Default;
		}

		private void DrawBoard(Graphics g, int x, int y)
		{
			for (var c = 0; c < boardWidth; c++)
			{
				g.DrawString(char.ConvertFromUtf32(c + 'A'), this.Font, Brushes.Black,
					x + c * Spacing, y - Spacing, LabelFormat);
				for (var r = 0; r < boardHeight; r++)
				{
					if (c == 0)
						g.DrawString((r + 1).ToString(), this.Font, Brushes.Black,
							x - Spacing, y + r * Spacing, LabelFormat);
					g.FillEllipse(Brushes.Black, x + c * Spacing - 1.5f, y + r * Spacing - 1.5f, 3.0f, 3.0f);
				}
			}

			for (var i = 0; i < lines.Count; i++)
			{
				var line = lines[i];
				g.DrawLine(i == lines.Count - 1 ? LastLinePen : LinePen,
					x + line.X1 * Spacing, y + line.Y1 * Spacing,
					x + line.X2 * Spacing, y + line.Y2 * Spacing);
			}

			for (var i = 0; i < points.Length; i++)
				if (points[i] > 0)
					g.FillEllipse(Brushes.CornflowerBlue,
						x + (i % boardWidth) * Spacing - 2.5f,
						y + (i / boardWidth) * Spacing - 2.5f,
						5.0f, 5.0f);

			if (triangles != null)
				foreach (var triangle in triangles)
					foreach (var line in triangle)
						g.DrawLine(TrianglePen,
							x + line.X1 * Spacing, y + line.Y1 * Spacing,
							x + line.X2 * Spacing, y + line.Y2 * Spacing);

		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Draw(e.Graphics);
		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			var width = (boardWidth - 2) * Spacing;
			var height = (boardHeight - 2) * Spacing;

			// For the purposes of mouse clicks, each point shall be
			// considered the centre of a square that surrounds it.
			// If the click falls within ClickTolerance pixels of
			// the point, the point is considered clicked. Simple!

			var boardX = (Width - width) / 2;
			var boardY = (Height - height) / 2;

			var col = (e.X - boardX + Spacing / 2) / Spacing;
			var row = (e.Y - boardY + Spacing / 2) / Spacing;

			if (!(col < 0 || col >= boardWidth ||
				row < 0 || row >= boardHeight ||
				e.X < boardX + col * Spacing - ClickTolerance ||
				e.X > boardX + col * Spacing + ClickTolerance ||
				e.Y < boardY + row * Spacing - ClickTolerance ||
				e.Y > boardY + row * Spacing + ClickTolerance))
			{
				MakeMove(col, row); // Oh god I hope so.
			}

			base.OnMouseClick(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			var key = e.KeyCode;

			if (key >= Keys.A && key <= Keys.Z) // Column
			{
				int column = key - Keys.A;
				if (column < boardWidth)
				{
					if (highlightRow >= 0)
						MakeMove(column, highlightRow);
					else
					{
						highlightColumn = column;
						this.Refresh();
					}
					e.Handled = true;
				}
			}
			else if (key >= Keys.D0 && key <= Keys.D9 ||
				key >= Keys.NumPad0 && key <= Keys.NumPad9) // Row
			{
				// Treat 0 as row 10 (index 9).
				// Shift + key adds 10. This makes all rows reachable by keyboard. :)
				int row = key == Keys.D0 || key == Keys.NumPad0 ? 9 :
					key >= Keys.D0 && key <= Keys.D9 ? key - Keys.D1 :
					key - Keys.NumPad1;
				if (e.Shift)
					row += 10;
				if (row < boardHeight)
				{
					if (highlightColumn >= 0)
						MakeMove(highlightColumn, row);
					else
					{
						highlightRow = row;
						this.Refresh();
					}
					e.Handled = true;
				}
			}
			else if (key == Keys.Escape || key == Keys.Back)
			{
				if (highlightColumn != -1 || highlightRow != -1)
				{
					highlightColumn = highlightRow = -1;
					e.Handled = true;
				}
				else if (firstPoint != null)
				{
					firstPoint = null;
					e.Handled = true;
				}
				else if (key == Keys.Back && lines.Count > 0)
				{
					RemoveLastLine();
					e.Handled = true;
				}
				this.Refresh();
			}

			base.OnKeyDown(e);
		}

		private static readonly Pen LinePen = new Pen(Color.Blue, 2.5f);
		private static readonly Pen LastLinePen = new Pen(Color.CornflowerBlue, 2.5f);
		private static readonly Pen TrianglePen = new Pen(Color.Red, 3f);
		private static readonly StringFormat LabelFormat = new StringFormat()
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center,
		};

		private static readonly Pen HighlightPen = new Pen(Brushes.Green, 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		private static readonly Brush HighlightBrush = new SolidBrush(Color.FromArgb(64, 0, 255, 0));

		private const int MinWidth = 2;
		private const int MaxWidth = 26;
		private const int MinHeight = 2;
		private const int MaxHeight = 20;

		private const int Spacing = 44;
		private const int ClickTolerance = 15;

		private struct Line : IEquatable<Line>
		{
			public Line(int x1, int y1, int x2, int y2)
			{
				X1 = x1;
				X2 = x2;

				Y1 = y1;
				Y2 = y2;
			}
			public Line(Point p1, Point p2)
			{
				X1 = p1.X;
				X2 = p2.X;

				Y1 = p1.Y;
				Y2 = p2.Y;
			}

			public int X1, X2, Y1, Y2;

			public Point Start { get { return new Point(X1, Y1); } }
			public Point End { get { return new Point(X2, Y2); } }

			public bool JoinsUpWith(Line lastLine, ref int targetX, ref int targetY, ref Line resultLine, ref bool isCollinear)
			{
				if (X1 == lastLine.X2 && Y1 == lastLine.Y2)
				{
					targetX = X2;
					targetY = Y2;
					resultLine = this;
					isCollinear = (Y1 - lastLine.Y1) * (X2 - X1) == (Y2 - Y1) * (X1 - lastLine.X1);
					return true;
				}
				else if (X2 == lastLine.X2 && Y2 == lastLine.Y2)
				{
					targetX = X1;
					targetY = Y1;
					resultLine = new Line(X2, Y2, X1, Y1);
					isCollinear = (Y2 - lastLine.Y1) * (X1 - X2) == (Y1 - Y2) * (X2 - lastLine.X1);
					return true;
				}
				return false;
			}

			public bool IntersectsWith(Line other)
			{
				double mx = X2 - X1;
				double my = Y2 - Y1;
				double rmx = other.X2 - other.X1;
				double rmy = other.Y2 - other.Y1;
				double dx = other.X1 - X1;
				double dy = Y1 - other.Y1;

				double d = (mx * rmy - my * rmx);
				double n = (mx * dy + my * dx) / d;
				double q = (rmx * dy + rmy * dx) / d;

				return (n > 0 && n < 1 && q > 0 && q < 1);
			}

			public IEnumerable<Point> GetGridPoints()
			{
				int x1 = X1, y1 = Y1, x2 = X2, y2 = Y2;

				var dx = x2 - x1;
				var dy = y2 - y1;
				var gcd = Extensions.GCD(Math.Abs(dx), Math.Abs(dy));
				dx /= gcd;
				dy /= gcd;

				if (x1 > x2)
				{
					Extensions.Swap(ref x1, ref x2);
					dx = -dx;

					Extensions.Swap(ref y1, ref y2);
					dy = -dy;
				}

				if (y1 > y2)
					for (int x = x1, y = y1; x <= x2 && y >= y2; x += dx, y += dy)
						yield return new Point(x, y);
				else
					for (int x = x1, y = y1; x <= x2 && y <= y2; x += dx, y += dy)
						yield return new Point(x, y);
			}

			public bool Equals(Line other)
			{
				return this == other;
			}

			public override bool Equals(object obj)
			{
				return obj is Line && Equals((Line)obj);
			}

			public override int GetHashCode()
			{
				return unchecked(((X1 * 13 + X2) * 13 + Y1) * 13 + Y2);
			}

			public override string ToString()
			{
				return string.Format("[{0}, {1}] → [{2}, {3}]", X1, Y1, X2, Y2);
			}

			public static bool operator ==(Line a, Line b)
			{
				return a.X1 == b.X1 && a.Y1 == b.Y1 &&
					a.X2 == b.X2 && a.Y2 == b.Y2;
			}

			public static bool operator !=(Line a, Line b)
			{
				return !(a == b);
			}
		}
	}
}