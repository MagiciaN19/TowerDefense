using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class RocketTower : Tower
    {
        public static int Cost = 100;

        public RocketTower(int x, int y) : base(x, y, Cost)
        {
            Range = 150f;      // Średni zasięg
            Damage = 40;       // Obrażenia mniejsze niż snajper, ale obszarowe!
            ReloadTime = 90;   // Wolne przeładowanie (co 3 sekundy)
        }

        public override Bullet Update(List<Enemy> enemies)
        {
            if (currentCoolDown > 0)
            {
                currentCoolDown--;
                return null;
            }

            Enemy nearestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                float dx = enemy.X - X;
                float dy = enemy.Y - Y;
                float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);

                if (dist <= Range && dist < minDistance)
                {
                    minDistance = dist;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null)
            {
                currentCoolDown = ReloadTime;
                return new Bullet(X, Y, nearestEnemy, Damage, 50);
            }

            return null;
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Purple, X - 20, Y - 20, 40, 40);
            g.DrawRectangle(Pens.Black, X - 20, Y - 20, 40, 40);

            g.FillEllipse(Brushes.Gray, X - 10, Y - 10, 20, 20);

            DrawLevelStars(g);
        }
    }
}
