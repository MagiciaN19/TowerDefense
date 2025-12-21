using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public abstract class Tower
    {
        public int X { get; protected set; }
        public int Y { get; protected set; }

        public float Range { get; protected set; }
        public int Damage { get; protected set; }
        public int ReloadTime { get; protected set; }

        protected int currentCoolDown;

        public Tower(int x, int y)
        {
            X = x;
            Y = y; 
        }

        public virtual Bullet Update(List<Enemy> enemies)
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
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                if (dist <= Range && dist < minDistance)
                {
                    minDistance = dist;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null)
            {
                currentCoolDown = ReloadTime;
                return new Bullet(X, Y, nearestEnemy, Damage);
            }

            return null;

        }

        public abstract void Draw(Graphics g);
    }
}
