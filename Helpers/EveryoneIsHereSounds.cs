using Terraria.Audio;

namespace EveryoneIsHere.Helpers;

public static class EveryoneIsHereSounds
{
	public static SoundStyle ShrineActivate => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/ShrineActivate") {
		Volume = 0.8f,
		MaxInstances = 0
	};

	public static SoundStyle ShrineInsufficientFunds => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/ShrineInsufficientFunds") {
		Volume = 0.6f,
		MaxInstances = 0
	};

	public static SoundStyle AlphaConstruct_Open => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/AlphaConstruct/attack_open_0") {
		Volume = 0.6f,
		Variants = new[] { 1, 2, 3 },
		MaxInstances = 0
	};

	public static SoundStyle AlphaConstruct_Hide => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/AlphaConstruct/hide_0") {
		Volume = 0.6f,
		Variants = new[] { 1, 2, 3 },
		MaxInstances = 0
	};

	public static SoundStyle AlphaConstruct_ChargeUp => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/AlphaConstruct/attack_chargeup_0") {
		Volume = 0.6f,
		Variants = new[] { 1, 2, 3 },
		MaxInstances = 0
	};

	public static SoundStyle AlphaConstruct_Shoot => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/AlphaConstruct/attack_shoot_0") {
		Volume = 0.6f,
		Variants = new[] { 1, 2, 3, 4 },
		MaxInstances = 0
	};

	public static SoundStyle AlphaConstruct_Death => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/AlphaConstruct/death_0") {
		Volume = 0.6f,
		Variants = new[] { 1, 2, 3, 4 },
		MaxInstances = 0
	};

	public static SoundStyle AlphaConstruct_ProjectileLoop => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/AlphaConstruct/projectile_loop_01") {
		Volume = 0.6f,
		IsLooped = true,
		MaxInstances = 0
	};

	public static SoundStyle AlphaConstruct_ProjectileExplode => new("EveryoneIsHere/RiskOfRain/Assets/Sounds/AlphaConstruct/attack_explode_0") {
		Volume = 0.6f,
		Variants = new[] { 1, 2, 3 },
		MaxInstances = 0
	};
}