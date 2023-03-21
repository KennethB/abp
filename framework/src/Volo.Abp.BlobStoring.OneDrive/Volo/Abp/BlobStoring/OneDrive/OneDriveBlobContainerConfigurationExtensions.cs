using System;

namespace Volo.Abp.BlobStoring.OneDrive;

public static class OneDriveBlobContainerConfigurationExtensions
{
    public static OneDriveGraphClientConfiguration GetOneDriveConfiguration(
        this BlobContainerConfiguration containerConfiguration)
    {
        return new OneDriveGraphClientConfiguration(containerConfiguration);
    }

    public static BlobContainerConfiguration UseOneDrive(
        this BlobContainerConfiguration containerConfiguration,
        Action<OneDriveGraphClientConfiguration> OneDriveConfigureAction)
    {
        containerConfiguration.ProviderType = typeof(OneDriveBlobProvider);
        containerConfiguration.NamingNormalizers.TryAdd<OneDriveBlobNamingNormalizer>();

        OneDriveConfigureAction(new OneDriveGraphClientConfiguration(containerConfiguration));

        return containerConfiguration;
    }
}