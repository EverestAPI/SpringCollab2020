module SpringCollab2020StrawberryIgnoringLighting

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Strawberry (Ignore Lighting) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        Maple.Strawberry,
        "point",
        Dict{String, Any}(
            "SpringCollab2020_ignoreLighting" => true
        )
    )
)

end