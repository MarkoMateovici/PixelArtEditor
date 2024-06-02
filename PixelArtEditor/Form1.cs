using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PixelArtEditor
{
    public partial class Form1 : Form
    {
        private Bitmap _canvas;
        private Color _currentColor = Color.Black;
        private bool _isDrawing = false;
        private bool _isErasing = false;
        private int _pixelSize = 20;
        private Stack<Bitmap> _undoStack = new Stack<Bitmap>();
        private Stack<Bitmap> _redoStack = new Stack<Bitmap>();
        private bool _showGrid = true;
        private Bitmap _background;

        public Form1()
        {
            InitializeComponent();
            InitializeCanvas();
        }

        private void InitializeCanvas()
        {
            _canvas = new Bitmap(panel1.Width, panel1.Height);
            _background = CreateCheckerboardBackground(panel1.Width, panel1.Height, _pixelSize);
            using (Graphics g = Graphics.FromImage(_canvas))
            {
                g.Clear(Color.Transparent);
            }
            panel1.BackgroundImage = _background;
            SaveCanvasState();
        }

        private Bitmap CreateCheckerboardBackground(int width, int height, int cellSize)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                bool toggle = false;
                for (int y = 0; y < height; y += cellSize)
                {
                    for (int x = 0; x < width; x += cellSize)
                    {
                        g.FillRectangle(toggle ? Brushes.LightGray : Brushes.DarkGray, x, y, cellSize, cellSize);
                        toggle = !toggle;
                    }
                    toggle = !toggle;
                }
            }
            return bmp;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_canvas, Point.Empty);
            if (_showGrid)
            {
                DrawGrid(e.Graphics, panel1.Width, panel1.Height, _pixelSize);
            }
        }

        private void DrawGrid(Graphics g, int width, int height, int cellSize)
        {
            Pen pen = new Pen(Color.Black, 1);
            for (int y = 0; y < height; y += cellSize)
            {
                g.DrawLine(pen, 0, y, width, y);
            }
            for (int x = 0; x < width; x += cellSize)
            {
                g.DrawLine(pen, x, 0, x, height);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            _isDrawing = true;
            DrawOrErasePixel(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                DrawOrErasePixel(e.X, e.Y);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            _isDrawing = false;
            SaveCanvasState();
        }

        private void DrawOrErasePixel(int x, int y)
        {
            int gridX = x / _pixelSize;
            int gridY = y / _pixelSize;

            using (Graphics g = Graphics.FromImage(_canvas))
            {
                g.FillRectangle(new SolidBrush(_isErasing ? Color.Transparent : _currentColor), gridX * _pixelSize, gridY * _pixelSize, _pixelSize, _pixelSize);
            }
            panel1.Invalidate();
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _currentColor = colorDialog1.Color;
                _isErasing = false;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromImage(_canvas))
            {
                g.Clear(Color.Transparent);
            }
            panel1.Invalidate();
            SaveCanvasState();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _canvas.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _canvas = new Bitmap(openFileDialog1.FileName);
                panel1.BackgroundImage = _background;
                panel1.Invalidate();
                SaveCanvasState();
            }
        }

        private void btnErase_Click(object sender, EventArgs e)
        {
            _isErasing = true;
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            if (_undoStack.Count > 1)
            {
                _redoStack.Push(_undoStack.Pop());
                _canvas = new Bitmap(_undoStack.Peek());
                panel1.Invalidate();
            }
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                _undoStack.Push(_redoStack.Pop());
                _canvas = new Bitmap(_undoStack.Peek());
                panel1.Invalidate();
            }
        }

        private void SaveCanvasState()
        {
            _undoStack.Push(new Bitmap(_canvas));
            _redoStack.Clear();
        }

        private void btnToggleGrid_Click(object sender, EventArgs e)
        {
            _showGrid = !_showGrid;
            panel1.Invalidate();
        }
    }
}
