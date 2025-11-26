namespace SocketServer.Models.Wheel
{
    public class ObjectiveModifier
    {
        public string Slot { get; set; } = default!;

        public string Name { get; set; } = default!;

        public int Points { get; set; } = 0;

        public List<string>PlayerIds { get; set; } = new List<string>();
    }
}
