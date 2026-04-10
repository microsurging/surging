<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.32">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Autofac" Version="4.9.4" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="2.2.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="2.2.4" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.32" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.32">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Common\{{ prefix }}.Core.Common.csproj" />
  </ItemGroup>

    <ItemGroup>
    <Reference Include="Surging.Core.CPlatform">
      <HintPath>..\Packages\Surging_v2.0\Surging.Core.CPlatform.dll</HintPath>
    </Reference>
  </ItemGroup>

  <ItemGroup>
    <Folder Include="Migrations\" />
  </ItemGroup>

</Project>