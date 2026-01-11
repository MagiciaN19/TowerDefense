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
            Speed = 1.0f;   // Bardzo wolny
            Health = 500;   // 5x więcej życia niż Normalny (10 strzałów snajpera!)
            MaxHealth = 500;
            Size = 45;      // Duży, łatwy do trafienia
            Reward = 50;    // Wysoka nagroda za pokonanie
        }

        public override void Draw(Graphics g)
        {
            // ciemnoszary "czołg"
            g.FillEllipse(Brushes.DarkSlateGray, X - Size / 2, Y - Size / 2, Size, Size);
            g.DrawEllipse(Pens.Black, X - Size / 2, Y - Size / 2, Size, Size);

            // Dodatkowy element graficzny, "pancerz" w środku
            g.DrawRectangle(Pens.Black, X - 10, Y - 10, 20, 20);

            base.DrawHealthBar(g);
        }
    }
}
