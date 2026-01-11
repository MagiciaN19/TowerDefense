using System;
using System.Collections.Generic;
using System.Drawing;

namespace TowerDefense
{
    public class Bullet
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public int Damage { get; private set; }
        public Enemy Target { get; private set; }
        public int ExplosionRadius { get; private set; }

        private float speed = 12f;
        public bool HasHit { get; private set; } = false;
        public int FreezeDuration { get; private set; }


        public Bullet(float startX, float startY, Enemy target, int damage, int explosionRadius = 0, int freezeDuration = 0)
        {
            X = startX;
            Y = startY;
            Target = target;
            Damage = damage;
            ExplosionRadius = explosionRadius;
            FreezeDuration = freezeDuration;
        }

        public void Move(List<Enemy> allEnemies)
        {
            if (Target == null || !allEnemies.Contains(Target))
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

                // --- LOGIKA WYBUCHU ---
                if (ExplosionRadius > 0)
                {
                    // Szukamy wszystkich wrogów w promieniu wybuchu
                    foreach (var enemy in allEnemies)
                    {
                        // Liczymy odległość pocisku od wroga
                        float ex = enemy.X - X;
                        float ey = enemy.Y - Y;
                        float distToEnemy = (float)Math.Sqrt(ex * ex + ey * ey);

                        if (distToEnemy <= ExplosionRadius)
                        {
                            enemy.Health -= Damage;
                        }
                    }
                }
                else
                {
                    // Zwykły pocisk (tylko jeden cel)
                    Target.Health -= Damage;
                    if (FreezeDuration > 0)
                    {
                        Target.Freeze(FreezeDuration);
                    }
                }
            }
            else
            {
                X += (dx / distance) * speed;
                Y += (dy / distance) * speed;
            }
        }

        public void Draw(Graphics g)
        {
            if (ExplosionRadius > 0)
            {
                // Rakieta jest większa i ciemniejsza
                g.FillEllipse(Brushes.Black, X - 5, Y - 5, 10, 10);
            }
            else if (FreezeDuration > 0) // Lód
            {
                g.FillEllipse(Brushes.Cyan, X - 3, Y - 3, 6, 6);
            }
            else
            {
                // Zwykły pocisk
                g.FillEllipse(Brushes.Yellow, X - 3, Y - 3, 6, 6);
            }
        }
    }
}