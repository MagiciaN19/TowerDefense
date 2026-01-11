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

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.MouseClick += Form1_MouseClick;
            this.MouseMove += (s, e) => { this.Invalidate(); };

            GameTimer.Start();
        }
        private void SetupForm()
        {
            this.ClientSize = new Size(gameEngine.GetScreenWidth(), gameEngine.GetMapHeight());

            this.Text = "Gra Tower Defense";
            this.DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Point mousePos = this.PointToClient(Cursor.Position);

            gameEngine.Draw(e.Graphics, mousePos);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            gameEngine.Update();
            this.Invalidate();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            gameEngine.HandleClick(e.X, e.Y);
            this.Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D1) // Klawisz "1"
            {
                gameEngine.SelectedTowerType = 1;
            }
            else if (e.KeyCode == Keys.D2) // Klawisz "2"
            {
                gameEngine.SelectedTowerType = 2;
            }
            else if (e.KeyCode == Keys.D3) // Klawisz "3"
            {
                gameEngine.SelectedTowerType = 3;
            }
            else if (e.KeyCode == Keys.D4) // Klawisz "4"
            {
                gameEngine.SelectedTowerType = 4;
            }
            else if (e.KeyCode == Keys.R) // Klawisz "R" - Restart gry
            {
                gameEngine.ResetGame();
            }
            else if (e.KeyCode == Keys.U) // Klawisz "U"
            {
                gameEngine.TryUpgradeTower();
            }

                this.Invalidate();
        }
    }
}

