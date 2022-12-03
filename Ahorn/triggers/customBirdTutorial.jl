module SpringCollab2020CustomBirdTutorialTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/CustomBirdTutorialTrigger" CustomBirdTutorialTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    birdId::String="birdId", showTutorial::Bool=true)

const placements = Ahorn.PlacementDict(
    "Custom Bird Tutorial (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CustomBirdTutorialTrigger,
        "rectangle"
    )
)

end
