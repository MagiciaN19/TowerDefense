using System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense;

namespace TowerDefense
{
    public class Bullet
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public int Damage { get; private set; }
        public Enemy Target { get; private set; }

        private float speed = 10f;

        public bool HasHit { get; private set; } = false;

        public Bullet(float startX, float startY, Enemy target, int damage)
        {
            X = startX;
            Y = startY;
            Target = target;
            Damage = damage;
        }

        public void Move()
        {
            if (Target == null)
            {
                HasHit = true;
                return;
            }

            float dx = Target.X - X;
            float dy = Target.Y - Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < speed)
            {
                HasHit = true;
                Target.Health -= Damage; // Zadajemy obrażenia wrogowi
            }
            else
            {
                // 3. Lot w stronę wroga
                X += (dx / distance) * speed;
                Y += (dy / distance) * speed;
            }
        }

        public void Draw(Graphics g)
        {
            // Rysujemy małą żółtą kropkę
            g.FillEllipse(Brushes.Yellow, X - 3, Y - 3, 6, 6);
        }
    }
}