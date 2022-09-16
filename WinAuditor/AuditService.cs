using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.IO;
using WinBinAudit;
using System.Linq;
using System.Text.RegularExpressions;

namespace WinAuditor
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _log;
        private readonly IConfiguration _config;
        private v5 _v5;
        private WinAuditScan _winAuditScan;
        private const string _version = "6.0.0";
        


        public AuditService(ILogger<AuditService> log, IConfiguration config)
        {
            _log = log;
            _config = config;
            _v5 = new v5();
            _winAuditScan = new WinAuditScan();
            _log.LogInformation("WinAuditor Version: {version}", _version);
            _log.LogInformation("Audit Lib Loaded");
            _log.LogInformation("Logging to File: {logfile}", _config.GetValue<string>("OutputLog"));

        }
        public void Run()
        {

            _log.LogInformation("Scan Input Path: {path}", _config.GetValue<string>("ScanPath"));
            _log.LogInformation("Scan Filter Search Pattern: {regex}", _config.GetValue<string>("SearchPattern"));
            _log.LogInformation("Recursive: {recurse}", _config.GetValue<string>("Recursive"));
            _log.LogInformation("Output File: {outputfile}", _config.GetValue<string>("OutputFile"));
            var files = new List<string>();
            files = this.GatherFiles(_config.GetValue<string>("ScanPath"));
            foreach(string file in files){
                this.ScanFile(file);
            }
            using (StreamWriter file = File.CreateText(_config.GetValue<string>("OutputFile")))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _winAuditScan);
            }
        }

        private List<string> GatherFiles(string path)
        {
            var files = new List<string>();
            FileAttributes fileAttr = new FileAttributes();
            try
            {
                fileAttr = File.GetAttributes(path);
            }
            catch
            {
                _log.LogError("Invalid File Specified: {path}", path);
                return files;
            }

            if ((fileAttr & FileAttributes.Directory) == FileAttributes.Directory){
                // _log.LogWarning("Directory Scanning not yet Implemented");
                var recurse = _config.GetValue<bool>("Recursive") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string regexString = (_config.GetValue<string>("SearchPattern") != null) ? _config.GetValue<string>("SearchPattern") : @"\\b[0-9a-zA-Z]*(\\.exe|\\.dll|\\.scr|\\.sys)$";
                //_log.LogInformation(regexString);
                Regex pattern = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                // Directory.GetFiles() doesnt handle real regexes, so this Linq filter is the next easiest way to do it
                files = Directory.GetFiles(path, "*", recurse).Where(dir => pattern.IsMatch(dir)).ToList();
                return files;
            }
            else
            {
                files.Add(path);
                return files;
            }
        }



        private v5 ScanFile(string fileName)
        {
            _log.LogInformation("Processing: {filename}", fileName);
            v5 auditor = new v5();
            auditor.ProcessFile(fileName);
            this._winAuditScan.AddScanResult(auditor.SecInfo);
            var logOutput = new List<string>();
            logOutput.Add(auditor.SecInfo.FileName);
            logOutput.Add(auditor.SecInfo.Version);
            logOutput.Add(auditor.SecInfo.SHA2);
            _log.LogInformation("Processed: {logOutput}", JsonConvert.SerializeObject(logOutput));
            return auditor;
        }
    }
}
