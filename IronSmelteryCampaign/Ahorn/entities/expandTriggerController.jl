module IronSmelteryCampaignExpandTriggerController

using ..Ahorn, Maple

@mapdef Entity "IronSmelteryCampaign/ExpandTriggerController" ExpandTriggerController(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Expand Trigger Controller (IronSmeltery's Campaign)" => Ahorn.EntityPlacement(
        ExpandTriggerController
    )
)

function Ahorn.selection(entity::ExpandTriggerController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

function Ahorn.editingOptions(effect::ExpandTriggerController)
    return Dict{String, Any}(
        "func" => String["AND", "OR", "XOR"]
    )
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ExpandTriggerController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/IronSmelteryCampaign/expand_trigger_controller", 0, 0)

end
