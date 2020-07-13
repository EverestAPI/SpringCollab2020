module SpringCollab2020BadelineBounceDirectionTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/BadelineBounceDirectionTrigger" BadelineBounceDirectionTrigger(x::Integer, y::Integer,
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, bounceLeft::Bool=false)

const placements = Ahorn.PlacementDict(
    "Badeline Bounce Direction (Spring Collab 2020)" => Ahorn.EntityPlacement(
        BadelineBounceDirectionTrigger,
        "rectangle",
    ),
)

end
