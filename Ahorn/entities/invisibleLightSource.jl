module SpringCollab2020InvisibleLightSource

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/invisibleLightSource" InvisibleLightSource(x::Integer, y::Integer, alpha::Number=1.0, radius::Number=48.0, startFade::Number=24.0, endFade::Number=48.0, color::String="White")

const colors = sort(collect(keys(Ahorn.XNAColors.colors)))

const placements = Ahorn.PlacementDict(
	"Light Source (Spring Collab 2020)" => Ahorn.EntityPlacement(
		InvisibleLightSource
	)
)

Ahorn.editingOptions(entity::InvisibleLightSource) = Dict{String,Any}(
	"color" => colors
)

function Ahorn.selection(entity::InvisibleLightSource)
	x, y = Ahorn.position(entity)

	return Ahorn.Rectangle(x - 4, y - 4, 7, 8)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::InvisibleLightSource, room::Maple.Room)
	x, y = Ahorn.position(entity)
	sprite = Ahorn.getTextureSprite("objects/hanginglamp", "Gameplay")
	
	Ahorn.drawImage(ctx, sprite, x - 4, y - 4, 0, 16, 7, 8, alpha=0.7)
end

end