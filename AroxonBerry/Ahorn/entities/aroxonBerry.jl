module AroxonBerryAroxonBerry

using ..Ahorn, Maple

@mapdef Entity "AroxonBerry/AroxonBerry" AroxonBerry(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Golden Strawberry (Winged, No Midair Jump) (Aroxon Berry)" => Ahorn.EntityPlacement(
        AroxonBerry,
        "point"
    )
)

const sprite = "collectables/goldberry/wings01"
const seedSprite = "collectables/goldberry/seed00"

Ahorn.nodeLimits(entity::AroxonBerry) = 0, -1

function Ahorn.selection(entity::AroxonBerry)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", ())

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

    for node in nodes
        nx, ny = node

        push!(res, Ahorn.getSpriteRectangle(seedSprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::AroxonBerry)
    x, y = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = node

        Ahorn.drawLines(ctx, Tuple{Number, Number}[(x, y), (nx, ny)], Ahorn.colors.selection_selected_fc)
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::AroxonBerry, room::Maple.Room)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", ())

    for node in nodes
        nx, ny = node

        Ahorn.drawSprite(ctx, seedSprite, nx, ny)
    end

    Ahorn.drawSprite(ctx, sprite, x, y)
end

end
