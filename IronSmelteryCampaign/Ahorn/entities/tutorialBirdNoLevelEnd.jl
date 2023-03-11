module IronSmelteryCampaignTutorialBirdNoLevelEnd

using ..Ahorn, Maple

@mapdef Entity "IronSmelteryCampaign/TutorialBirdNoLevelEnd" TutorialBirdNoLevelEnd(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Bird NPC (Dash Tutorial Not Ending Level) (IronSmelteryCampaign)" => Ahorn.EntityPlacement(
        TutorialBirdNoLevelEnd
    )
)

sprite = "characters/bird/crow00"

function Ahorn.selection(entity::TutorialBirdNoLevelEnd)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)
    scaleX = -1

    return Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y, sx=scaleX, jx=0.5, jy=1.0)]
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TutorialBirdNoLevelEnd, room::Maple.Room)
    key = lowercase(get(entity.data, "mode", "Sleeping"))
    scaleX = -1

    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=scaleX, jx=0.5, jy=1.0)
end

end
