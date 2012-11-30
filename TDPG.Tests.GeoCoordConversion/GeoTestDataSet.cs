namespace TDPG.GeoCoordConversion.Test
{
    internal class GeoTestDataSet
    {
        #region Constructors and Destructors

        internal GeoTestDataSet(string city, PolarGeoCoordinate oSGB36, PolarGeoCoordinate wGS84, GridReference nE)
        {
            this.City = city;
            this.OSGB36 = oSGB36;
            this.WGS84 = wGS84;
            this.NE = nE;
        }

        #endregion

        #region Properties

        internal string City { get; set; }

        internal GridReference NE { get; set; }

        internal PolarGeoCoordinate OSGB36 { get; set; }

        internal PolarGeoCoordinate WGS84 { get; set; }

        #endregion
    }
}