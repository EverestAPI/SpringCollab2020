module SpringCollab2020BubbleReturnBerry

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/returnBerry" ReturnBerry(x::Integer, y::Integer, order::Integer=-1, checkpointID::Integer=-1, winged::Bool=false)

const placements = Ahorn.PlacementDict(
	"Strawberry With Return (Spring Collab 2020)" => Ahorn.EntityPlacement(
		ReturnBerry
	)
)

function getSpriteName(entity::ReturnBerry)
	winged = get(entity.data, "winged", false)

	if winged
		return "collectables/strawberry/wings01"
	end

	return "collectables/strawberry/normal00"
end

function Ahorn.selection(entity::ReturnBerry)
	x, y = Ahorn.position(entity)

	return Ahorn.getSpriteRectangle(getSpriteName(entity), x, y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReturnBerry, room::Maple.Room)
	x, y = Ahorn.position(entity)

	Ahorn.drawSprite(ctx, getSpriteName(entity), x, y)
end

end
