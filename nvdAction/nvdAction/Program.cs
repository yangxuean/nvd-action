using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using nspector.Common;
using nspector.Common.Helper;
using nspector.Native.NVAPI2;
using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;

namespace nvidiaProfileInspector
{
    class Program
    {
        static DrsSettingsService _drs;
        static DrsSettingsMetaService _meta;
        static DrsScannerService _scanner;
        static DrsImportService _import; 
       static  Program()
        {
            _drs = DrsServiceLocator.SettingService;
            _meta = DrsServiceLocator.MetaService;
            _scanner = DrsServiceLocator.ScannerService;
            _import = DrsServiceLocator.ImportService;
        }

       static void AddToModifiedProfiles(string profileName, bool userProfile = false)
       {
           string _baseProfileName = "";
           if (!_scanner.UserProfiles.Contains(profileName) && profileName != _baseProfileName && userProfile)
           {
               _scanner.UserProfiles.Add(profileName);
           }

           if (!_scanner.ModifiedProfiles.Contains(profileName) && profileName != _baseProfileName)
           {
               _scanner.ModifiedProfiles.Add(profileName);
               //RefreshModifiesProfilesDropDown();
           }
       }

        static void setProfileSetting(string profileName)
       {
           var settingsToStore = new List<KeyValuePair<uint, string>>();
           settingsToStore.Clear();
           settingsToStore.Add(new KeyValuePair<uint, string>((uint)284810369, "0x00000013 (Futuremark System Diagnosis Tool, PhysX Particle Fluid Demo, Adventure Island Online/MapleStory, Phantasy Star Online 2, Wild Tangent Game: Zuma Deluxe)"));
           if (settingsToStore.Count > 0)
           {
               _drs.StoreSettingsToProfile(profileName, settingsToStore);
               AddToModifiedProfiles(profileName);
           }
       }

        static void Main(string[] args)
        {
            string _CurrentProfile = "";
            string CURRENTPROFILE_NOX_PATH = "";
            string[] executeNameList = {};
            string installPath = "";
            const string EXTENSIONFILE = ".exe";
            const string CURRENTPROFILE_NOX = "Nox";
            const string amd64InstallPath = "SOFTWARE\\WOW6432Node\\DuoDianOnline\\SetupInfo";
            const string x86InstallPath = "SOFTWARE\\DuoDianOnline\\SetupInfo";
            const float EPSINON = 0.000001F;
            string CURRENTPROFILE_NOXVMHANDLE = "\\Bignox\\BigNoxVM\\RT\\NoxVMHandle.exe";
            string ProgramFiles = System.Environment.GetEnvironmentVariable("ProgramFiles");
            string CURRENTPROFILE_NOXVMHANDLE_PATH = ProgramFiles + CURRENTPROFILE_NOXVMHANDLE;
            
            _CurrentProfile = CURRENTPROFILE_NOX;

            bool type;
            type = Environment.Is64BitOperatingSystem;

            if (Math.Abs(_import.DriverVersion - 382.64) <= EPSINON)
            {
                //
            }
            if (type)
	        {
                installPath = amd64InstallPath;
	        }
            else
            {
                installPath = x86InstallPath;
            }

            RegistryKey rsg = null;
            rsg = Registry.LocalMachine.OpenSubKey(installPath);
            if (rsg.ValueCount > 0)
            {
                if (rsg.GetValue("InstallPath") != null)
                {
                    CURRENTPROFILE_NOX_PATH = rsg.GetValue("InstallPath").ToString() + "\\bin\\" + CURRENTPROFILE_NOX + EXTENSIONFILE;
                }
            }
           
            if (args != null)
            {
                int argsLength = args.Length;
                if (argsLength > 0)
                {
                    executeNameList = new string[argsLength];
                    for (int i = 0; i < argsLength; i++)
                    {
                        executeNameList[i] = args[i];
                    }                    
                }
                else
                {
                    executeNameList = new string[2];
                    executeNameList[0] = CURRENTPROFILE_NOX + EXTENSIONFILE;
                    executeNameList[1] = CURRENTPROFILE_NOX_PATH;
                }
            }

            bool existProfileName = false;
            var profileNames = _drs.GetProfileNames(ref _CurrentProfile);
            _CurrentProfile = CURRENTPROFILE_NOX;
             if (profileNames.Count > 0)
	        {
                foreach (var profileName in profileNames)
                {
                    if(profileName == _CurrentProfile)
                    {
                        existProfileName = true;
                        var applicationList = _drs.GetApplications(profileName);
                        if (executeNameList != null)
                        {
                            int argsLength = executeNameList.Length;
                            for (int i = 0; i < argsLength; i++)
                            {
                                string exeName = executeNameList[i].ToLowerInvariant().Replace("\\", "/");
                                if (!applicationList.Contains(exeName))
                                {
                                    _drs.AddApplication(profileName, executeNameList[i]); 
                                }
                                
                            }
                            setProfileSetting(_CurrentProfile);
                            break;
                        }
                    }
                }

                if (!existProfileName)
                {
                    _drs.CreateProfile(_CurrentProfile);
                    if (executeNameList != null)
                    {
                        int argsLength = executeNameList.Length;
                        for (int i = 0; i < argsLength; i++)
                        {
                            _drs.AddApplication(_CurrentProfile, executeNameList[i]);
                        }
                        setProfileSetting(_CurrentProfile);
                    }                    
                }
             
            }//end if profileNames.Count
            
        }

      
    }
}
