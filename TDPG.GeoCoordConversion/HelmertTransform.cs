namespace TDPG.GeoCoordConversion
{
    #region

    using System.Collections.Generic;

    #endregion

    internal class HelmertTransform
    {
        // Metres

        // Seconds
        #region Static Fields

        private static readonly Dictionary<HelmertTransformType, HelmertTransform> m_Transforms =
            new Dictionary<HelmertTransformType, HelmertTransform>
                {
                    {
                        HelmertTransformType.WGS84toOSGB36, 
                        new HelmertTransform(
                        -446.448, 
                        125.157, 
                        -542.060, 
                        -0.1502, 
                        -0.2470, 
                        -0.8421, 
                        20.4894, 
                        CoordinateSystems.OSGB36)
                    }, 
                    {
                        HelmertTransformType.OSGB36toWGS84, 
                        new HelmertTransform(
                        446.448, 
                        -125.157, 
                        542.060, 
                        0.1502, 
                        0.2470, 
                        0.8421, 
                        -20.4894, 
                        CoordinateSystems.WGS84)
                    }
                };

        #endregion

        #region Constructors and Destructors

        private HelmertTransform(
            double _tx, 
            double _ty, 
            double _tz, 
            double _rx, 
            double _ry, 
            double _rz, 
            double _s, 
            CoordinateSystems _outputCoordinateSystem)
        {
            this.tx = _tx;
            this.ty = _ty;
            this.tz = _tz;
            this.rx = _rx;
            this.ry = _ry;
            this.rz = _rz;
            this.s = _s;
            this.outputCoordinateSystem = _outputCoordinateSystem;
        }

        #endregion

        #region Enums

        public enum HelmertTransformType
        {
            WGS84toOSGB36, 

            OSGB36toWGS84
        }

        #endregion

        #region Properties

        internal CoordinateSystems outputCoordinateSystem { get; private set; }

        internal double rx { get; private set; }

        internal double ry { get; private set; }

        internal double rz { get; private set; }

        // Parts per million
        internal double s { get; private set; }

        internal double tx { get; private set; }

        internal double ty { get; private set; }

        internal double tz { get; private set; }

        #endregion

        #region Public Methods and Operators

        public static HelmertTransform GetTransform(HelmertTransformType type)
        {
            return m_Transforms[type];
        }

        #endregion
    }
}