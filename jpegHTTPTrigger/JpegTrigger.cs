using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

namespace jpegHTTPTrigger
{
    public static class JpegTrigger
    {
        [FunctionName("JpegTrigger")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            name = name ?? data?.name;

            var newland = new {
                names = name
            };
         
          
            string storageConnectionString = Environment.GetEnvironmentVariable("cv");
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
             CloudBlobContainer   cloudBlobContainer = cloudBlobClient.GetContainerReference("fnct");
               await  cloudBlobContainer.CreateIfNotExistsAsync();

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference("sdf.jpeg");
                //cloudBlockBlob.UploadText(JsonConvert.SerializeObject(newland).ToString());
                cloudBlockBlob.UploadFromFile("C:\\Pics\\msdn1.jpg");
            }

            //trnct = JsonConvert.SerializeObject(newland);
            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(newland,Formatting.Indented));
        }
    }
}

