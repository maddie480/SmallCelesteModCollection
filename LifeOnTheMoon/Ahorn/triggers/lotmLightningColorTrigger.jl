module LifeOnTheMoonLightningColorTrigger

using ..Ahorn, Maple

@mapdef Trigger "LifeOnTheMoon/LightningColorTrigger" LightningColorTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    lightningColorSmall::String="ffffff", lightningColorBig::String="dedede", lightningHasOutline::Bool=false)

const placements = Ahorn.PlacementDict(
    "Lightning Color (Life On The Moon)" => Ahorn.EntityPlacement(
        LightningColorTrigger,
        "rectangle"
    ),
)

end
