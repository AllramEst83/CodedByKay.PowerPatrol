namespace CodedByKay.PowerPatrol.Models
{
    using System;
    using System.Collections.Generic;

    public class CurrentEnergyPrice
    {
        public Address Address { get; set; }
        public CurrentSubscription CurrentSubscription { get; set; }
    }

    public class Address
    {
        public string Address1 { get; set; }
    }
    public class CurrentSubscription
    {
        public PriceInfo PriceInfo { get; set; }
    }

    public class PriceInfo
    {
        public Current Current { get; set; }
        public List<Price> Today { get; set; }
        public List<Price> Tomorrow { get; set; }
    }

    public class Current
    {
        public double Total { get; set; }
        public double Energy { get; set; }
        public double Tax { get; set; }
        public DateTime StartsAt { get; set; }
    }

    public class Price
    {
        public double Total { get; set; }
        public double Energy { get; set; }
        public double Tax { get; set; }
        public DateTime StartsAt { get; set; }
    }

    public class EnergyConsumptionResponse
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public Viewer Viewer { get; set; }
    }

    public class Viewer
    {
        public List<CurrentEnergyPrice> Homes { get; set; }
    }

}
