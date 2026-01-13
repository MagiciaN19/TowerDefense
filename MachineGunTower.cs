using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class MachineGunTower : Tower
    {
        public static int Cost = 80;

        public MachineGunTower(int x, int y) : base(x, y)
        {
            Range = 120f;       // Mniejszy zasięg niż Snajper
            Damage = 15;        // Małe obrażenia (Snajper ma 50)
            ReloadTime = 10;    // Bardzo szybka! (Strzał co 10 klatek, Snajper ma 60)
        }

        public override void Draw(Graphics g)
        {
            Point[] points = {
                new Point(X, Y - 20), // Góra
                new Point(X + 20, Y), // Prawo
                new Point(X, Y + 20), // Dół
                new Point(X - 20, Y)  // Lewo
            };

            g.FillPolygon(Brushes.DarkOrange, points);
            g.DrawPolygon(Pens.Black, points);

            // Lufa
            g.FillRectangle(Brushes.Black, X - 5, Y - 25, 10, 20);

            DrawLevelStars(g);
        }
    }
}