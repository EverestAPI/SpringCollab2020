module SpringCollab2020FlagToggleStarRotateSpinner

using ..Ahorn, Maple

@pardef FlagToggleStarRotateSpinner(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, clockwise::Bool=false, flag::String="flag_toggle_star_rotate_spinner", inverted::Bool=false) =
    Entity("SpringCollab2020/FlagToggleStarRotateSpinner", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], clockwise=clockwise, flag=flag, inverted=inverted)

function rotatingSpinnerFinalizer(entity::FlagToggleStarRotateSpinner)
    x, y = Int(entity.data["x"]), Int(entity.data["y"])
    entity.data["x"], entity.data["y"] = x + 32, y
    entity.data["nodes"] = [(x, y)]
end

const placements = Ahorn.PlacementDict(
    "Star (Rotating, Clockwise, Flag Toggle) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        FlagToggleStarRotateSpinner,
        "point",
        Dict{String, Any}(
            "clockwise" => true
        ),
        rotatingSpinnerFinalizer
    ),
    "Star (Rotating, Flag Toggle) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        FlagToggleStarRotateSpinner,
        "point",
        Dict{String, Any}(
            "clockwise" => false
        ),
        rotatingSpinnerFinalizer
    ),
)

Ahorn.nodeLimits(entity::FlagToggleStarRotateSpinner) = 1, 1

function Ahorn.selection(entity::FlagToggleStarRotateSpinner)
    nx, ny = Int.(entity.data["nodes"][1])
    x, y = Ahorn.position(entity)

    return [Ahorn.Rectangle(x - 8, y - 8, 16, 16), Ahorn.Rectangle(nx - 8, ny - 8, 16, 16)]
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FlagToggleStarRotateSpinner, room::Maple.Room)
    clockwise = get(entity.data, "clockwise", false)
    dir = clockwise ? 1 : -1

    centerX, centerY = Int.(entity.data["nodes"][1])
    x, y = Ahorn.position(entity)

    radius = sqrt((centerX - x)^2 + (centerY - y)^2)

    Ahorn.drawCircle(ctx, centerX, centerY, radius, Ahorn.colors.selection_selected_fc)
    Ahorn.drawArrow(ctx, centerX + radius, centerY, centerX + radius, centerY + 0.001 * dir, Ahorn.colors.selection_selected_fc, headLength=6)
    Ahorn.drawArrow(ctx, centerX - radius, centerY, centerX - radius, centerY + 0.001 * -dir, Ahorn.colors.selection_selected_fc, headLength=6)

    Ahorn.drawSprite(ctx, "danger/starfish13", x, y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FlagToggleStarRotateSpinner, room::Maple.Room)
    centerX, centerY = Int.(entity.data["nodes"][1])

    Ahorn.drawSprite(ctx, "danger/starfish13", centerX, centerY)
end

end