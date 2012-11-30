namespace TDPG.GeoCoordConversion
{
    #region

    using System;
    using System.Globalization;

    #endregion

    /// <summary>
    ///     Set of static methods to do hard maths.
    ///     Convert, GeodesicToGridReference and GridReferenceToGeodesic methods
    ///     are adapted from Chris Vernes JavaScript (LGPL license)
    ///     found at:
    ///     http://www.movable-type.co.uk/scripts/latlong-gridref.html
    ///     http://www.movable-type.co.uk/scripts/latlong-convert-coords.html
    /// </summary>
    internal static class Converter
    {
        #region Constants

        private const double _NatGridScaleFactor = 0.9996012717; // NatGrid scale factor on central meridian

        #endregion

        #region Static Fields

        private static readonly EllipseParameter _Airy1830 =
            EllipseParameter.GetEllipseParameters(EllipseParameter.Ellipses.Airy1830);

        private static readonly PolarGeoCoordinate _NatGridTrueOrigin =
            PolarGeoCoordinate.ChangeUnits(
                new PolarGeoCoordinate(49, -2, 0, AngleUnit.Degrees, CoordinateSystems.OSGB36), AngleUnit.Radians);

        private static readonly GridReference _NatNETrueOrigin = new GridReference(400000, -100000);

        private static readonly double _e2 = 1
                                             - Math.Pow(_Airy1830.SemiMinorAxis, 2)
                                             / Math.Pow(_Airy1830.SemimajorAxis, 2);

        private static readonly double _n = (_Airy1830.SemimajorAxis - _Airy1830.SemiMinorAxis)
                                            / (_Airy1830.SemimajorAxis + _Airy1830.SemiMinorAxis);

        private static readonly double _n2 = Math.Pow(_n, 2);

        private static readonly double _n3 = Math.Pow(_n, 3);

        #endregion

        #region Methods

        internal static PolarGeoCoordinate Convert(PolarGeoCoordinate source, CoordinateSystems destination)
        {
            PolarGeoCoordinate retVal;
            if (source.CoordinateSystem == CoordinateSystems.OSGB36 && destination == CoordinateSystems.WGS84)
            {
                retVal = Convert(
                    source, 
                    EllipseParameter.GetEllipseParameters(EllipseParameter.Ellipses.Airy1830), 
                    HelmertTransform.GetTransform(HelmertTransform.HelmertTransformType.OSGB36toWGS84), 
                    EllipseParameter.GetEllipseParameters(EllipseParameter.Ellipses.WGS84));
            }
            else if (source.CoordinateSystem == CoordinateSystems.WGS84 && destination == CoordinateSystems.OSGB36)
            {
                retVal = Convert(
                    source, 
                    EllipseParameter.GetEllipseParameters(EllipseParameter.Ellipses.WGS84), 
                    HelmertTransform.GetTransform(HelmertTransform.HelmertTransformType.WGS84toOSGB36), 
                    EllipseParameter.GetEllipseParameters(EllipseParameter.Ellipses.Airy1830));
            }
            else
            {
                throw new NotImplementedException("Requested source and destination are not currently supported");
            }

            retVal.AlignSigFigs(source);
            return retVal;
        }

        internal static double DegToRad(double original)
        {
            return (original / 360) * (2 * Math.PI);
        }

        internal static GridReference GeodesicToGridReference(PolarGeoCoordinate originalCoords)
        {
            PolarGeoCoordinate originalRads = PolarGeoCoordinate.ChangeUnits(originalCoords, AngleUnit.Radians);

            double cosLat = Math.Cos(originalRads.Lat);
            double sinLat = Math.Sin(originalRads.Lat);
            double nu = _Airy1830.SemimajorAxis * _NatGridScaleFactor / Math.Sqrt(1 - _e2 * sinLat * sinLat);

            // transverse radius of curvature
            double rho = _Airy1830.SemimajorAxis * _NatGridScaleFactor * (1 - _e2)
                         / Math.Pow(1 - _e2 * sinLat * sinLat, 1.5); // meridional radius of curvature
            double eta2 = nu / rho - 1;

            double Ma = (1 + _n + (5d / 4d) * _n2 + (5d / 4d) * _n3) * (originalRads.Lat - _NatGridTrueOrigin.Lat);
            double Mb = (3 * _n + 3 * _n * _n + (21d / 8d) * _n3) * Math.Sin(originalRads.Lat - _NatGridTrueOrigin.Lat)
                        * Math.Cos(originalRads.Lat + _NatGridTrueOrigin.Lat);
            double Mc = ((15d / 8d) * _n2 + (15d / 8d) * _n3)
                        * Math.Sin(2 * (originalRads.Lat - _NatGridTrueOrigin.Lat))
                        * Math.Cos(2 * (originalRads.Lat + _NatGridTrueOrigin.Lat));
            double Md = (35d / 24d) * _n3 * Math.Sin(3 * (originalRads.Lat - _NatGridTrueOrigin.Lat))
                        * Math.Cos(3 * (originalRads.Lat + _NatGridTrueOrigin.Lat));
            double M = _Airy1830.SemiMinorAxis * _NatGridScaleFactor * (Ma - Mb + Mc - Md); // meridional arc

            double cos3lat = cosLat * cosLat * cosLat;
            double cos5lat = cos3lat * cosLat * cosLat;
            double tan2lat = Math.Tan(originalRads.Lat) * Math.Tan(originalRads.Lat);
            double tan4lat = tan2lat * tan2lat;

            double I = M + _NatNETrueOrigin.Northing;
            double II = (nu / 2) * sinLat * cosLat;
            double III = (nu / 24) * sinLat * cos3lat * (5 - tan2lat + 9 * eta2);
            double IIIA = (nu / 720) * sinLat * cos5lat * (61 - 58 * tan2lat + tan4lat);
            double IV = nu * cosLat;
            double V = (nu / 6) * cos3lat * (nu / rho - tan2lat);
            double VI = (nu / 120) * cos5lat * (5 - 18 * tan2lat + tan4lat + 14 * eta2 - 58 * tan2lat * eta2);

            double dLon = originalRads.Lon - _NatGridTrueOrigin.Lon;
            double dLon2 = dLon * dLon, 
                   dLon3 = dLon2 * dLon, 
                   dLon4 = dLon3 * dLon, 
                   dLon5 = dLon4 * dLon, 
                   dLon6 = dLon5 * dLon;

            double N = I + II * dLon2 + III * dLon4 + IIIA * dLon6;
            double E = _NatNETrueOrigin.Easting + IV * dLon + V * dLon3 + VI * dLon5;

            return new GridReference((int)E, (int)N);
        }

        internal static int GetSigFigs(double d)
        {
            if (d == 0)
            {
                return 0;
            }

            double scale = d >= 0
                               ? Math.Pow(10, Math.Floor(Math.Log10(d)) + 1)
                               : Math.Pow(10, Math.Floor(Math.Log10(-d)) + 1);

            double allAfterDecimal = d / scale;

            int retVal = Math.Abs(allAfterDecimal).ToString(CultureInfo.InvariantCulture).Trim('0').Length - 1;

            return retVal;
        }

        internal static PolarGeoCoordinate GridReferenceToGeodesic(GridReference g)
        {
            double lat = _NatGridTrueOrigin.Lat;
            double M = 0;
            do
            {
                lat = (g.Northing - _NatNETrueOrigin.Northing - M) / (_Airy1830.SemimajorAxis * _NatGridScaleFactor)
                      + lat;

                double Ma = (1 + _n + (5d / 4d) * _n2 + (5d / 4d) * _n3) * (lat - _NatGridTrueOrigin.Lat);
                double Mb = (3 * _n + 3 * _n * _n + (21d / 8d) * _n3) * Math.Sin(lat - _NatGridTrueOrigin.Lat)
                            * Math.Cos(lat + _NatGridTrueOrigin.Lat);
                double Mc = ((15d / 8d) * _n2 + (15d / 8d) * _n3) * Math.Sin(2 * (lat - _NatGridTrueOrigin.Lat))
                            * Math.Cos(2 * (lat + _NatGridTrueOrigin.Lat));
                double Md = (35d / 24d) * _n3 * Math.Sin(3 * (lat - _NatGridTrueOrigin.Lat))
                            * Math.Cos(3 * (lat + _NatGridTrueOrigin.Lat));
                M = _Airy1830.SemiMinorAxis * _NatGridScaleFactor * (Ma - Mb + Mc - Md); // meridional arc
            }
            while (g.Northing - _NatNETrueOrigin.Northing - M >= 0.00001); // ie until < 0.01mm

            double cosLat = Math.Cos(lat), sinLat = Math.Sin(lat);
            double nu = _Airy1830.SemimajorAxis * _NatGridScaleFactor / Math.Sqrt(1 - _e2 * sinLat * sinLat);

            // transverse radius of curvature
            double rho = _Airy1830.SemimajorAxis * _NatGridScaleFactor * (1 - _e2)
                         / Math.Pow(1 - _e2 * sinLat * sinLat, 1.5); // meridional radius of curvature
            double eta2 = nu / rho - 1;

            double tanLat = Math.Tan(lat);
            double tan2lat = tanLat * tanLat, tan4lat = tan2lat * tan2lat, tan6lat = tan4lat * tan2lat;
            double secLat = 1 / cosLat;
            double nu3 = nu * nu * nu, nu5 = nu3 * nu * nu, nu7 = nu5 * nu * nu;
            double VII = tanLat / (2 * rho * nu);
            double VIII = tanLat / (24 * rho * nu3) * (5 + 3 * tan2lat + eta2 - 9 * tan2lat * eta2);
            double IX = tanLat / (720 * rho * nu5) * (61 + 90 * tan2lat + 45 * tan4lat);
            double X = secLat / nu;
            double XI = secLat / (6 * nu3) * (nu / rho + 2 * tan2lat);
            double XII = secLat / (120 * nu5) * (5 + 28 * tan2lat + 24 * tan4lat);
            double XIIA = secLat / (5040 * nu7) * (61 + 662 * tan2lat + 1320 * tan4lat + 720 * tan6lat);

            double dE = g.Easting - _NatNETrueOrigin.Easting;
            double dE2 = dE * dE;
            double dE3 = dE2 * dE;
            double dE4 = dE2 * dE2;
            double dE5 = dE3 * dE2;
            double dE6 = dE4 * dE2;
            double dE7 = dE5 * dE2;
            lat = lat - VII * dE2 + VIII * dE4 - IX * dE6;
            double lon = _NatGridTrueOrigin.Lon + X * dE - XI * dE3 + XII * dE5 - XIIA * dE7;

            var retVal = new PolarGeoCoordinate(lat, lon, 0, AngleUnit.Radians, CoordinateSystems.OSGB36);
            retVal = PolarGeoCoordinate.ChangeUnits(retVal, AngleUnit.Degrees);
            retVal.SetSigFigs(7);
            return retVal;
        }

        internal static double RadToDeg(double original)
        {
            return (original / (2 * Math.PI)) * 360;
        }

        internal static double SetSigFigs(double d, int digits)
        {
            if (d == 0 || digits == 0)
            {
                return 0.0;
            }

            double scale = d >= 0
                               ? Math.Pow(10, Math.Floor(Math.Log10(d)) + 1)
                               : Math.Pow(10, Math.Floor(Math.Log10(-d)) + 1);

            double allAfterDecimal = d / scale;
            double rounded = Math.Round(allAfterDecimal, digits);

            decimal retVal = (decimal)scale * (decimal)rounded;

            return (double)retVal;
        }

        private static PolarGeoCoordinate Convert(
            PolarGeoCoordinate originalCoord, EllipseParameter e1, HelmertTransform t, EllipseParameter e2)
        {
            // -- convert polar to cartesian coordinates (using ellipse 1)            
            PolarGeoCoordinate p1 = PolarGeoCoordinate.ChangeUnits(originalCoord, AngleUnit.Radians);

            double sinPhi = Math.Sin(p1.Lat);
            double cosPhi = Math.Cos(p1.Lat);
            double sinLambda = Math.Sin(p1.Lon);
            double cosLambda = Math.Cos(p1.Lon);

            double H = p1.Height;

            double eSq = (Math.Pow(e1.SemimajorAxis, 2) - Math.Pow(e1.SemiMinorAxis, 2)) / Math.Pow(e1.SemimajorAxis, 2);
            double nu = e1.SemimajorAxis / Math.Sqrt(1 - eSq * sinPhi * sinPhi);

            double x1 = (nu + H) * cosPhi * cosLambda;
            double y1 = (nu + H) * cosPhi * sinLambda;
            double z1 = ((1 - eSq) * nu + H) * sinPhi;

            // -- apply helmert transform using appropriate params
            double tx = t.tx, ty = t.ty, tz = t.tz;
            double rx = t.rx / 3600 * Math.PI / 180; // normalise seconds to radians
            double ry = t.ry / 3600 * Math.PI / 180;
            double rz = t.rz / 3600 * Math.PI / 180;
            double s1 = t.s / 1e6 + 1; // normalise ppm to (s+1)

            // apply transform
            double x2 = tx + x1 * s1 - y1 * rz + z1 * ry;
            double y2 = ty + x1 * rz + y1 * s1 - z1 * rx;
            double z2 = tz - x1 * ry + y1 * rx + z1 * s1;

            // -- convert cartesian to polar coordinates (using ellipse 2)
            double a = e2.SemimajorAxis;

            double precision = 4 / e2.SemimajorAxis; // results accurate to around 4 metres

            eSq = (Math.Pow(e2.SemimajorAxis, 2) - Math.Pow(e2.SemiMinorAxis, 2)) / Math.Pow(e2.SemimajorAxis, 2);
            double p = Math.Sqrt(x2 * x2 + y2 * y2);
            double phi = Math.Atan2(z2, p * (1 - eSq)), phiP = 2 * Math.PI;
            while (Math.Abs(phi - phiP) > precision)
            {
                nu = a / Math.Sqrt(1 - eSq * Math.Sin(phi) * Math.Sin(phi));
                phiP = phi;
                phi = Math.Atan2(z2 + eSq * nu * Math.Sin(phi), p);
            }

            double lambda = Math.Atan2(y2, x2);
            H = p / Math.Cos(phi) - nu;

            return new PolarGeoCoordinate(
                RadToDeg(phi), RadToDeg(lambda), H, AngleUnit.Degrees, t.outputCoordinateSystem);
        }

        #endregion
    }
}