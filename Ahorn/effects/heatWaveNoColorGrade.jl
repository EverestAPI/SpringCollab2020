module SpringCollab2020HeatWaveNoColorGrade

using ..Ahorn, Maple

@mapdef Effect "SpringCollab2020/HeatWaveNoColorGrade" HeatWaveNoColorGrade(only::String="*", exclude::String="")

placements = HeatWaveNoColorGrade

function Ahorn.canFgBg(effect::HeatWaveNoColorGrade)
    return true, true
end

end
