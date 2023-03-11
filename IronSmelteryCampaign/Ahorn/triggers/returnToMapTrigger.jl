module IronSmelteryCampaignReturnToMapTrigger

using ..Ahorn, Maple

@mapdef Trigger "IronSmelteryCampaign/ReturnToMapTrigger" ReturnToMapTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Return To Map Trigger (Iron Smeltery Campaign)" => Ahorn.EntityPlacement(
        ReturnToMapTrigger,
        "rectangle"
    )
)

end
