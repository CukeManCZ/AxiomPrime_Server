using AxiomPrime.Models.Items;
using AxiomPrime_DTOs.Missions;
using AxiomPrime_DTOs.Reward;

public static class MissionMapper
{
    public static MissionDto ToDto(Mission_Database mission)
    {
        ArgumentNullException.ThrowIfNull(mission);
        return new MissionDto
        {
            Identity = mission.Identity,
            GeneralData = mission.GeneralData,
            State = mission.State,
            Reward = ToDto(mission.Reward) 
        };
    }

    public static List<MissionDto> ToDto(List<Mission_Database> missions)
    {
        var missionsDto = new List<MissionDto>();
        foreach(var m in missions)
            missionsDto.Add(ToDto(m));

        return missionsDto;
    }

    public static RewardDto ToDto(MissionReward_Database reward)
    {
        ArgumentNullException.ThrowIfNull(reward);
        return new RewardDto
        {
            GeneralData = reward.GeneralData,
            Item = reward.Item != null ? InventoryMapper.ToDto(reward.Item) : null
        };
    }
}