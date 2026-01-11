using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class NormalEnemy : Enemy
    {
        public NormalEnemy(List<Point> path) : base(path)
        {
            Speed = 3.0f;
            Health = 100;
            MaxHealth = 100;
            Size = 30;
            Reward = 20;
        }
        public override void Draw(Graphics g)
        {
            g.FillEllipse(Brushes.Red, X - Size / 2, Y - Size / 2, Size, Size);
            g.DrawEllipse(Pens.Black, X - Size / 2, Y - Size / 2, Size, Size);

            base.DrawHealthBar(g);
        }
    }
}
