//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BAN_VE_CINE
{
    using System;
    using System.Collections.Generic;
    
    public partial class CTHD
    {
        public int ma { get; set; }
        public Nullable<int> maHD { get; set; }
        public string vitrighe { get; set; }
        public Nullable<decimal> sotien { get; set; }
    
        public virtual HOADON HOADON { get; set; }
    }
}
