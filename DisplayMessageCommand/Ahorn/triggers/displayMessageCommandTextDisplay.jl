module DisplayMessageCommandTextDisplayTrigger

using ..Ahorn, Maple

@mapdef Trigger "DisplayMessageCommand/TextDisplayTrigger" TextDisplayTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    textID::String="0", message::String="hi", scale::Number=1.0, yPosition::Number=500.0, isLeft::Bool=false, duration::Number=0.0, onlyOnce::Bool=true)

const placements = Ahorn.PlacementDict(
    "Display Text (Display Message Command)" => Ahorn.EntityPlacement(
        TextDisplayTrigger,
        "rectangle"
    )
)

end
