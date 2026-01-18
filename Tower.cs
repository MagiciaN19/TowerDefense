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
        public int Level { get; protected set; } = 1;
        public int UpgradeCost { get; protected set; } = 50;
        public const int MaxLevel = 4;
        public int TotalSpent { get; protected set; } = 0;

        public Tower(int x, int y, int initialCost)
        {
            X = x;
            Y = y;
            TotalSpent = initialCost;
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

        public void Upgrade()
        {
            if (Level >= MaxLevel) return; 

            TotalSpent += UpgradeCost;
            Level++;
            Damage += 10;       
            Range += 20f;       
            UpgradeCost += 50;  
        }

        public abstract void Draw(Graphics g);

        protected void DrawLevelStars(Graphics g)
        {
            // 1. Konfiguracja wyglądu
            int starSize = 12;
            int spacing = 3;
            int count = Level - 1; 

            if (count <= 0) return;

            // 2. Matematyka centrowania
            int totalWidth = (count * starSize) + ((count - 1) * spacing);

            int startX = X - (totalWidth / 2);

            int startY = Y - (starSize / 2);

            // 3. Rysowanie
            for (int i = 0; i < count; i++)
            {
                int currentX = startX + i * (starSize + spacing);

                g.FillEllipse(Brushes.Gold, currentX, startY, starSize, starSize);
                g.DrawEllipse(new Pen(Color.Black, 2), currentX, startY, starSize, starSize);
            }
        }
    }
}
