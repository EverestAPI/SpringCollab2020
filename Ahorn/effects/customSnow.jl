module SpringCollab2020CustomSnow

using ..Ahorn, Maple

@mapdef Effect "SpringCollab2020/CustomSnow" CustomSnow(only::String="*", exclude::String="", color::String="FFFFFF,FFFFFF")

placements = CustomSnow

function Ahorn.canFgBg(effect::CustomSnow)
    return true, true
end

end
