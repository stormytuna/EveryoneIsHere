using EasyPacketsLib;
using EveryoneIsHere.Helpers;
using EveryoneIsHere.RiskOfRain.Common.EasyPackets;
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

public class ChanceShrine : ModTile
{
	private readonly int[] ChanceShrineItems = {
		// TODO: Add modded items here
		ItemID.CloudinaBottle,
		ItemID.BandofRegeneration,
		ItemID.HermesBoots
	};

	private static bool IsShrineActive(int i, int j) {
		if (TileUtils.TryGetTileEntityAs(i, j, out ChanceShrine_TileEntity chanceShrineEntity)) {
			return chanceShrineEntity.Active;
		}

		return false;
	}

	private static bool IsShrineInteractible(int i, int j) {
		if (TileUtils.TryGetTileEntityAs(i, j, out ChanceShrine_TileEntity chanceShrineEntity)) {
			return chanceShrineEntity.Active && chanceShrineEntity.InteractionCooldown <= 0;
		}

		return false;
	}

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
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ChanceShrine_TileEntity>().Hook_AfterPlacement, -1, 0, false);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.addTile(Type);

		DustType = DustID.Stone;

		AddMapEntry(new Color(144, 148, 144), CreateMapEntryName());
	}

	public override bool CanKillTile(int i, int j, ref bool blockDamaged) => !IsShrineActive(i, j);

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

	public override void KillMultiTile(int i, int j, int frameX, int frameY) {
		Point16 origin = TileUtils.GetTileOrigin(i, j);
		ModContent.GetInstance<ChanceShrine_TileEntity>().Kill(origin.X, origin.Y);
	}

	public override void MouseOver(int i, int j) {
		if (!IsShrineInteractible(i, j) || !TileUtils.TryGetTileEntityAs(i, j, out ChanceShrine_TileEntity chanceShrineEntity)) {
			return;
		}

		Player player = Main.LocalPlayer;

		player.cursorItemIconID = -1;
		player.cursorItemIconText = GeneralUtils.CoinValueToString(chanceShrineEntity.Price);
		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
	}

	public override bool RightClick(int i, int j) {
		if (!IsShrineInteractible(i, j) || !TileUtils.TryGetTileEntityAs(i, j, out ChanceShrine_TileEntity chanceShrineEntity)) {
			return false;
		}

		Player player = Main.LocalPlayer;
		if (!player.CanBuyItem(chanceShrineEntity.Price)) {
			SoundEngine.PlaySound(EveryoneIsHereSounds.ShrineInsufficientFunds);
			return false;
		}

		bool shrineAttemptSuccess = Main.rand.NextBool(2);

		if (shrineAttemptSuccess) {
			int newItemIndex = Item.NewItem(new EntitySource_TileInteraction(player, i, j), i * 16, j * 16, 16, 16, Main.rand.Next(ChanceShrineItems));
			Main.item[newItemIndex].noGrabDelay = 100;
			chanceShrineEntity.Active = false;

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItemIndex, 1f);
				Mod.SendPacket(new SyncChanceShrineTileEntityPacket(chanceShrineEntity.Position.X, chanceShrineEntity.Position.Y, chanceShrineEntity.Active, chanceShrineEntity.Price), forward: true);
			}
		} else {
			int newPrice = (int)(chanceShrineEntity.Price * 1.5f);
			chanceShrineEntity.Price = newPrice;

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				Mod.SendPacket(new SyncChanceShrineTileEntityPacket(chanceShrineEntity.Position.X, chanceShrineEntity.Position.Y, chanceShrineEntity.Active, chanceShrineEntity.Price), forward: true);
			}
		}

		player.BuyItem(chanceShrineEntity.Price);
		chanceShrineEntity.InteractionCooldown = 100;

		Point16 tileOrigin = TileUtils.GetTileOrigin(i, j);
		Vector2 tileOriginWorldPosition = tileOrigin.ToWorldCoordinates();
		Vector2 dustOriginPosition = tileOriginWorldPosition + new Vector2(8f, 8f);
		for (int iterator = 0; iterator < 30; iterator++) {
			float rotValue = iterator / 20f * MathHelper.TwoPi;
			Vector2 dustOffset = Vector2.UnitX.RotatedBy(rotValue) * 30f;
			Vector2 dustPosition = dustOffset + dustOriginPosition;
			int dustType = shrineAttemptSuccess ? DustID.GreenTorch : DustID.RedTorch;
			Dust newDust = Dust.NewDustPerfect(dustPosition, dustType);
			newDust.velocity = dustPosition.DirectionTo(dustOriginPosition) * 2f;
			newDust.noGravity = true;
			newDust.scale = 1.3f;
		}

		for (int iterator = 0; iterator < 10; iterator++) {
			Vector2 dustOffset = Main.rand.NextVector2Circular(8f, 8f);
			Vector2 dustPosition = dustOffset + dustOriginPosition;
			int dustType = shrineAttemptSuccess ? DustID.GreenTorch : DustID.RedTorch;
			Dust newDust = Dust.NewDustPerfect(dustPosition, dustType);
			newDust.velocity = Vector2.Zero;
			newDust.noGravity = true;
			newDust.scale = 1.3f;
		}

		SoundEngine.PlaySound(EveryoneIsHereSounds.ShrineActivate);

		return true;
	}
}

public class ChanceShrine_TileEntity : ModTileEntity
{
	public bool Active { get; set; } = true;
	public int Price { get; set; } = 100000;
	public int InteractionCooldown { get; set; }

	public override bool IsTileValidForEntity(int x, int y) => Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<ChanceShrine>();

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

	public override void Update() {
		InteractionCooldown--;
	}

	public override void OnNetPlace() {
		NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
	}
}