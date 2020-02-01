module SpringCollabLightningDashSwitch

using ..Ahorn, Maple

sides = String[
	"left",
	"right",
	"up",
	"down"
]

textures = String[
	"default",
	"mirror"
]

@mapdef Entity "SpringCollab2020/LightningDashSwitch" LDashSwitch(x::Integer, y::Integer, side::String="up", persistent::Bool=false, sprite::String="default")

const placements = Ahorn.PlacementDict(
	"Lightning Dash Switch ($(uppercasefirst(side))) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        LDashSwitch,
        "rectangle",
		Dict{String, Any}(
			"side" => side
		)
	) for side in sides
)

Ahorn.editingOptions(entity::LDashSwitch) = Dict{String, Any}(
	"side" => sides
)

function Ahorn.selection(entity::LDashSwitch)
    x, y = Ahorn.position(entity)
    side = get(entity.data, "side", false)

    if side == "left"
        return Ahorn.Rectangle(x, y - 1, 10, 16)
    elseif side == "right"
        return Ahorn.Rectangle(x - 2, y, 10, 16)
    elseif side == "down"
        return Ahorn.Rectangle(x, y, 16, 12)
	elseif side == "up"
        return Ahorn.Rectangle(x, y - 4, 16, 12)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LDashSwitch, room::Maple.Room)
    sprite = get(entity.data, "sprite", "default")
    side = get(entity.data, "side", "up")
    texture = sprite == "default" ? "objects/temple/dashButton00.png" : "objects/temple/dashButtonMirror00.png"

    if side == "left"
        Ahorn.drawSprite(ctx, texture, 20, 25, rot=pi)
    elseif side == "right"
        Ahorn.drawSprite(ctx, texture, 8, 8)
    elseif side == "up"
        Ahorn.drawSprite(ctx, texture, 9, 20, rot=-pi / 2)
    elseif side == "down"
        Ahorn.drawSprite(ctx, texture, 27, 7, rot=pi / 2)
    end
end

end