namespace Volo.Abp.BlobStoring.OneDrive;

public class OneDriveGraphClientConfiguration
{
    public string TenantId
    {
        get => _containerConfiguration.GetConfiguration<string>(OneDriveGraphServiceConfigurationNames.TenantId);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.TenantId, Check.NotNullOrWhiteSpace(value, nameof(value)));
    }

    /// <summary>
    /// This name may only contain lowercase letters, numbers, and hyphens, and must begin with a letter or a number.
    /// Each hyphen must be preceded and followed by a non-hyphen character.
    /// The name must also be between 3 and 63 characters long.
    /// If this parameter is not specified, the ContainerName of the <see cref="BlobProviderArgs"/> will be used.
    /// </summary>
    public string Secret
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(OneDriveGraphServiceConfigurationNames.Secret);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.Secret, Check.NotNullOrWhiteSpace(value, nameof(value)));
    }

    /// <summary>
    /// Default value: false.
    /// </summary>
    public string ClientId
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(OneDriveGraphServiceConfigurationNames.ClientId);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.ClientId, Check.NotNullOrWhiteSpace(value, nameof(value)));
    }

    public string SiteId
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(OneDriveGraphServiceConfigurationNames.SiteId);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.SiteId, value);
    }

    public string DriveId
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(OneDriveGraphServiceConfigurationNames.DriveId);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.DriveId, value);
    }

    public string RootPathId
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(OneDriveGraphServiceConfigurationNames.RootPathId);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.RootPathId, value);
    }

    public string RootPath
    {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(OneDriveGraphServiceConfigurationNames.RootPath);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.RootPath, value);
    }

    public bool StandardMode
    {
        get => _containerConfiguration.GetConfigurationOrDefault(OneDriveGraphServiceConfigurationNames.StandardMode, true);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.StandardMode, value);
    }

    public bool CreateContainerIfNotExists
    {
        get => _containerConfiguration.GetConfigurationOrDefault(OneDriveGraphServiceConfigurationNames.CreateContainerIfNotExists, true);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.CreateContainerIfNotExists, value);
    }

    public string ContainerName {
        get => _containerConfiguration.GetConfigurationOrDefault<string>(OneDriveGraphServiceConfigurationNames.ContainerName);
        set => _containerConfiguration.SetConfiguration(OneDriveGraphServiceConfigurationNames.ContainerName, value);
    }

    private readonly BlobContainerConfiguration _containerConfiguration;

    public OneDriveGraphClientConfiguration(BlobContainerConfiguration containerConfiguration)
    {
        _containerConfiguration = containerConfiguration;
    }
}