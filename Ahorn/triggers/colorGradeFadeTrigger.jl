module SpringCollab2020ColorGradeFadeTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/ColorGradeFadeTrigger" ColorGradeFadeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    colorGradeA::String="none", colorGradeB::String="none", direction::String="LeftToRight")

const placements = Ahorn.PlacementDict(
    "Color Grade Fade (Spring Collab 2020)" => Ahorn.EntityPlacement(
        ColorGradeFadeTrigger,
        "rectangle",
    ),
)

const colorGrades = String["none", "oldsite", "panicattack", "templevoid", "reflection", "credits", "cold", "hot", "feelingdown", "golden"]

function Ahorn.editingOptions(trigger::ColorGradeFadeTrigger)
    return Dict{String, Any}(
        "direction" => String["LeftToRight", "TopToBottom"],
        "colorGradeA" => colorGrades,
        "colorGradeB" => colorGrades
    )
end

end