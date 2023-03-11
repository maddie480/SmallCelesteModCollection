module IronSmelteryCampaignDoNotLoadTrigger

using ..Ahorn, Maple

@mapdef Trigger "IronSmelteryCampaign/DoNotLoadTrigger" DoNotLoadTrigger(x::Integer, y::Integer,
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, roomName::String="1", entityID::Integer=0)

const placements = Ahorn.PlacementDict(
    "Do Not Load Trigger (Iron Smeltery Campaign)" => Ahorn.EntityPlacement(
        DoNotLoadTrigger,
        "rectangle"
    )
)

end
