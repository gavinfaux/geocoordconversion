namespace TDPG.GeoCoordConversion
{
    public class GridReference
    {
        #region Constructors and Destructors

        public GridReference(long easting, long northing)
        {
            this.Northing = northing;
            this.Easting = easting;
        }

        #endregion

        #region Public Properties

        public long Easting { get; set; }

        public long Northing { get; set; }

        #endregion

        #region Public Methods and Operators

        public static PolarGeoCoordinate ChangeToPolarGeo(GridReference original)
        {
            return Converter.GridReferenceToGeodesic(original);
        }

        public bool IsTheSameAs(GridReference compareTo)
        {
            if (compareTo.Northing != this.Northing || compareTo.Easting != this.Easting)
            {
                return false;
            }

            return true;
        }

        // overload of IsTheSameAs to ignore rounding errors on final digit
        public bool IsTheSameAs(GridReference compareTo, bool ignoreFinalDigit)
        {
            if (ignoreFinalDigit)
            {
                this.AlignSigFigs(compareTo);
                GridReference compareToReduced = ReduceSigFigsBy1(compareTo);
                GridReference comparer = ReduceSigFigsBy1(this);

                return comparer.IsTheSameAs(compareToReduced);
            }

            return this.IsTheSameAs(compareTo);
        }

        #endregion

        #region Methods

        internal static GridReference ReduceSigFigsBy1(GridReference original)
        {
            double Northing = Converter.SetSigFigs(original.Northing, Converter.GetSigFigs(original.Northing) - 1);

            double Easting = Converter.SetSigFigs(original.Easting, Converter.GetSigFigs(original.Easting) - 1);

            return new GridReference((int)Easting, (int)Northing);
        }

        internal void AlignSigFigs(GridReference source)
        {
            int SigFigsSrc = Converter.GetSigFigs(source.Easting);
            int SigFigsDest = Converter.GetSigFigs(this.Easting);

            this.Easting = (int)Converter.SetSigFigs(this.Easting, SigFigsDest < SigFigsSrc ? SigFigsDest : SigFigsSrc);

            source.Easting =
                (int)Converter.SetSigFigs(source.Easting, SigFigsDest < SigFigsSrc ? SigFigsDest : SigFigsSrc);

            SigFigsSrc = Converter.GetSigFigs(source.Northing);
            SigFigsDest = Converter.GetSigFigs(this.Northing);

            this.Northing =
                (int)Converter.SetSigFigs(this.Northing, SigFigsDest < SigFigsSrc ? SigFigsDest : SigFigsSrc);

            source.Northing =
                (int)Converter.SetSigFigs(source.Northing, SigFigsDest < SigFigsSrc ? SigFigsDest : SigFigsSrc);
        }

        #endregion
    }
}