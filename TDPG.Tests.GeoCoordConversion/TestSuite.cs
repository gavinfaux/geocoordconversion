namespace TDPG.GeoCoordConversion.Test
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestSuite
    {
        // You can use the following additional attributes as you write your tests:
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        #region Static Fields

        /// <summary>
        ///     Following values come from http://www.nearby.org.uk/coord.cgi?p=BN1+6PJ&f=conv
        /// </summary>
        private static readonly List<GeoTestDataSet> TestData = new List<GeoTestDataSet>
                                                                    {
                                                                        new GeoTestDataSet(
                                                                            "Brighton", 
                                                                            new PolarGeoCoordinate(
                                                                            50.84609, 
                                                                            -0.1424094, 
                                                                            0, 
                                                                            AngleUnit.Degrees, 
                                                                            CoordinateSystems
                                                                            .OSGB36), 
                                                                            new PolarGeoCoordinate(
                                                                            50.84668, 
                                                                            -0.1439875, 
                                                                            0, 
                                                                            AngleUnit.Degrees, 
                                                                            CoordinateSystems.WGS84), 
                                                                            new GridReference(
                                                                            530760, 106880)), 
                                                                        new GeoTestDataSet(
                                                                            "Newcastle", 
                                                                            new PolarGeoCoordinate(
                                                                            54.979808, 
                                                                            -1.584025, 
                                                                            0, 
                                                                            AngleUnit.Degrees, 
                                                                            CoordinateSystems
                                                                            .OSGB36), 
                                                                            new PolarGeoCoordinate(
                                                                            54.979889, 
                                                                            -1.585609, 
                                                                            0, 
                                                                            AngleUnit.Degrees, 
                                                                            CoordinateSystems.WGS84), 
                                                                            new GridReference(
                                                                            426620, 565110)), 
                                                                        new GeoTestDataSet(
                                                                            "Truro", 
                                                                            new PolarGeoCoordinate(
                                                                            50.262067, 
                                                                            -5.052743, 
                                                                            0, 
                                                                            AngleUnit.Degrees, 
                                                                            CoordinateSystems
                                                                            .OSGB36), 
                                                                            new PolarGeoCoordinate(
                                                                            50.262655, 
                                                                            -5.053748, 
                                                                            0, 
                                                                            AngleUnit.Degrees, 
                                                                            CoordinateSystems.WGS84), 
                                                                            new GridReference(
                                                                            182450, 044760)), 
                                                                        
                                                                        // I can't get sufficiently good quality data for belfast to include this
                                                                        // {new GeoTestDataSet(
                                                                        // "Belfast",
                                                                        // //OSGB36 is calculated using this tool since I can't any osgb36 coords for NI
                                                                        // PolarGeoCoordinate.ChangeCoordinateSystem(
                                                                        // new PolarGeoCoordinate(54.579254, -5.934520, 0, AngleUnit.Degrees, CoordinateSystems.WGS84),
                                                                        // CoordinateSystems.OSGB36),
                                                                        // new PolarGeoCoordinate(54.579254, -5.934520, 0, AngleUnit.Degrees, CoordinateSystems.WGS84),
                                                                        // new GridReference(145849, 527567))},
                                                                        new GeoTestDataSet(
                                                                            "John O'Groats", 
                                                                            new PolarGeoCoordinate(
                                                                            58.639451, 
                                                                            -3.069178, 
                                                                            0, 
                                                                            AngleUnit.Degrees, 
                                                                            CoordinateSystems
                                                                            .OSGB36), 
                                                                            new PolarGeoCoordinate(
                                                                            58.639073, 
                                                                            -3.070747, 
                                                                            0, 
                                                                            AngleUnit.Degrees, 
                                                                            CoordinateSystems.WGS84), 
                                                                            new GridReference(
                                                                            337940, 972850))
                                                                    };

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void TestDegreesToRadians()
        {
            var deg = new PolarGeoCoordinate(180, 180, 0, AngleUnit.Degrees, CoordinateSystems.OSGB36);
            var rad = new PolarGeoCoordinate(Math.PI, Math.PI, 0, AngleUnit.Radians, CoordinateSystems.OSGB36);

            Assert.IsTrue(rad.IsTheSameAs(PolarGeoCoordinate.ChangeUnits(deg, AngleUnit.Radians)));
        }

        [TestMethod]
        public void TestGridReferenceComparison()
        {
            Assert.IsTrue(
                TestData[0].NE.IsTheSameAs(TestData[0].NE) && !TestData[0].NE.IsTheSameAs(TestData[1].NE)
                && TestData[0].NE.IsTheSameAs(TestData[0].NE, true) && !TestData[0].NE.IsTheSameAs(TestData[1].NE, true));
        }

        [TestMethod]
        public void TestGridrefToPolarGeo()
        {
            var sb = new StringBuilder();

            foreach (GeoTestDataSet item in TestData)
            {
                PolarGeoCoordinate converted = GridReference.ChangeToPolarGeo(item.NE);

                if (!item.OSGB36.IsTheSameAs(converted, true, true))
                {
                    sb.AppendLine(item.City);
                }
            }

            if (sb.Length > 0)
            {
                sb.AppendLine("failed out of" + TestData.Count.ToString(CultureInfo.InvariantCulture));
                Assert.Fail(sb.ToString());
            }
        }

        [TestMethod]
        public void TestOSGB36PolarGeoToGridref()
        {
            var sb = new StringBuilder();

            foreach (GeoTestDataSet item in TestData)
            {
                GridReference converted = PolarGeoCoordinate.ChangeToGridReference(item.OSGB36);

                if (!item.NE.IsTheSameAs(converted, true))
                {
                    sb.AppendLine(item.City);
                }
            }

            if (sb.Length > 0)
            {
                sb.AppendLine("failed out of" + TestData.Count.ToString(CultureInfo.InvariantCulture));
                Assert.Fail(sb.ToString());
            }
        }

        [TestMethod]
        public void TestOSGB36ToWGS84()
        {
            var sb = new StringBuilder();

            foreach (GeoTestDataSet item in TestData)
            {
                PolarGeoCoordinate converted = PolarGeoCoordinate.ChangeCoordinateSystem(
                    item.OSGB36, CoordinateSystems.WGS84);

                if (!item.WGS84.IsTheSameAs(converted, true, true))
                {
                    sb.AppendLine(item.City);
                }
            }

            if (sb.Length > 0)
            {
                sb.AppendLine("failed out of" + TestData.Count.ToString(CultureInfo.InvariantCulture));
                Assert.Fail(sb.ToString());
            }
        }

        [TestMethod]
        public void TestPolarGeoCoordComparison()
        {
            Assert.IsTrue(
                TestData[0].OSGB36.IsTheSameAs(TestData[0].OSGB36) && !TestData[0].OSGB36.IsTheSameAs(TestData[0].WGS84)
                && TestData[0].OSGB36.IsTheSameAs(TestData[0].OSGB36, true, true)
                && !TestData[0].OSGB36.IsTheSameAs(TestData[0].WGS84, true, true));
        }

        [TestMethod]
        public void TestRadiansToDegrees()
        {
            var deg = new PolarGeoCoordinate(180, 180, 0, AngleUnit.Degrees, CoordinateSystems.OSGB36);
            var rad = new PolarGeoCoordinate(Math.PI, Math.PI, 0, AngleUnit.Radians, CoordinateSystems.OSGB36);

            Assert.IsTrue(deg.IsTheSameAs(PolarGeoCoordinate.ChangeUnits(rad, AngleUnit.Degrees)));
        }

        [TestMethod]
        public void TestWGS84PolarGeoToGridref()
        {
            var sb = new StringBuilder();

            foreach (GeoTestDataSet item in TestData)
            {
                GridReference converted = PolarGeoCoordinate.ChangeToGridReference(item.WGS84);

                if (!item.NE.IsTheSameAs(converted, true))
                {
                    sb.AppendLine(item.City);
                }
            }

            if (sb.Length > 0)
            {
                sb.AppendLine("failed out of" + TestData.Count.ToString(CultureInfo.InvariantCulture));
                Assert.Fail(sb.ToString());
            }
        }

        [TestMethod]
        public void TestWGS84ToOSGB36TestOSGB36ToWGS84()
        {
            var sb = new StringBuilder();

            foreach (GeoTestDataSet item in TestData)
            {
                PolarGeoCoordinate converted = PolarGeoCoordinate.ChangeCoordinateSystem(
                    item.WGS84, CoordinateSystems.OSGB36);

                if (!item.OSGB36.IsTheSameAs(converted, true, true))
                {
                    sb.AppendLine(item.City);
                }
            }

            if (sb.Length > 0)
            {
                sb.AppendLine("failed out of" + TestData.Count.ToString(CultureInfo.InvariantCulture));
                Assert.Fail(sb.ToString());
            }
        }

        #endregion
    }
}