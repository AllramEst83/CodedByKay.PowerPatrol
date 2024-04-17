namespace CodedByKay.PowerPatrol.Models
{
    public class EnergyPrice
    {
        public DateTime Time { get; }
        public double Price { get; }

        public EnergyPrice(DateTime time, double price)
        {
            Time = time;
            Price = price;
        }
    }
}
