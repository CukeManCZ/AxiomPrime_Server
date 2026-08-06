using AxiomPrime_Metadata.Inventory;
using AxiomPrime_Metadata.Rewards;

public class MissionReward_Database
{
    public required RewardGeneralData GeneralData {get; set;}
    public Item_Database? Item {get; set;}
}