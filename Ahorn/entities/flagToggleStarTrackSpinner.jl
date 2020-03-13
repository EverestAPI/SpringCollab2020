module SpringCollab2020FlagToggleStarTrackSpinner

using ..Ahorn, Maple

@pardef FlagToggleStarTrackSpinner(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, speed::String="Normal", startCenter::Bool=false, flag::String="flag_toggle_star_track_spinner") =
    Entity("SpringCollab2020/FlagToggleStarTrackSpinner", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], speed=speed, startCenter=startCenter, flag=flag)

const placements = Ahorn.PlacementDict(
    "Star (Track, $(uppercasefirst(speed)), Flag Toggle) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        FlagToggleStarTrackSpinner,
        "line",
        Dict{String, Any}(
            "speed" => speed
        )
    ) for speed in Maple.track_spinner_speeds
)

Ahorn.editingOptions(entity::FlagToggleStarTrackSpinner) = Dict{String, Any}(
    "speed" => Maple.track_spinner_speeds
)

Ahorn.nodeLimits(entity::FlagToggleStarTrackSpinner) = 1, 1

function Ahorn.selection(entity::FlagToggleStarTrackSpinner)
    startX, startY = Ahorn.position(entity)
    stopX, stopY = Int.(entity.data["nodes"][1])

    return [Ahorn.Rectangle(startX - 8, startY - 8, 16, 16), Ahorn.Rectangle(stopX - 8, stopY - 8, 16, 16)]
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FlagToggleStarTrackSpinner, room::Maple.Room)
    startX, startY = Ahorn.position(entity)
    stopX, stopY = entity.data["nodes"][1]

    Ahorn.drawSprite(ctx, "danger/starfish13", stopX, stopY)
    Ahorn.drawArrow(ctx, startX, startY, stopX, stopY, Ahorn.colors.selection_selected_fc, headLength=10)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FlagToggleStarTrackSpinner, room::Maple.Room)
    startX, startY = Ahorn.position(entity)

    Ahorn.drawSprite(ctx, "danger/starfish13", startX, startY)
end

end