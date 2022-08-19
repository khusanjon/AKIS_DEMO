using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class IndexModel
    {
        
        public List<string> IndexDates { get; set; }

        public List<double> IndexData { get; set; }

        public List<double> IrrigationData { get; set; }

        public IndexModel()
        {
            IndexDates = new List<string>();
            IndexData = new List<double>();
            IrrigationData = new List<double>();
        }
    }
    
    public class IndexRecord
    {
        static CultureInfo usCult = new CultureInfo("en-US");

        public int Id { get; set; }
        public IndexSource Source { get; set; }
        public string Date { get; set; }
        public IndexType Type { get; set; }
        public double Value { get; set; }

        public static List<IndexRecord> FromCsv(IndexSource source, IndexType type, List<string> dates, string csvLine)
        {
            string[] values = csvLine.Split(';');

            var result = new List<IndexRecord>();
            int id = Convert.ToInt32(values[0]);

            for (int i = 1; i < values.Length; i++)
            {
                var item = new IndexRecord();
                item.Id = id;
                item.Source = source;
                item.Type = type;
                item.Date = dates[i-1];
                item.Value = Convert.ToDouble(values[i], usCult);
                result.Add(item);
            }
            return result;
        }

        public static List<string> GetDates(string firstCsvLine)
        {
            string[] values = firstCsvLine.Split(';');
            var result = new List<string>();

            for (int i = 1; i < values.Length; i++)
            {
                result.Add(values[i]);
            }

            return result;
        }
    }

    public class IonAsset
    {
        public IndexSource Source { get; set; }
        public string Date { get; set; }
        public IndexType Type { get; set; }
        public string AssetId { get; set; }
    }

    public enum IndexSource
    {
        Satellite,
        Uav
    }

    public enum IndexType
    {
        CLRE,
        EVI,
        MRESR,
        NDMI,
        NDVI,
        NDWI,
        RTVI,
        SAVI,
        NDRE,
        GNDVI,
        DSWI,
        CVI,
        CIGREEN,
        RECL
    }

}
