using jsreport.Shared;
using jsreport.Types;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Shouldly;
using System.Threading.Tasks;
using jsreport.Binary;
using Newtonsoft.Json.Serialization;

namespace jsreport.Local.Test
{
    [TestFixture]
    [SingleThreaded]
    public class LocalUtilityReportingTest
    {
        private ILocalUtilityReportingService _rs;

        [SetUp]
        public void SetUp()
        {
            _rs = new LocalReporting().KillRunningJsReportProcesses().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();
        }

        [TearDown]
        public void TearDown()
        {
            new LocalReporting().KillRunningJsReportProcesses().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();           
        }

        [Test]
        public async Task TestUtilityRender()
        {
            var result = await _rs.RenderAsync(new RenderRequest()
            {
                Template = new Template()
                {
                    Content = "Hello world",
                    Recipe = Recipe.Html,
                    Engine = Engine.Handlebars
                }
            });

            new StreamReader(result.Content).ReadToEnd().ShouldBe("Hello world");
        }

        [Test] 
        public void TestUtilityRenderSimultaneous()
        {
            var tasks = Enumerable.Range(0, 3).Select(async (i) =>
            {
                var result = await _rs.RenderAsync(new RenderRequest()
                {
                    Template = new Template()
                    {
                        Content = "Hello world",
                        Recipe = Recipe.Html,
                        Engine = Engine.Handlebars
                    }
                });

                new StreamReader(result.Content).ReadToEnd().ShouldBe("Hello world");
            });

            Task.WaitAll(tasks.ToArray());
        }
    }    
    
    [TestFixture]
    [SingleThreaded]
    public class LocalUtilityReportingInCustomTempTest
    {
        private ILocalUtilityReportingService _rs;

        [SetUp]
        public void SetUp()
        {
            _rs = new LocalReporting()
                .KillRunningJsReportProcesses()
                .Configure((cfg) =>
                {
                    cfg.TempDirectory = Path.Combine(Path.GetTempPath(), "jsreport with space temp");
                    return cfg;
                }).UseBinary(JsReportBinary.GetBinary())
                .AsUtility()
                .Create();
        }

        [TearDown]
        public void TearDown()
        {
            new LocalReporting().KillRunningJsReportProcesses().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();           
        }

        [Test]
        public async Task TestUtilityRenderWithTempPathIncludingSpace()
        {
            var result = await _rs.RenderAsync(new RenderRequest()
            {
                Template = new Template()
                {
                    Content = "Hello world",
                    Recipe = Recipe.Html,
                    Engine = Engine.Handlebars
                }
            });

            new StreamReader(result.Content).ReadToEnd().ShouldBe("Hello world");
        }
    }

    [TestFixture]
    [SingleThreaded]
    public class LocalUtilityReportingWithCustomPathTest
    {
        private ILocalUtilityReportingService _rs;

        [SetUp]
        public void SetUp()
        {
            _rs = new LocalReporting()
                .KillRunningJsReportProcesses()
                .RunInDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\jsreportTest"))     
                .Configure((cfg => cfg.FileSystemStore()))
                .UseBinary(JsReportBinary.GetBinary())
                .AsUtility()
                .Create();
        }

        [TearDown]
        public void TearDown()
        {
            new LocalReporting().KillRunningJsReportProcesses().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();
        }

        [Test]
        public async Task TestUtilityRenderWithCustomRunInDirectory()
        {
            var result = await _rs.RenderAsync(new RenderRequest()
            {
                Template = new Template()
                {
                    Name = "Invoice"                    
                }
            });

            new StreamReader(result.Content).ReadToEnd().ShouldBe("Hello");
        }
    }

    [TestFixture]
    [SingleThreaded]
    public class LocalUtilityReportingWithCustomDataContractSerializer
    {
        private ILocalUtilityReportingService _rs;

        [SetUp]
        public void SetUp()
        {
            _rs = new LocalReporting()
                .KillRunningJsReportProcesses()
                .UseBinary(JsReportBinary.GetBinary())
                .UseContractResolverForDataProperty(new CamelCasePropertyNamesContractResolver())
                .AsUtility()
                .Create();
        }

        [TearDown]
        public void TearDown()
        {
            new LocalReporting().KillRunningJsReportProcesses().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();
        }

        [Test]
        public async Task TestDataSerializeWithCamelCase()
        {
            var result = await _rs.RenderAsync(new RenderRequest()
            {
                Template = new Template()
                {
                    Content = "{{helloWorld}}",
                    Recipe = Recipe.Html,
                    Engine = Engine.Handlebars
                },
                Data = new
                {
                    HelloWorld = "foo"
                }
            });

            new StreamReader(result.Content).ReadToEnd().ShouldBe("foo");
        }
    }
}