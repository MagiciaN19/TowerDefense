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
        public int Health { get; set; }
        protected int Size;
        private List<Point> waypoints;
        private int currentWaypointIndex;
        public Enemy(List<Point> path)
        {
            this.waypoints = path;
            if (waypoints != null && waypoints.Count > 0)
            {
                X = waypoints[0].X;
                Y = waypoints[0].Y;
            }
        }
        public virtual void Move()
        {
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
        public abstract void Draw(Graphics g);
    }
}
