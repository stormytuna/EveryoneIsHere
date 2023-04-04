using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EveryoneIsHere.RiskOfRain.Content.Buffs;

public class LifeDebt : ModBuff
{
	private static float GetLifeMultiplier(int buffTime) => MathHelper.Lerp(1f, 0.5f, buffTime / (20f * 60f * 60f));

	public override void SetStaticDefaults() {
		Main.debuff[Type] = true;
		Main.persistentBuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare) {
		Player player = Main.LocalPlayer;

		int buffIndex = player.FindBuffIndex(Type);
		int buffTime = player.buffTime[buffIndex];
		float lifeLost = 1f - GetLifeMultiplier(buffTime);
		int lifeLostPercent = (int)(lifeLost * 100f);

		tip = $"{lifeLostPercent}% of maximum life lost!";
	}

	public override void Update(Player player, ref int buffIndex) {
		int buffTime = player.buffTime[buffIndex];
		float lifeMultiplier = GetLifeMultiplier(buffTime);
		player.statLifeMax2 = (int)(player.statLifeMax2 * lifeMultiplier);
	}
}