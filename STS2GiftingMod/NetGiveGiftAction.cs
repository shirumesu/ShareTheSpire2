using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace STS2GiftingMod;

public struct NetGiveGiftAction : INetAction, IPacketSerializable
{
	public GiftKind kind;

	public int targetPlayerIndex;

	public int sourceIndex;

	public int fee;

	public GameAction ToGameAction(Player player)
	{
		return (GameAction)(object)new GiveGiftAction(player, kind, targetPlayerIndex, sourceIndex, fee);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt((int)kind, 8);
		writer.WriteInt(targetPlayerIndex, 8);
		writer.WriteInt(sourceIndex, 16);
		writer.WriteInt(fee, 16);
	}

	public void Deserialize(PacketReader reader)
	{
		kind = (GiftKind)reader.ReadInt(8);
		targetPlayerIndex = reader.ReadInt(8);
		sourceIndex = reader.ReadInt(16);
		fee = reader.ReadInt(16);
	}
}
