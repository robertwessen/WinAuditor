using System;
using System.Collections.Generic;

namespace WinAuditor
{
    public class WinAuditScan 
    {
        public DateTime Time;
        public OperatingSystem ScanOS;
        public List<WinBinAuditv1.SecurityInfo> ScanResults;

        public WinAuditScan()
        {
            this.Time = DateTime.Now;
            this.ScanOS = Environment.OSVersion;
            this.ScanResults = new List<WinBinAuditv1.SecurityInfo>();
        }
        public void AddScanResult(WinBinAuditv1.SecurityInfo scanResult)
        {
            this.ScanResults.Add(scanResult);
        }

        public WinAuditScan getScan()
        {
            return this;
        }
    }

    
}
