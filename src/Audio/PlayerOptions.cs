using System;

namespace rize.Audio
{
    public class PlayerOptions
    {
        public float Volume { get; set; }
        public TimeSpan FadeIn { get; set; }
        public TimeSpan FadeOut { get; set; }
        public int PacketLoss { get; set; }
        public bool AutoNext { get; set; }

        public PlayerOptions()
        {
            Volume = 1.0f;
            FadeIn = TimeSpan.FromSeconds(5);
            FadeOut = TimeSpan.FromSeconds(8);
            PacketLoss = 0;
            AutoNext = true;
        }
    }
}
