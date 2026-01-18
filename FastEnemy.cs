using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class FastEnemy : Enemy
    {
        public FastEnemy(List<Point> path) : base(path)
        {
            Speed = 6.0f;
            Health = 30;
            MaxHealth = 30;
            Size = 20;
            Reward = 10;
        }

        public override void Draw(Graphics g)
        {
            g.FillEllipse(Brushes.Gold, X - Size / 2, Y - Size / 2, Size, Size);
            g.DrawEllipse(Pens.Black, X - Size / 2, Y - Size / 2, Size, Size);

            base.DrawHealthBar(g);
        }
    }
}
