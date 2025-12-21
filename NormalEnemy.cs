using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class NormalEnemy : Enemy
    {
        public NormalEnemy(List<Point> path) : base(path)
        {
            Speed = 3.0f;
            Health = 100;
        }
        public override void Draw(Graphics g)
        {
            g.FillEllipse(Brushes.Red, X - Size / 2, Y - Size / 2, Size, Size);
            g.DrawEllipse(Pens.Black, X - Size / 2, Y - Size / 2, Size, Size);
        }
    }
}
