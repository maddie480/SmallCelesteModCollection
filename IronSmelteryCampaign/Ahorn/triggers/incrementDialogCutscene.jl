module IronSmelteryCampaignIncrementDialogCutsceneTrigger

using ..Ahorn, Maple

@mapdef Trigger "IronSmelteryCampaign/IncrementDialogCutsceneTrigger" IncrementDialogCutsceneTrigger(x::Integer, y::Integer,
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, dialogIdPrefix::String="IronSmelteryCampaign_IncrementDialogCutscene", miniTextbox::Bool=false)

const placements = Ahorn.PlacementDict(
    "Increment Dialog Cutscene (Iron Smeltery Campaign)" => Ahorn.EntityPlacement(
        IncrementDialogCutsceneTrigger,
        "rectangle"
    )
)

end
