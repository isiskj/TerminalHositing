using System;
using System.Collections.ObjectModel;
using System.IO;
using TerminalHositing.Models;

namespace TerminalHositing.ViewModels
{
    public class MainViewModel
    {
        //public ObservableCollection<DirectoryItem> Drives { get; set; }
        public MainViewModel() 
        { 
            //Drives = new ObservableCollection<DirectoryItem>();
            //LoadDrives();
        }

        //private void LoadDrives()
        //{
        //    foreach (var drive in DriveInfo.GetDrives())
        //    {
        //        if(drive.IsReady)
        //        {
        //            var driveItem = new DirectoryItem { Name = drive.Name };
        //            //LoadDirectories(drive.RootDirectory, driveItem);
        //            Drives.Add(driveItem);
        //        }
        //    }
        //}

        //private void LoadDirectories(DirectoryInfo directoryInfo, DirectoryItem parentItem)
        //{
        //    try
        //    {
        //        foreach (var dir in directoryInfo.EnumerateDirectories())
        //        {
        //            var dirItem = new DirectoryItem { Name = dir.Name };
        //            parentItem.SubDirectories.Add(dirItem);
        //            LoadDirectories(dir, dirItem);
        //        }
        //    }
        //    catch(UnauthorizedAccessException)
        //    {

        //    }
        //}
    }
}
