module IronSmelteryDreamMirrorChangeInventory

using ..Ahorn, Maple

@mapdef Entity "IronSmelteryCampaign/DreamMirrorChangeInventory" DreamMirrorChangeInventory(x::Integer, y::Integer, targetInventory::String="Default", tpTo::String="roomName")

const placements = Ahorn.PlacementDict(
    "Dream Mirror (Change Inventory) (IronSmelteryCampaign)" => Ahorn.EntityPlacement(
        DreamMirrorChangeInventory
    )
)

frame = "objects/mirror/frame.png"
glass = "objects/mirror/glassbreak00.png"

function Ahorn.editingOptions(trigger::DreamMirrorChangeInventory)
    return Dict{String, Any}(
        "targetInventory" => sort(Maple.inventories)
    )
end

function Ahorn.selection(entity::DreamMirrorChangeInventory)
    x, y = Ahorn.position(entity)

    return Ahorn.coverRectangles([
        Ahorn.getSpriteRectangle(frame, x, y, jx=0.5, jy=1.0),
        Ahorn.getSpriteRectangle(glass, x, y, jx=0.5, jy=1.0)
    ])
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamMirrorChangeInventory, room::Maple.Room)
    Ahorn.drawSprite(ctx, glass, 0, 0, jx=0.5, jy=1.0)
    Ahorn.drawSprite(ctx, frame, 0, 0, jx=0.5, jy=1.0)
end

end
