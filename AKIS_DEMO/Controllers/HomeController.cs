using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        List<string> csvFiles = new List<string>();
        List<IndexRecord> allRecords = new List<IndexRecord>();

        public IActionResult Index()
        {
            var model = new IndexModel();

            return View(model);
        }

        [HttpPost]
        public IActionResult Search(IndexModel model)
        {
            return Json(new { header = "" });
        }

        private List<IndexRecord> GetIndexRecords(int id, string source, string type)
        {
            IndexSource indexSource = GetSource(source);
            IndexType indexType = GetType(type);

            if (indexSource == IndexSource.Satellite)
            {
                if (id > 3 && id < 13)
                    id = 3;
            }

            FillIndexRecords();

            var result = allRecords.FindAll(x => x.Source == indexSource && x.Type == indexType && x.Id == id);

            return result;
        }

        private IndexType GetType(string type)
        {
            IndexType indexType = IndexType.CLRE;

            if (type == "EVI")
                indexType = IndexType.EVI;

            if (type == "MRESR")
                indexType = IndexType.MRESR;

            if (type == "NDMI")
                indexType = IndexType.NDMI;

            if (type == "NDVI")
                indexType = IndexType.NDVI;

            if (type == "NDWI")
                indexType = IndexType.NDWI;

            if (type == "RTVI")
                indexType = IndexType.RTVI;

            if (type == "SAVI")
                indexType = IndexType.SAVI;

            if(type == "NDRE")
                indexType = IndexType.NDRE;

            if (type == "GNDVI")
                indexType = IndexType.GNDVI;

            if (type == "DSWI")
                indexType = IndexType.DSWI;

            if (type == "CVI")
                indexType = IndexType.CVI;

            if (type == "CIGREEN")
                indexType = IndexType.CIGREEN;
           
            if (type == "RECL")
                indexType = IndexType.RECL;

            return indexType;
        }

        private IndexSource GetSource(string source)
        {
            IndexSource indexSource = IndexSource.Satellite;
            if (source.ToUpper() == "UAV")
                indexSource = IndexSource.Uav;

            return indexSource;
        }

        [HttpGet]
        public IActionResult GetIndexData(string idStr, string source, string type)
        {
            int id = Convert.ToInt32(idStr);

            var indexRecords = GetIndexRecords(id, source, type);
            var result = new IndexModel();

            var dateResult = new List<string>();
            var dataResult = new List<double>();
            var irrResult = new List<double>();
            foreach (var indexRecord in indexRecords)
            {
                dateResult.Add(indexRecord.Date);
                dataResult.Add(indexRecord.Value);

                if (indexRecord.Date == "20.5.2022")
                    irrResult.Add(1);
                else if (indexRecord.Date == "15.6.2022")
                    irrResult.Add(1);
                else
                    irrResult.Add(0);
            }

            result.IndexData = dataResult;
            result.IndexDates = dateResult;
            result.IrrigationData = irrResult;

            return Json(result);
        }

        [HttpGet]
        public IActionResult GetIonIndex(string source, string type, string dataIndex)
        {
            var indexSource = GetSource(source);
            var indexType = GetType(type);

            return Json(CalculateIonIndex(indexSource, indexType, Convert.ToInt32(dataIndex)));
        }

        [HttpGet]
        public IActionResult GetMeteoData()
        {
            var response = GetMeteoResponse();
            return Json(response);
        }
       
       
        private string GetMeteoResponse() 
        {
            var username = "chirchiq";
            var password = "R?J=*R4y*KeQBy;C";
            var url = "https://meteoapi.meteo.uz/api/ministation/chirchiq";

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(username, password ),
            };

            var client = new HttpClient(httpClientHandler);
            HttpResponseMessage response = client.GetAsync(url).Result;
            return response.Content.ReadAsStringAsync().Result;
        }


        private void FillIndexRecords()
        {
            if (allRecords.Count == 0)
            {
                if (csvFiles.Count == 0)
                {
                    var directory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
                    var fileList = Directory.GetFiles(directory);

                    foreach (var file in fileList)
                        csvFiles.Add(file);
                }

                allRecords = new List<IndexRecord>();

                foreach (var csvFile in csvFiles)
                {
                    IndexSource source = IndexSource.Satellite;
                    IndexType type = IndexType.NDVI;

                    var fileName = Path.GetFileName(csvFile);

                    if (fileName.StartsWith("SAT_"))
                    {
                        if (fileName.Contains("_CLRE"))
                            type = IndexType.CLRE;

                        if (fileName.Contains("_EVI"))
                            type = IndexType.EVI;

                        if (fileName.Contains("_MRESR"))
                            type = IndexType.MRESR;

                        if (fileName.Contains("_NDMI"))
                            type = IndexType.NDMI;

                        if (fileName.Contains("_NDWI"))
                            type = IndexType.NDWI;

                        if (fileName.Contains("_RTVI"))
                            type = IndexType.RTVI;

                        if (fileName.Contains("_SAVI"))
                            type = IndexType.SAVI;
                    }

                    if (fileName.StartsWith("UAV_"))
                    {
                        source = IndexSource.Uav;

                        if (fileName.Contains("_NDRE"))
                            type = IndexType.NDRE;

                        if (fileName.Contains("_NDWI"))
                            type = IndexType.NDWI;

                        if (fileName.Contains("_GNDVI"))
                            type = IndexType.GNDVI;

                        if (fileName.Contains("_DSWI"))
                            type = IndexType.DSWI;

                        if (fileName.Contains("_CVI"))
                            type = IndexType.CVI;

                        if (fileName.Contains("_CIGREEN"))
                            type = IndexType.CIGREEN;

                        if (fileName.Contains("_RECL"))
                            type = IndexType.RECL;
                    }
                    
                    var lines = System.IO.File.ReadAllLines(csvFile);

                    var dates = IndexRecord.GetDates(lines[0]);

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var recordsInline = IndexRecord.FromCsv(source, type, dates, lines[i]);
                        allRecords.AddRange(recordsInline);
                    }
                }
            }
        }

        private int CalculateIonIndex(IndexSource source, IndexType type, int dataIndex)
        {
            if (source == IndexSource.Satellite)
            {
                if (type == IndexType.NDVI)
                    return satNDVI[dataIndex];

             

                if (type == IndexType.EVI)
                    return satEVI[dataIndex];


                if (type == IndexType.NDMI)
                    return satNDMI[dataIndex];

                if (type == IndexType.NDWI)
                    return satNDWI[dataIndex];


                if (type == IndexType.SAVI)
                    return satSAVI[dataIndex];
            }

            if (source == IndexSource.Uav)
            {
                if (type == IndexType.NDVI)
                    return uavNDVI[dataIndex];

                if (type == IndexType.NDRE)
                    return uavNDRE[dataIndex];

                if (type == IndexType.GNDVI)
                    return uavGNDVI[dataIndex];

                if (type == IndexType.DSWI)
                    return uavDSWI[dataIndex];

                if (type == IndexType.CVI)
                    return uavCVI[dataIndex];

                if (type == IndexType.CIGREEN)
                    return uavCIGREEN[dataIndex];

                if (type == IndexType.RECL)
                    return uavRECL[dataIndex];
            }
                return 0;
        }

        int[] satNDVI = new int[] { 1270276, 1270277, 1270280, 1270281, 1270282, 1270283, 1270285, 1270285, 1270286, 1270287, 1270288, 1270289, 1270290, 1270291, 1270292, 1270292, 1270293, 1270294, 1270295, 1270296, 1270297 };
        int[] satSAVI = new int[] { 1270326, 1270327, 1270328, 1270329, 1270330, 1270331, 1270332, 1270332, 1270333, 1270335, 1270336, 1270337, 1270339, 1270340, 1270341, 1270341, 1270342, 1270343, 1270344, 1270345, 1270346 };
        int[] satEVI = new int[]  { 1270401, 1270403, 1270404, 1270405, 1270406, 1270407, 1270408, 1270408, 1270409, 1270411, 1270412, 1270413, 1270414, 1270415, 1270416, 1270416, 1270417, 1270418, 1270419, 1270420, 1270421 };
 
        int[] satNDMI = new int[] { 1201268, 1201269, 1201270, 1201271, 1201272, 1201273, 1201273, 1201274, 1201275, 1201276, 1201277, 1201278, 1201279, 1201281, 1201282, 1201283, 1201285, 1201286, 1201287 };
        int[] satNDWI = new int[] { 1201321, 1201318, 1201317, 1201315, 1201313, 1201312,  1201311, 1201311,1201310, 1201309, 1201308, 1201307, 1201305, 1201304, 1201303, 1201303, 1201301, 1201300, 1201299, 1201298, 1201296 };        
    
        int[] uavNDVI = new int[] { 1206699 };
        int[] uavNDRE = new int[] { 1206697 };
        int[] uavGNDVI = new int[] { 1206696 };
        int[] uavDSWI = new int[] { 1206692 };
        int[] uavCVI = new int[] { 1206688 };
        int[] uavCIGREEN = new int[] { 1206684 };
        int[] uavRECL = new int[] { 12067069 };
    }
}
