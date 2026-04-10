<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
    <ItemGroup> 
    <ProjectReference Include="..\IModuleService\{{ prefix }}.IModuleService.{{ project.name }}.csproj" />
    <ProjectReference Include="..\Common\{{ prefix }}.Core.Common.csproj" />
      <ProjectReference Include="..\DataAccess\{{ prefix }}.DataAccess.{{ project.name }}.csproj" />
  </ItemGroup>

    <ItemGroup>
    <Reference Include="Surging.Core.CPlatform">
      <HintPath>..\Packages\Surging_v2.0\Surging.Core.CPlatform.dll</HintPath>
    </Reference>
        <Reference Include="Surging.Core.ProxyGenerator">
      <HintPath>..\Packages\Surging_v2.0\Surging.Core.ProxyGenerator.dll</HintPath>
    </Reference>
  </ItemGroup>

</Project>