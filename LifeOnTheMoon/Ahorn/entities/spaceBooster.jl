module LifeOnTheMoonSpaceBooster

using ..Ahorn, Maple

@mapdef Entity "LifeOnTheMoon/SpaceBooster" SpaceBooster(x::Integer, y::Integer, stayTime::Number=0.0, canDashOut::Bool=true, pathColor::String="0000FF",
    continuesWithoutPlayer::Bool=false, respawnTime::Number=1.0, controlCamera::Bool=true, speed::Number=240.0, teleportAtNodes::String="", muffle::Number=0.75)

const placements = Ahorn.PlacementDict(
    "Space Booster (Life On The Moon)" => Ahorn.EntityPlacement(
        SpaceBooster,
        "point",
        Dict{String, Any}(),
        function(entity::SpaceBooster)
            entity.data["nodes"] = [
                (Int(entity.data["x"]) + 32, Int(entity.data["y"])),
                (Int(entity.data["x"]) + 64, Int(entity.data["y"]))
            ]
        end
    ),
)

Ahorn.nodeLimits(entity::SpaceBooster) = (1, -1)

sprite = "objects/muntheory/lifeonthemoon/spacebooster/00.png"
spriteStaying = "objects/muntheory/lifeonthemoon/spacebooster/dash00.png"
spriteContinuous = "objects/muntheory/lifeonthemoon/spacebooster/continuous00.png"

function Ahorn.selection(entity::SpaceBooster)
    x, y = Ahorn.position(entity)

    rectangles = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

    nodes = get(entity.data, "nodes", ())
    for node in nodes
        nodeX, nodeY = Int.(node)
        push!(rectangles, Ahorn.getSpriteRectangle(sprite, nodeX, nodeY))
    end

    return rectangles
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::SpaceBooster)
    px, py = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", Tuple{Int, Int}[])
    stayTime = get(entity.data, "stayTime", 0.0)
    continuesWithoutPlayer = get(entity.data, "continuesWithoutPlayer", false)

    thisSprite = continuesWithoutPlayer ? spriteContinuous : (stayTime > 0 ? spriteStaying : sprite)

    for node in nodes
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, thisSprite, nx, ny)
        px, py = nx, ny
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpaceBooster, room::Maple.Room)
    stayTime = get(entity.data, "stayTime", 0.0)
    continuesWithoutPlayer = get(entity.data, "continuesWithoutPlayer", false)

    thisSprite = continuesWithoutPlayer ? spriteContinuous : (stayTime > 0 ? spriteStaying : sprite)

    Ahorn.drawSprite(ctx, thisSprite, 0, 0)
end

end