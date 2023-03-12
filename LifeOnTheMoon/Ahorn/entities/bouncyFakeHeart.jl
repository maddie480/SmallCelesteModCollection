module LifeOnTheMoonBouncyFakeHeart

using ..Ahorn, Maple

@mapdef Entity "LifeOnTheMoon/BouncyFakeHeart" BouncyFakeHeart(x::Integer, y::Integer, color::String="Random", glitch::Bool=false)

const placements = Ahorn.PlacementDict(
    "Bouncy Fake Heart (Life On The Moon)" => Ahorn.EntityPlacement(
        BouncyFakeHeart
    ),
)

Ahorn.editingOptions(entity::BouncyFakeHeart) = Dict{String, Any}(
    "color" => Maple.everest_fake_heart_colors
)

const sprite = "collectables/heartGem/0/00.png"

function Ahorn.selection(entity::BouncyFakeHeart)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BouncyFakeHeart, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
