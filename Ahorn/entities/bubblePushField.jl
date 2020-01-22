module SpringCollab2020BubblePushField

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/bubblePushField" BubblePushField(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, strength::Number=1, upwardStrength::Number=1, direction::String="Right", water::Bool=true)

const placements = Ahorn.PlacementDict(
	"Bubble Column (Spring Collab 2020)" => Ahorn.EntityPlacement(
		BubblePushField,
		"rectangle"
	)
)

Ahorn.editingOptions(entity::BubblePushField) = Dict{String,Any}(
	"direction" => ["Up", "Down", "Left", "Right"]
)

Ahorn.minimumSize(entity::BubblePushField) = 8, 8
Ahorn.resizable(entity::BubblePushField) = true, true

Ahorn.selection(entity::BubblePushField) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BubblePushField, room::Maple.Room)
	x = Int(get(entity.data, "x", 0))
	y = Int(get(entity.data, "y", 0))

	width = Int(get(entity.data, "width", 32))
	height = Int(get(entity.data, "height", 32))

	Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.7, 0.28, 0.0, 0.34), (1.0, 1.0, 1.0, 0.5))
end

end
