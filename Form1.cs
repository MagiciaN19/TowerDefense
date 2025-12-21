using System.Drawing;
using System.Windows.Forms;

namespace TowerDefense
{
    public partial class Form1 : Form
    {
        private GameEngine gameEngine;
        public Form1()
        {
            InitializeComponent();
            gameEngine = new GameEngine();
            SetupForm();
            GameTimer.Start();
        }
        private void SetupForm()
        {
            this.ClientSize = new Size(gameEngine.GetMapWidth(), gameEngine.GetMapHeight());
            this.Text = "Gra Tower Defense";
            this.DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            gameEngine.Draw(e.Graphics);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            gameEngine.Update();
            this.Invalidate();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            gameEngine.TryPlaceTower(e.X, e.Y);
            this.Invalidate();
        }
    }
}

