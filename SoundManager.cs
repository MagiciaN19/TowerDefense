using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;


namespace TowerDefense
{
    public class SoundManager
    {
        private Dictionary<GameSound, Uri> soundPaths = new Dictionary<GameSound, Uri>();

        // PULA ODTWARZACZY
        // Tworzymy 20 odtwarzaczy raz, a potem ich używamy w kółko.
        private List<MediaPlayer> playersPool = new List<MediaPlayer>();
        private int poolSize = 20;
        private int currentPlayerIndex = 0;

        public SoundManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            AddSound(GameSound.Build, Path.Combine(baseDir, "Sounds", "Build.wav"));
            AddSound(GameSound.Sell, Path.Combine(baseDir, "Sounds", "Sell.wav"));
            AddSound(GameSound.Shoot, Path.Combine(baseDir, "Sounds", "Shoot.wav"));
            AddSound(GameSound.Explosion, Path.Combine(baseDir, "Sounds", "Explosion.wav"));
            AddSound(GameSound.Win, Path.Combine(baseDir, "Sounds", "Win.wav"));
            AddSound(GameSound.Lose, Path.Combine(baseDir, "Sounds", "Lose.wav"));
            AddSound(GameSound.Error, Path.Combine(baseDir, "Sounds", "Error.wav"));
            AddSound(GameSound.Freeze, Path.Combine(baseDir, "Sounds", "Freeze.wav"));
            AddSound(GameSound.Sniper, Path.Combine(baseDir, "Sounds", "Sniper.wav"));

            // Inicjalizacja puli
            for (int i = 0; i < poolSize; i++)
            {
                playersPool.Add(new MediaPlayer());
            }
        }

        private void AddSound(GameSound sound, string path)
        {
            if (File.Exists(path))
            {
                soundPaths[sound] = new Uri(path);
            }
        }

        public void Play(GameSound sound)
        {
            if (soundPaths.ContainsKey(sound))
            {
                // Bierzemy kolejny odtwarzacz z puli zamiast tworzyć nowy
                MediaPlayer player = playersPool[currentPlayerIndex];

                // Przesuwamy indeks
                currentPlayerIndex++;
                if (currentPlayerIndex >= poolSize)
                {
                    currentPlayerIndex = 0;
                }

                try
                {
                    player.Open(soundPaths[sound]);
                    player.Volume = 0.5; 
                    player.Play();
                }
                catch { }
            }
        }
    }
}
