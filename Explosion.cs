using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class Explosion
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public int Radius { get; private set; }

        // Przezroczystość (255 = pełny kolor, 0 = niewidoczny)
        private int alpha = 150;

        public bool IsFinished => alpha <= 0;

        public Explosion(float x, float y, int radius)
        {
            X = x;
            Y = y;
            Radius = radius;
        }

        public void Update()
        {
            // Zmniejszamy widoczność co klatkę (efekt zanikania)
            alpha -= 15;
        }

        public void Draw(Graphics g)
        {
            if (alpha <= 0) return;

            // Tworzymy kolor z aktualną przezroczystością
            Color color = Color.FromArgb(alpha, Color.OrangeRed);
            using (Brush brush = new SolidBrush(color))
            {
                // Rysujemy koło wybuchu
                g.FillEllipse(brush, X - Radius, Y - Radius, Radius * 2, Radius * 2);
            }

            using (Pen pen = new Pen(Color.FromArgb(alpha, Color.Yellow), 2))
            {
                g.DrawEllipse(pen, X - Radius, Y - Radius, Radius * 2, Radius * 2);
            }
        }
    }
}
