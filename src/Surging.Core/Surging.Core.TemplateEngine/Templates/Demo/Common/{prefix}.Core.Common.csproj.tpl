<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

      <ItemGroup>
    <Reference Include="Surging.Core.CPlatform">
      <HintPath>..\Packages\Surging_v2.0\Surging.Core.CPlatform.dll</HintPath>
    </Reference>
  </ItemGroup>

    <ItemGroup>
        <PackageReference Include="Autofac" Version="4.9.4" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.32" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="6.0.32" />
  </ItemGroup>
</Project>