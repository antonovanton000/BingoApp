using RandomizerCommon;

namespace SocketServer.Models.Huntpoint.NightreignSeedGenerator
{
    public static class TargetBosses
    {
        public static readonly IReadOnlyCollection<NightreignData.TargetBoss> All = (IReadOnlyCollection<NightreignData.TargetBoss>)Enumerable.ToList<NightreignData.TargetBoss>((IEnumerable<NightreignData.TargetBoss>)Enum.GetValues<NightreignData.TargetBoss>()).AsReadOnly();

        public static int GetID(this NightreignData.TargetBoss boss) => (int)boss;

        public static string GetName(this NightreignData.TargetBoss boss)
        {
            return NightreignData.TargetBossNames[boss];
        }

        public static NightreignData.TargetBoss From(int id) => (NightreignData.TargetBoss)id;
    }
}
