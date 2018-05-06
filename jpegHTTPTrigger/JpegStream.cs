using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace jpegHTTPTrigger
{
    public static class JpegStream
    {
        [FunctionName("JpegStream")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string name;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            name =  data?.path;

            Image img = Image.FromFile(name);
            byte[] arr;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                arr = ms.GetBuffer();

                string storageConnectionString = System.Environment.GetEnvironmentVariable("pass");
                CloudStorageAccount storageAccount;
                if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
                    CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("fnct");
                    await cloudBlobContainer.CreateIfNotExistsAsync();

                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference("sdf.jpeg");
                    cloudBlockBlob.Properties.ContentType = "image\\jpeg";
                    //cloudBlockBlob.UploadText(JsnConvert.SerializeObject(newland).ToString());
                    //ms.Seek(0, SeekOrigin.Begin);
                    //cloudBlockBlob.UploadFromStream(ms);
                    try
                    {
                       // cloudBlockBlob.UploadFromByteArray(arr, 0, arr.Length, accessCondition: AccessCondition.GenerateIfNoneMatchCondition("*"));
                        cloudBlockBlob.UploadFromFile(name);
                       
                    }
                    catch (StorageException ex)
                    {
                        if (ex.RequestInformation.HttpStatusCode == (int)System.Net.HttpStatusCode.Conflict)
                            return req.CreateResponse(HttpStatusCode.BadRequest, "file might be already present");


                    }
                }
            }
            

            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a Path  in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Uploaded to Storage Blob" );
        }
    }
}
