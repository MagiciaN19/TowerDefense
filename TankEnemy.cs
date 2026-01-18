using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TowerDefense
{
    public class TankEnemy : Enemy
    {
        public TankEnemy(List<Point> path) : base(path)
        {
            Speed = 1.0f;   
            Health = 500;  
            MaxHealth = 500;
            Size = 45;      
            Reward = 50;    
        }

        public override void Draw(Graphics g)
        {
            g.FillEllipse(Brushes.DarkSlateGray, X - Size / 2, Y - Size / 2, Size, Size);
            g.DrawEllipse(Pens.Black, X - Size / 2, Y - Size / 2, Size, Size);

            g.DrawRectangle(Pens.Black, X - 10, Y - 10, 20, 20);

            base.DrawHealthBar(g);
        }
    }
}
