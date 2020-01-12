module SpringCollab2020InvisibleLightSource
using ..Ahorn, Maple
@mapdef Entity "SpringCollab2020/invisibleLightSource" InvisibleLightSource(x::Integer, y::Integer, alpha::Number=1, radius::Number=48, color::String="White")

sprite = Ahorn.getSprite("scenery/lamp", "Gameplay")

const placements = Ahorn.PlacementDict(
	"Light Source (Spring Collab 2020)" => Ahorn.EntityPlacement(
		InvisibleLightSource
	)
)

Ahorn.selection(entity::InvisibleLightSource)
	x, y = Ahorn.position(entity)

	return Ahorn.Rectangle(x - 4, y - 6, 8, 12)
end

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::InvisibleLightSource, room::Maple.Room)
	x, y = Ahorn.position(entity)

	Ahorn.drawSprite(ctx, sprite, x, y)
end
