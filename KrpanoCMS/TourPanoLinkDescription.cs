//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KrpanoCMS
{
    using System;
    using System.Collections.Generic;
    
    public partial class TourPanoLinkDescription
    {
        public int Id { get; set; }
        public int FkPanoId { get; set; }
        public int FkTourId { get; set; }
        public double Coordinate_X { get; set; }
        public double Coordinate_Y { get; set; }
        public int FkPanoIdLinkedTo { get; set; }
    }
}
