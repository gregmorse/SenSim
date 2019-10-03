using SemSimWebService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using CsvHelper;
using SemSimWebService.Helpers;
using System.Web.Http.Cors;

namespace SemSimWebService.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UploadFilesController : ApiController
    {
        [HttpPost]
        [Route("api/upload")]
        public IEnumerable<PGPISingleResponse> UploadPGPI()
        {
            List<PGPISingleResponse> ret = new List<PGPISingleResponse>();
            string sPath = HostingEnvironment.MapPath("~");

            for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
            {
                HttpPostedFile postedFile = HttpContext.Current.Request.Files[i] as HttpPostedFile;

                if (postedFile.ContentLength > 0)
                {
                    var savedFilePath = sPath + Path.GetFileName(postedFile.FileName);

                    if (!File.Exists(savedFilePath))
                    {
                        // Save the file before using it.
                        postedFile.SaveAs(savedFilePath);
                    }

                    // Read the csv file, and get the response for each input.
                    var descriptions = new List<DescriptionInput>();

                    using (var reader = new StreamReader(savedFilePath))
                    using (var csvReader = new CsvReader(reader))
                    {
                        descriptions = csvReader.GetRecords<DescriptionInput>().ToList();
                    }

                    foreach (var desc in descriptions)
                    {
                        var pgpiResponse = SenSimHelper.GetPGPIResponses(desc.Description).ToList();

                        if (pgpiResponse.Count > 0)
                        {
                            ret.Add(new PGPISingleResponse
                            {
                                Input = desc.Description,
                                Responses = pgpiResponse
                            });
                        }
                    }
                }
            }

            return ret;
        }
    }
}
