using System;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Azure.Identity;
using Microsoft.Graph.Models.ExternalConnectors;

namespace Volo.Abp.BlobStoring.OneDrive;


/// <summary>
/// This module will not try to connect to OneDrive.
/// </summary>
[DependsOn(
    typeof(AbpBlobStoringOneDriveModule),
    typeof(AbpBlobStoringTestModule)
)]
public class AbpBlobStoringOneDriveTestCommonModule : AbpModule
{

}

[DependsOn(
    typeof(AbpBlobStoringOneDriveTestCommonModule)
)]
public class AbpBlobStoringOneDriveTestModule : AbpModule
{
    public const string _clientId = "OneDrive.ClientId";
    public const string _tenantId = "OneDrive.TenantId";
    public const string _secret = "OneDrive.Secret";
    public const string _siteId = "OneDrive.SiteId";
    public const string _driveId = "OneDrive.DriveId";
    public const string _rootPath = "OneDrive.RootPath";
    public const string _rootPathId = "OneDrive.RootPathId";
    public const string _standardMode = "OneDrive.StandardMode";
    public const string _createContainerIfNotExists = "OneDrive.CreateContainerIfNotExists";
    public const string _containerName = "OneDrive.ContainerName";

    private readonly string _randomContainerName = "abp-onedrive-test-container-" + Guid.NewGuid().ToString("N");

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.ReplaceConfiguration(ConfigurationHelper.BuildConfiguration(builderAction: builder =>
        {
            builder.AddUserSecrets(_secret);
        }));

        //var configuration = context.Services.GetConfiguration();
        //_clientId = configuration["OneDrive:ClientId"];

        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureAll((containerName, containerConfiguration) =>
            {
                containerConfiguration.UseOneDrive(oneDrive =>
                {
                    oneDrive.TenantId = _tenantId;
                    oneDrive.ClientId = _clientId;
                    oneDrive.Secret = _secret;
                    oneDrive.SiteId = _siteId;
                    oneDrive.DriveId = _driveId;
                    oneDrive.RootPath = _rootPath;
                    oneDrive.RootPathId = _rootPathId;
                    oneDrive.ContainerName = _randomContainerName;
                    oneDrive.CreateContainerIfNotExists = true;
                });
            });
        });
    }

    public override async void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };
        var options = new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
        var clientSecretCredential = new ClientSecretCredential(_tenantId, _clientId, _secret, options);

        var graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);
        await graphServiceClient.Drives[_driveId].Items[_rootPathId].ItemWithPath(_randomContainerName).DeleteAsync();
    }
}