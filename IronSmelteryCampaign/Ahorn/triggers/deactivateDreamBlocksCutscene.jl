module IronSmelteryCampaignDeactivateDreamBlocksCutsceneThing

using ..Ahorn, Maple

@mapdef Trigger "IronSmelteryCampaign/DeactivateDreamBlocksCutsceneThing" DeactivateDreamBlocksCutsceneThing(x::Integer, y::Integer,
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, speed::Number=30.0)

const placements = Ahorn.PlacementDict(
    "Deactivate Dream Blocks Cutscene (Iron Smeltery Campaign)" => Ahorn.EntityPlacement(
        DeactivateDreamBlocksCutsceneThing,
        "rectangle",
        Dict{String, Any}(),
        function(trigger)
            trigger.data["nodes"] = [
                (Int(trigger.data["x"]) + Int(trigger.data["width"]) + 8, Int(trigger.data["y"]))
            ]
        end
    ),
)

Ahorn.nodeLimits(trigger::DeactivateDreamBlocksCutsceneThing) = 1, 1

end
