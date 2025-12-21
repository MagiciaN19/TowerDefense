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
        public float Speed { get; protected set; }
        public int Health { get; protected set; }
        protected int Size = 30;
        private List<Point> waypoints;
        private int currentWaypointIndex = 0;

        public Enemy(List<Point> path)
        {
            this.waypoints = path;
            if (waypoints.Count > 0)
            {
                X = waypoints[0].X;
                Y = waypoints[0].Y;
            }
        }

        public virtual void Move()
        {
            if (currentWaypointIndex >= waypoints.Count - 1)
                return;

            Point target = waypoints[currentWaypointIndex + 1];
            float deltaX = target.X - X;
            float deltaY = target.Y - Y;
            float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance < Speed)
            {
                X = target.X;
                Y = target.Y;
                currentWaypointIndex++;
            }
            else
            {
                X += (deltaX / distance) * Speed;
                Y += (deltaY / distance) * Speed;
            }
        }

        public abstract void Draw(Graphics g);
    }
}
