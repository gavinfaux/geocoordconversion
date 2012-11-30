namespace TDPG.GeoCoordConversion
{
    #region

    using System;

    #endregion

    public class PolarGeoCoordinate
    {
        #region Constructors and Destructors

        public PolarGeoCoordinate(
            double lat, double lon, double height, AngleUnit units, CoordinateSystems coordinateSystem)
        {
            this.Lat = lat;
            this.Lon = lon;
            this.Height = height;
            this.Units = units;
            this.CoordinateSystem = coordinateSystem;
        }

        #endregion

        #region Public Properties

        public CoordinateSystems CoordinateSystem { get; private set; }

        public double Height { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }

        public AngleUnit Units { get; private set; }

        #endregion

        #region Public Methods and Operators

        public static PolarGeoCoordinate ChangeCoordinateSystem(
            PolarGeoCoordinate original, CoordinateSystems destination)
        {
            PolarGeoCoordinate retVal = Converter.Convert(original, destination);
            return retVal;
        }

        public static GridReference ChangeToGridReference(PolarGeoCoordinate original)
        {
            PolarGeoCoordinate convertible = original.CoordinateSystem != CoordinateSystems.OSGB36
                                                 ? ChangeCoordinateSystem(original, CoordinateSystems.OSGB36)
                                                 : original;

            return Converter.GeodesicToGridReference(convertible);
        }

        public static PolarGeoCoordinate ChangeUnits(PolarGeoCoordinate original, AngleUnit changeTo)
        {
            if (original.Units == changeTo)
            {
                return new PolarGeoCoordinate(
                    original.Lat, original.Lon, original.Height, original.Units, original.CoordinateSystem);
            }

            if (original.Units == AngleUnit.Degrees && changeTo == AngleUnit.Radians)
            {
                return new PolarGeoCoordinate(
                    Converter.DegToRad(original.Lat), 
                    Converter.DegToRad(original.Lon), 
                    original.Height, 
                    AngleUnit.Radians, 
                    original.CoordinateSystem);
            }

            if (original.Units == AngleUnit.Radians && changeTo == AngleUnit.Degrees)
            {
                return new PolarGeoCoordinate(
                    Converter.RadToDeg(original.Lat), 
                    Converter.RadToDeg(original.Lon), 
                    original.Height, 
                    AngleUnit.Degrees, 
                    original.CoordinateSystem);
            }

            throw new NotImplementedException("Invalid conversion requested");
        }

        // overload of IsTheSameAs to ignore rounding errors on final digit and align the sigfigs on both coords
        public bool IsTheSameAs(PolarGeoCoordinate compareTo, bool ignoreFinalDigit, bool alignSigFigs)
        {
            if (alignSigFigs)
            {
                this.AlignSigFigs(compareTo);
            }

            if (ignoreFinalDigit)
            {
                compareTo = ReduceSigFigsBy1(compareTo);
                PolarGeoCoordinate comparer = ReduceSigFigsBy1(this);

                bool retVal = comparer.IsTheSameAs(compareTo);
                return retVal;
            }

            return this.IsTheSameAs(compareTo);
        }

        public bool IsTheSameAs(PolarGeoCoordinate compareTo)
        {
            if (compareTo.CoordinateSystem != this.CoordinateSystem || compareTo.Height != this.Height
                || compareTo.Lat != this.Lat || compareTo.Lon != this.Lon || compareTo.Units != this.Units)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Methods

        internal static PolarGeoCoordinate ReduceSigFigsBy1(PolarGeoCoordinate source)
        {
            double Lat = Converter.SetSigFigs(source.Lat, Converter.GetSigFigs(source.Lat) - 1);

            double Lon = Converter.SetSigFigs(source.Lon, Converter.GetSigFigs(source.Lon) - 1);

            double Height = Converter.SetSigFigs(source.Height, Converter.GetSigFigs(source.Height) - 1);

            return new PolarGeoCoordinate(Lat, Lon, Height, source.Units, source.CoordinateSystem);
        }

        internal void AlignSigFigs(PolarGeoCoordinate source)
        {
            int SigFigsSrc = Converter.GetSigFigs(source.Lat);
            int SigFigsDest = Converter.GetSigFigs(this.Lat);

            this.Lat = Converter.SetSigFigs(this.Lat, SigFigsDest < SigFigsSrc ? SigFigsDest : SigFigsSrc);

            SigFigsSrc = Converter.GetSigFigs(source.Lon);
            SigFigsDest = Converter.GetSigFigs(this.Lon);

            this.Lon = Converter.SetSigFigs(this.Lon, SigFigsDest < SigFigsSrc ? SigFigsDest : SigFigsSrc);

            SigFigsSrc = Converter.GetSigFigs(source.Height);
            SigFigsDest = Converter.GetSigFigs(this.Height);

            this.Height = Converter.SetSigFigs(this.Height, SigFigsDest < SigFigsSrc ? SigFigsDest : SigFigsSrc);
        }

        internal void SetSigFigs(int figs)
        {
            this.Lat = Converter.SetSigFigs(this.Lat, figs);

            this.Lon = Converter.SetSigFigs(this.Lon, figs);

            this.Height = Converter.SetSigFigs(this.Height, figs);
        }

        #endregion
    }
}