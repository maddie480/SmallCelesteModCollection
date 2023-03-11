module IronSmelteryCampaignFlagChangeInventoryTrigger

using ..Ahorn, Maple

@mapdef Trigger "IronSmelteryCampaign/FlagInventoryTrigger" FlagChangeInventoryTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, inventory::String="Default", flag::String="flagname")

const placements = Ahorn.PlacementDict(
    "Change Inventory (Flag-Toggled) (IronSmelteryCampaign)" => Ahorn.EntityPlacement(
        FlagChangeInventoryTrigger,
        "rectangle"
    )
)

function Ahorn.editingOptions(trigger::FlagChangeInventoryTrigger)
    return Dict{String, Any}(
        "inventory" => sort(Maple.inventories)
    )
end

end
