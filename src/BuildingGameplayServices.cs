using Orion.Api;
using Orion.Api.Items;
using Orion.Gameplay;
using Orion.Plugins;
using Orion.Player;
using Orion.Protocol.Types;
using ApiBlockPos = Orion.Api.Math.BlockPos;
using ProtoBlockPos = Orion.Protocol.Types.BlockPos;

namespace OrionBuilding;

/// <summary>
/// Host facade for block place / item-use-on-block.
/// </summary>
public sealed class BuildingGameplayServices : IBuildingApi, IPlayerBlockUseHandler
{
    const byte UseItemTriggerInitial = 1;
    const byte UseItemClientPredictionPlace = 1;

    public IPlayerBlockUseHandler BlockUse => this;

    public bool TryUseOnBlock(IPlayer player, ApiBlockPos blockPos, int face, ApiBlockPos placePos, IItemStack? held)
    {
        _ = placePos;
        _ = held;
        Player concrete = RequirePlayer(player);
        UseItemInventoryTransactionData data = BuildTransaction(concrete, blockPos, face);
        return BlockUseHandler.TryUseOnBlock(concrete, data);
    }

    public bool TryUseOnAir(IPlayer player, IItemStack? held)
    {
        _ = held;
        Player concrete = RequirePlayer(player);
        UseItemInventoryTransactionData data = BuildTransaction(concrete, new ApiBlockPos(0, 0, 0), face: 0);
        return BlockUseHandler.TryUseOnAir(concrete, data);
    }

    static UseItemInventoryTransactionData BuildTransaction(Player player, ApiBlockPos blockPos, int face)
    {
        int hotBarSlot = 0;
        if (PluginHost.Services.TryGet(out IPlayerInventoryService? inventory)
            && inventory is not null
            && inventory.TryGetAccess(player, out IPlayerInventoryAccess? access)
            && access is not null)
        {
            hotBarSlot = access.SelectedSlot;
        }

        return new UseItemInventoryTransactionData
        {
            TriggerType = UseItemTriggerInitial,
            ClientPrediction = UseItemClientPredictionPlace,
            BlockPosition = new ProtoBlockPos { X = blockPos.X, Y = blockPos.Y, Z = blockPos.Z },
            BlockFace = (byte)Math.Clamp(face, 0, 255),
            HotBarSlot = hotBarSlot,
            Position = player.Position,
            ClickedPosition = new Vec3f(),
            BlockRuntimeId = 0
        };
    }

    static Player RequirePlayer(IPlayer player) =>
        player as Player ?? throw new ArgumentException("Player must be an Orion.Player.Player.", nameof(player));
}
