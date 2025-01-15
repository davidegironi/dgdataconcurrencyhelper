#region License
// Copyright (c) 2014 Davide Gironi
//
// Please refer to LICENSE file for licensing information.
#endregion

using System;

namespace DG.DataConcurrencyHelper.Objects
{
    public class ConcurrencyRecord
    {
        public int Id { get; set; }

        public DGDataConcurrencyHelper.Status Status { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
        public string RecordId { get; set; }
        public string Application { get; set; }
        public string Logusername { get; set; }
        public DateTime Datetime { get; set; }
    }
}
