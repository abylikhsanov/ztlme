namespace ztlme.Methods;

using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;

public class S3Service
{
    private readonly AmazonS3Client _client;

    public S3Service()
    {
        _client = new AmazonS3Client();
    }

    public async Task<byte[]> DownloadFileAsync(string bucketName, string keyName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            using (var response = await _client.GetObjectAsync(request))
            using (var responseStream = response.ResponseStream)
            using (var memoryStream = new MemoryStream())
            {
                await responseStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray(); // Returns the downloaded object as a byte array
            }
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when writing an object");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unknown encountered on server. Message:'{e.Message}' when writing an object");
            throw;
        }
    }
}
