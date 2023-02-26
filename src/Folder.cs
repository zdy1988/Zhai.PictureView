using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Zhai.PictureView
{
    internal class Folder : ObservableCollection<Picture>
    {
        public DirectoryInfo Current { get; }

        public List<DirectoryInfo> Borthers { get; private set; }

        public bool IsAccessed => CanAccess(Current);

        public Folder(DirectoryInfo dir, List<DirectoryInfo> borthers = null)
        {
            if (!dir.Exists)
                throw new DirectoryNotFoundException();

            Current = dir;

            if (borthers != null && Borthers != borthers)
            {
                Borthers = borthers;
            }
        }


        public async Task LoadAsync()
        {
            var files = Current.EnumerateFiles().Where(PictureSupport.PictureSupportExpression).OrderBy(t=>t.Name);

            if (files.Any())
            {
                await Task.Run(() =>
                {
                    foreach (var file in files)
                    {
                        Application.Current.Dispatcher.Invoke(() => Add(new Picture(file.FullName)));
                    };

                });

                if (Borthers == null)
                {
                    LoadBorthers();
                }
            }
        }

        public void LoadBorthers()
        {
            if (Current.Parent != null && CanAccess(Current.Parent))
            {
                Borthers = Current.Parent.EnumerateDirectories().Where(dir =>
                {
                    var isSecurity = CanAccess(dir);

                    if (isSecurity)
                    {
                        return dir.EnumerateFiles().Where(PictureSupport.PictureSupportExpression).Any();
                    }

                    return false;

                }).OrderBy(t => t.Name).ToList();

                BorthersLoaded?.Invoke(this, Borthers);
            }
        }

        private bool CanAccess(DirectoryInfo dir = null)
        {
            return !dir.GetAccessControl(AccessControlSections.Access).AreAccessRulesProtected;



            //var currentUserIdentity = Path.Combine(Environment.UserDomainName, Environment.UserName);

            //DirectorySecurity directorySecurity = dir.GetAccessControl();

            //var userAccessRules = directorySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)).OfType<FileSystemAccessRule>().Where(i => i.IdentityReference.Value == currentUserIdentity).ToList();

            //return userAccessRules.Any(i => i.AccessControlType == AccessControlType.Deny);
        }

        public bool GetNext(out DirectoryInfo dir)
        {
            dir = Current;

            if (Borthers == null || Borthers.Count == 1)
                return false;

            var item = Borthers.Find(t => t.FullName == Current.FullName);

            var index = Borthers.IndexOf(item);

            index += 1;

            if (index > Borthers.Count - 1)
            {
                index = 0;
            }

            dir = Borthers[index];

            return true;
        }

        public bool GetPrev(out DirectoryInfo dir)
        {
            dir = Current;

            if (Borthers == null || Borthers.Count == 1)
                return false;

            var item = Borthers.Find(t => t.FullName == Current.FullName);

            var index = Borthers.IndexOf(item);

            index -= 1;

            if (index < 0)
            {
                index = Borthers.Count - 1;
            }

            dir = Borthers[index];

            return true;
        }

        public void Cleanup()
        {
            if (this.Any())
            {
                foreach (var item in this)
                {
                    item.Cleanup();
                }

                this.Clear();
            }
        }

        #region Events

        public event EventHandler<List<DirectoryInfo>> BorthersLoaded;

        #endregion

    }
}
