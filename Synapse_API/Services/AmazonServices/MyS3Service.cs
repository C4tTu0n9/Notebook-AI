﻿using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using Synapse_API.Models.Config;

namespace Synapse_API.Services.AmazonServices
{
    public class MyS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly AwsOptions _awsOptions;
        public MyS3Service(IAmazonS3 s3Client, IOptions<AwsOptions> awsOptions)
        {
            _s3Client = s3Client;
            _awsOptions = awsOptions.Value;
        }

        public async Task CreateNewBucketAsync()
        {
            try
            {
                // to check if there is another bucket with the same name
                // make sure to use V2 of the function because V1 is obsolete
                if (!(await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _awsOptions.Bucket)))                
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = _awsOptions.Bucket,
                        UseClientRegion = true
                    };                    
                    PutBucketResponse putBucketResponse = await _s3Client.PutBucketAsync(putBucketRequest);
                    Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>ok rooi");
                }
                else
                {
                    Console.WriteLine("Bucket already exists");
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        public async Task<string> UploadObjectAsync(IFormFile file, string keyName)
        {
            // "keyName" là tên file sẽ lưu trên S3
            using var stream = file.OpenReadStream();
            string test = _awsOptions.Region + " " + _awsOptions.Bucket;
            var putRequest = new PutObjectRequest
            {
                BucketName = _awsOptions.Bucket,
                Key = keyName,
                InputStream = stream,
                ContentType = file.ContentType,
            };
            await _s3Client.PutObjectAsync(putRequest);
            return $"https://{_awsOptions.Bucket}.s3.{_awsOptions.Region}.amazonaws.com/{keyName}";
        }
        public async Task<GetObjectResponse> GetObjectAsync(string keyName)
        {
            return await _s3Client.GetObjectAsync(_awsOptions.Bucket, keyName);
        }
        public async Task DeleteObjectAsync(string keyName)
        {
            await _s3Client.DeleteObjectAsync(_awsOptions.Bucket, keyName);
        }
        public async Task<List<string>> ListObjectsAsync(string prefix = "")
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _awsOptions.Bucket,
                Prefix = prefix ?? string.Empty // lấy file trong “folder” ảo nếu có
            };
            var response = await _s3Client.ListObjectsV2Async(request);
            return response.S3Objects.Select(o => o.Key).ToList();
        }

    }
}
