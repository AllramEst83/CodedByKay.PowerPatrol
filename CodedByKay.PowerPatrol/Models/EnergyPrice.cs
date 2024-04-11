namespace CodedByKay.PowerPatrol.Models
{
    public class EnergyPrice
    {
        public DateTime Time { get; }
        public double Price { get; }
        public string Color { get; }

        public EnergyPrice(DateTime time, double price, string color)
        {
            Time = time;
            Price = price;
            Color = color;
        }
    }
}
