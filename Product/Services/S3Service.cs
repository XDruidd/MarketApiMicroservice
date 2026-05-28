using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Product.Services.Inteface;
using SixLabors.ImageSharp;

namespace Product.Services;

public class S3Service : IS3Service
{ 
    private  readonly  string _bucketName; 
    private  readonly IAmazonS3 _s3Client; 
    private readonly string _baseUrl;

    public  S3Service()
    { 
        _s3Client = new AmazonS3Client(RegionEndpoint.EUNorth1);
        _bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_PRODUCT") ?? throw new ArgumentNullException("Location in aws not faund");
        _baseUrl = $"https://{_bucketName}.s3.{RegionEndpoint.EUNorth1.SystemName}.amazonaws.com/";
    } 

    public  async Task<string> UploadFileAsync(IFormFile file) {
        var prefix = "images/" ; 
        var key = prefix + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); 

        await using var originalStream = file.OpenReadStream(); 

        await using var cleanStream = RemoveMetadata(originalStream);

        var putRequest = new PutObjectRequest 
        { 
            BucketName = _bucketName, 
            Key = key, 
            InputStream = cleanStream, 
            ContentType = file.ContentType 
        }; 

        var response = await _s3Client.PutObjectAsync(putRequest); 

        if (response.HttpStatusCode == HttpStatusCode.OK) 
        { 
            return $"{_baseUrl}{key}";
        } 

        throw  new Exception( "Error uploading file." ); 
    } 
    
    public async Task UpdateFileAsync(string url, IFormFile file )
    {
        var key = url.Replace(_baseUrl, "");
        
        await using var originalStream = file.OpenReadStream(); 

        await using var cleanStream = RemoveMetadata(originalStream);
        
        var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = cleanStream,
                ContentType = file.ContentType
            };

        var response = await _s3Client.PutObjectAsync(putRequest);

        if (response.HttpStatusCode is not (HttpStatusCode.OK or HttpStatusCode.NoContent))
        {
            throw new Exception("Error updating file in AWS S3.");
        }

    }
    
    public async Task<bool> DeleteFileAsync(string key)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        try
        {
            var response = await _s3Client.DeleteObjectAsync(deleteRequest);
            return response.HttpStatusCode == HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception( $"Error in proces delete img: {ex.Message} " );
        }
    }
    
    private Stream RemoveMetadata(Stream inputStream)
    {
        inputStream.Position = 0;

        using var image = Image.Load(inputStream);

        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;

        var outputStream = new MemoryStream();

        image.Save(outputStream, image.Metadata.DecodedImageFormat ?? SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance);
    
        outputStream.Position = 0;
    
        return outputStream;
    }
}