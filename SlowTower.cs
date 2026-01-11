using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class SlowTower : Tower
    {
        public static int Cost = 70;

        public SlowTower(int x, int y) : base(x, y)
        {
            Range = 130f;
            Damage = 5;       // Symboliczne obrażenia
            ReloadTime = 45;  // Średnia szybkostrzelność
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
                return new Bullet(X, Y, nearestEnemy, Damage, 0, 120);
            }
            return null;
        }

        public override void Draw(Graphics g)
        {
            // Błękitny romb lub kółko
            g.FillRectangle(Brushes.LightSkyBlue, X - 15, Y - 15, 30, 30);
            g.DrawRectangle(Pens.Blue, X - 15, Y - 15, 30, 30);

            // "Kryształ" w środku
            g.FillEllipse(Brushes.White, X - 5, Y - 5, 10, 10);
        }
    }
}