module LifeOnTheMoonCustomMoonCreature

using ..Ahorn, Maple

@mapdef Entity "LifeOnTheMoon/CustomMoonCreature" CustomMoonCreature(x::Integer, y::Integer, centerColor::String="ffffff", tailColor::String="000000", followPlayer::Bool=true)

const placements = Ahorn.PlacementDict(
    "Moon Creature (Custom) (Life On The Moon)" => Ahorn.EntityPlacement(
        CustomMoonCreature
    )
)

sprite = "scenery/moon_creatures/tiny05"

function Ahorn.selection(entity::CustomMoonCreature)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomMoonCreature, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
