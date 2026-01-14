using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public enum GameSound
    {
        Build,      // Postawienie wieży
        Sell,       // Sprzedaż wieży
        Shoot,      // Wystrzał CKM
        Freeze,     // Wystrzał zamrażający
        Sniper,    // Wystrzał snajperski
        Explosion,  // Wybuch rakiety
        Win,        // Wygrana
        Lose,       // Przegrana
        Error       // Brak złota / złe miejsce
    }
}
