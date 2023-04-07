using EasyPacketsLib;
using EveryoneIsHere.Helpers;
using EveryoneIsHere.RiskOfRain.Common.EasyPackets;
using EveryoneIsHere.RiskOfRain.Content.Buffs;
using EveryoneIsHere.RiskOfRain.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EveryoneIsHere.RiskOfRain.Content.Tiles;

public class SacrificeShrine : ModTile
{
	private readonly int[] SacrificeShrineItems = { ModContent.ItemType<OddlyShapedOpal>(), ItemID.LavaCharm, ItemID.ObsidianRose, ItemID.MagmaStone };

	private static bool IsShrineActive(int i, int j) => TileUtils.TryGetTileEntityAs(i, j, out SacrificeShrine_TileEntity chanceShrineEntity) && chanceShrineEntity.Active;

	public override void SetStaticDefaults() {
		Main.tileShine2[Type] = true;
		Main.tileShine[Type] = 1000;
		Main.tileFrameImportant[Type] = true;
		Main.tileOreFinderPriority[Type] = 500;
		Main.tileSpelunker[Type] = true;
		TileID.Sets.HasOutlines[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<SacrificeShrine_TileEntity>().Hook_AfterPlacement, -1, 0, false);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.addTile(Type);

		DustType = DustID.Stone;

		AddMapEntry(new Color(144, 148, 144), CreateMapEntryName());
	}

	public override bool CanKillTile(int i, int j, ref bool blockDamaged) => !IsShrineActive(i, j);

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

	public override void KillMultiTile(int i, int j, int frameX, int frameY) {
		Point16 origin = TileUtils.GetTileOrigin(i, j);
		ModContent.GetInstance<SacrificeShrine_TileEntity>().Kill(origin.X, origin.Y);
	}

	public override void MouseOver(int i, int j) {
		if (!IsShrineActive(i, j) || !TileUtils.TryGetTileEntityAs(i, j, out SacrificeShrine_TileEntity sacrificeShrineEntity)) {
			return;
		}

		Player player = Main.LocalPlayer;

		player.cursorItemIconID = -1;
		player.cursorItemIconText = $"50% [i:{ItemID.LifeCrystal}]";
		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
	}

	public override bool RightClick(int i, int j) {
		if (!IsShrineActive(i, j) || !TileUtils.TryGetTileEntityAs(i, j, out SacrificeShrine_TileEntity sacrificeShrineEntity)) {
			return false;
		}

		Player player = Main.LocalPlayer;
		if (player.HasBuff(ModContent.BuffType<LifeDebt>())) {
			SoundEngine.PlaySound(EveryoneIsHereSounds.ShrineInsufficientFunds);
			return false;
		}

		Item newItem = GeneralUtils.NewItemFromTile(new EntitySource_TileInteraction(player, i, j), i, j, Main.rand.Next(SacrificeShrineItems));
		sacrificeShrineEntity.Active = false;
		player.AddBuff(ModContent.BuffType<LifeDebt>(), 20 * 60 * 60 /* 20 minutes */);

		if (Main.netMode == NetmodeID.MultiplayerClient) {
			NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem.whoAmI, 1f);
			Mod.SendPacket(new SyncSacrificeShrineTileEntityPacket(sacrificeShrineEntity.Position.X, sacrificeShrineEntity.Position.Y, sacrificeShrineEntity.Active), forward: true);
		}

		// TODO: Visuals

		SoundEngine.PlaySound(EveryoneIsHereSounds.ShrineActivate);

		return true;
	}
}

public class SacrificeShrine_TileEntity : ModTileEntity
{
	public bool Active { get; set; } = true;

	public override bool IsTileValidForEntity(int x, int y) => Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<SacrificeShrine>();

	public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
		TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
		int topLeftX = i - tileData.Origin.X;
		int topLeftY = j - tileData.Origin.Y;

		if (Main.netMode == NetmodeID.MultiplayerClient) {
			NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, tileData.Width, tileData.Height);
			NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: type);
			return -1;
		}

		return Place(topLeftX, topLeftY);
	}

	public override void OnNetPlace() {
		NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
	}
}