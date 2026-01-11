using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public abstract class Enemy
    {
        public float X { get; protected set; }
        public float Y { get; protected set; }
        private float baseSpeed;
        public float Speed
        {
            get
            {
                if (freezeTimer > 0) return baseSpeed * 0.5f;
                return baseSpeed;
            }
            protected set { baseSpeed = value; }
        }
        private int freezeTimer = 0;
        public int Health { get; set; }
        public int MaxHealth { get; protected set; }
        protected int Size;
        private List<Point> waypoints;
        private int currentWaypointIndex;
        public bool HasReachedEnd => waypoints != null && currentWaypointIndex >= waypoints.Count - 1;
        public int Reward { get; protected set; }

        public Enemy(List<Point> path)
        {
            this.waypoints = path;
            if (waypoints != null && waypoints.Count > 0)
            {
                X = waypoints[0].X;
                Y = waypoints[0].Y;
            }
        }

        public void Freeze(int duration)
        {
            freezeTimer = duration;
        }

        public virtual void Move()
        {
            if (freezeTimer > 0)
            {
                freezeTimer--;
            }

            if (HasReachedEnd) return;
            
            if (waypoints == null || currentWaypointIndex >= waypoints.Count - 1)
                return;

            Point target = waypoints[currentWaypointIndex + 1];

            float dx = target.X - X;
            float dy = target.Y - Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < Speed)
            {
                X = target.X;
                Y = target.Y;
                currentWaypointIndex++;
            }
            else
            {
                X += (dx / distance) * Speed;
                Y += (dy / distance) * Speed;
            }
        }

        protected void DrawHealthBar(Graphics g)
        {
            // Obliczamy procent życia (0.0 do 1.0)
            float healthPercentage = (float)Health / MaxHealth;
            if (healthPercentage < 0) healthPercentage = 0;

            // Ustawienia paska
            int barWidth = Size;          // Pasek szeroki jak wróg
            int barHeight = 5;
            int barX = (int)X - Size / 2; // Pozycja X (wyrównana do wroga)
            int barY = (int)Y - Size / 2 - 10; // Pozycja Y (troszkę nad głową)

            g.FillRectangle(Brushes.Red, barX, barY, barWidth, barHeight);

            int greenWidth = (int)(barWidth * healthPercentage);
            g.FillRectangle(Brushes.Lime, barX, barY, greenWidth, barHeight);

            g.DrawRectangle(Pens.Black, barX, barY, barWidth, barHeight);
        }
        public abstract void Draw(Graphics g);
    }
}
