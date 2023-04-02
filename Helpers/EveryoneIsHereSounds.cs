using Terraria.Audio;

namespace EveryoneIsHere.Helpers
{
    public static class EveryoneIsHereSounds
    {
        public static SoundStyle ShrineActivate => new SoundStyle("EveryoneIsHere/RiskOfRain/Assets/Sounds/ShrineActivate") with { Volume = 0.8f };
        public static SoundStyle ShrineInsufficientFunds => new SoundStyle("EveryoneIsHere/RiskOfRain/Assets/Sounds/ShrineInsufficientFunds") with { Volume = 0.6f };
    }
}
