module DonkerJelliesFrozenJelly

using ..Ahorn, Maple

@mapdef Entity "DonkerJellies/FrozenJelly" FrozenJelly(x::Integer, y::Integer, tutorial::Bool=false, spriteDirectory::String="objects/glider", glow::Bool=false)

const placements = Ahorn.PlacementDict(
    "Frozen Jelly (Donker Jellies)" => Ahorn.EntityPlacement(
        FrozenJelly
    )
)

function Ahorn.selection(entity::FrozenJelly)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(get(entity, "spriteDirectory", "objects/glider") * "/idle0", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FrozenJelly, room::Maple.Room)
    Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/glider") * "/idle0", 0, 0)
end

end
