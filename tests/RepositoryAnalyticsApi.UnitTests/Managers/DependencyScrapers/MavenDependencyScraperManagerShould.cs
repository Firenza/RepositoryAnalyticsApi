using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnaltyicsApi.Managers.DependencyScrapers;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.UnitTests
{
    [TestClass]
    public class MavenDependencyScraperManagerShould
    {
        private Mock<IRepositorySourceManager> mockRepositorySourceManger;
        private MavenDependencyScraperManager manager;

        [TestInitialize]
        public void Initailize()
        {
            this.mockRepositorySourceManger = new Mock<IRepositorySourceManager>();
            this.manager = new MavenDependencyScraperManager(this.mockRepositorySourceManger.Object);
        }

        [TestMethod]
        public async Task ParseNonVariablizedNonScopedDependency()
        {
            // Arrange
            var owner = "bob";
            var name = "someRepo";
            var branch = "master";
            var asOf = DateTime.Now;

            var mavenFile = new ServiceModel.RepositoryFile
            {
                Name = "pom.xml",
                FullPath = "/repo/src/pom.xml"
            };

            var mavenFileContent = await File.ReadAllTextAsync(@"Managers\DependencyScrapers\TestDependencyFiles\ValidMavenPom.xml");

            this.mockRepositorySourceManger
                .Setup(mock => mock.ReadFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new List<ServiceModel.RepositoryFile> { mavenFile });

            this.mockRepositorySourceManger
                .Setup(mock => mock.ReadFileContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(mavenFileContent);

            var expectedDependency = new RepositoryDependency
            {
                Name = "httpclient",
                Version = "4.5.3",
                MajorVersion = "4",
                RepoPath = mavenFile.FullPath,
                Source = "Maven",
            };

            // Act
            var dependencies = await this.manager.ReadAsync(owner, name, branch, asOf);

            // Assert
            dependencies.Should().Contain(expectedDependency);


        }
    }
}
