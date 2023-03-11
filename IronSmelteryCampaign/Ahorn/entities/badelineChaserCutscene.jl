module IronSmelteryCampaignBadelineChaserCutscene

using ..Ahorn, Maple

@mapdef Entity "IronSmelteryCampaign/BadelineOldsiteCutscene" BadelineChaserCutscene(x::Integer, y::Integer, canChangeMusic::Bool=true)

const placements = Ahorn.PlacementDict(
    "Badeline Chaser with Cutscene (IronSmelteryCampaign)" => Ahorn.EntityPlacement(
        BadelineChaserCutscene
    )
)

# This sprite fits best, not perfect, thats why we have a offset here
chaserSprite = "characters/badeline/sleep00.png"

function Ahorn.selection(entity::BadelineChaserCutscene)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(chaserSprite, x + 4, y, jx=0.5, jy=1.0)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BadelineChaserCutscene) = Ahorn.drawSprite(ctx, chaserSprite, 4, 0, jx=0.5, jy=1.0)

end
