using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.ExchangesModel
{
    public class TickerModel
    {
        public string Symbol { get; set; }
        /// <summary>
        /// The last trade purchase price
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// The bid is the price to sell at
        /// </summary>
        public decimal Bid { get; set; }
        /// <summary>
        /// The ask is the price to buy at
        /// </summary>
        public decimal Ask { get; set; }

        public VolumeModel Volume { get; set; }
    }

    public class VolumeModel
    {
        /// <summary>
        /// Last volume update timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Price symbol - will equal quantity symbol if exchange doesn't break it out by price unit and quantity unit
        /// </summary>
        public string PriceSymbol { get; set; }

        /// <summary>
        /// Price amount - will equal QuantityAmount if exchange doesn't break it out by price unit and quantity unit
        /// </summary>
        public decimal PriceAmount { get; set; }

        /// <summary>
        /// Quantity symbol (converted into this unit)
        /// </summary>
        public string QuantitySymbol { get; set; }

        /// <summary>
        /// Quantity amount (this many units total)
        /// </summary>
        public decimal QuantityAmount { get; set; }
    }
}
