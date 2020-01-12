module SpringCollab2020InvisibleLightSource
using ..Ahorn, Maple
@mapdef Entity "SpringCollab2020/invisibleLightSource" InvisibleLightSource(x::Integer, y::Integer, alpha::Number=1, radius::Number=48, startFade::Number=24, endFade::Number=48, color::String="White")

const placements = Ahorn.PlacementDict(
	"Light Source (Spring Collab 2020)" => Ahorn.EntityPlacement(
		InvisibleLightSource
	)
)

function Ahorn.selection(entity::InvisibleLightSource)
	x, y = Ahorn.position(entity)

	return Ahorn.Rectangle(x, y, 9, 12)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::InvisibleLightSource, room::Maple.Room)
	x, y = Ahorn.position(entity)
	
	Ahorn.drawImage(ctx, "objects/hanginglamp", x + 1, y - 14)
end

end