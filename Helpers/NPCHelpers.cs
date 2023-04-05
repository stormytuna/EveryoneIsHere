using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;

namespace EveryoneIsHere.Helpers;

public class NPCHelpers
{
	public static IEnumerable<NPC> FindNearbyNPCs(Vector2 searchCenter, float searchRadius, bool careAboutLineOfSight = false, List<int> excludedNPCs = null) {
		excludedNPCs ??= new List<int>();

		foreach (NPC npc in Main.npc.SkipLast(1).Where(npc => npc.CanBeChasedBy() && npc.WithinRange(searchCenter, searchRadius))) {
			// Doing more expensive checks here so we only do them on applicable npcs
			bool isExcluded = excludedNPCs.Contains(npc.whoAmI);
			bool isInLineOfSight = !careAboutLineOfSight || Collision.CanHitLine(npc.Center, 1, 1, searchCenter, 1, 1);
			if (!isExcluded && isInLineOfSight) {
				yield return npc;
			}
		}
	}

	/// <summary>
	///     Finds the closest NPC to the given search. Returns null if no NPC was found
	/// </summary>
	public static NPC FindClosestNPC(Vector2 searchCenter, float searchRadius, bool careAboutLineOfSight = false) {
		NPC closestNPC = null;
		float closestNPCRangeSquared = float.PositiveInfinity;

		for (int i = 0; i < Main.maxNPCs; i++) {
			NPC npc = Main.npc[i];

			float distanceSquared = npc.DistanceSQ(searchCenter);
			bool isValid = npc.CanBeChasedBy();
			bool isCloser = distanceSquared < closestNPCRangeSquared;
			bool isInRange = distanceSquared < searchRadius * searchRadius;
			if (!isValid || !isCloser || !isInRange) {
				continue;
			}

			// Doing this check down here to improve FPS
			bool isInLineOfSight = !careAboutLineOfSight || Collision.CanHitLine(npc.Center, 1, 1, searchCenter, 1, 1);
			if (!isInLineOfSight) {
				continue;
			}

			closestNPC = npc;
			closestNPCRangeSquared = distanceSquared;
		}

		return closestNPC;
	}
}