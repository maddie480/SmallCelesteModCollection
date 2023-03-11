module DonkerJelliesReverseJelly

using ..Ahorn, Maple

@mapdef Entity "DonkerJellies/ReverseJelly" ReverseJelly(x::Integer, y::Integer, bubble::Bool=false, tutorial::Bool=false, spriteDirectory::String="objects/glider", glow::Bool=false)

const placements = Ahorn.PlacementDict(
    "Reverse Jelly (Donker Jellies)" => Ahorn.EntityPlacement(
        ReverseJelly
    ),
    "Reverse Jelly (Floating) (Donker Jellies)" => Ahorn.EntityPlacement(
        ReverseJelly,
        "point",
        Dict{String, Any}(
            "bubble" => true
        )
    )
)

function Ahorn.selection(entity::ReverseJelly)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(get(entity, "spriteDirectory", "objects/glider") * "/idle0", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReverseJelly, room::Maple.Room)
    Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/glider") * "/idle0", 0, 0)

    if get(entity, "bubble", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end
