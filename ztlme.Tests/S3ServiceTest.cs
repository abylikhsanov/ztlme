namespace ztlme.Tests.Methods;
using ztlme.Methods;
using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Xunit;

public class S3ServiceTest
{
    [Fact]
    public async Task DownloadFileAsync_ReturnsCorrectContent()
    {
        var mockS3Client = new Mock<AmazonS3Client>(MockBehavior.Strict);
        var testBucketName = "ztl.me";
        var testKeyName = "Document.pdf";
        var testData = new byte[] { 1, 2, 3, 4, 5 };
        var testStream = new MemoryStream(testData);
        
        mockS3Client.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
            .ReturnsAsync(new GetObjectResponse()
            {
                ResponseStream = testStream
            });

        var s3Service = new S3Service();

        // Act
        var result = await s3Service.DownloadFileAsync(testBucketName, testKeyName);

        // Assert
        Assert.NotNull(result);

        // Verify that GetObjectAsync was called with correct parameters
        mockS3Client.Verify(x => x.GetObjectAsync(It.Is<GetObjectRequest>(
                req => req.BucketName == testBucketName && req.Key == testKeyName),
            default));
    }
}