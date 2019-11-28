using s4pi.Extensions;
using s4pi.Interfaces;
using s4pi.Package;
using s4pi.WrapperDealer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static S4PIDemoFE.MainForm;

namespace S4PIDemoFE.Zjy
{
    class TranslateUtil
    {

        public void onError(string msg) {

        }
        public void ExportFile(IResourceIndexEntry rie,IPackage CurrentPackage, string filename)
        {
            IResource res = WrapperDealer.GetResource(0, CurrentPackage, rie, true); //Don't need wrapper
            Stream s = res.Stream;
            s.Position = 0;
            if (s.Length != rie.Memsize)
            {
                //CopyableMessageBox.Show(string.Format("Resource stream has {0} bytes; index entry says {1}.",
                //    s.Length,
                //    rie.Memsize));
                onError(string.Format("Resource stream has {0} bytes; index entry says {1}.",
                    s.Length,
                    rie.Memsize));
            }
            BinaryWriter w = new BinaryWriter(new FileStream(filename, FileMode.Create));
            w.Write((new BinaryReader(s)).ReadBytes((int)s.Length));
            w.Close();
        }

        private enum DuplicateHandling
        {
            /// <summary>
            /// Refuse to create the request resource
            /// </summary>
            Reject,

            /// <summary>
            /// Delete any conflicting resource
            /// </summary>
            Replace,

            /// <summary>
            /// Ignore any conflicting resource
            /// </summary>
            Allow
        }
        private enum AutoSaveState
        {
            Never,
            Ask,
            Always
        }


        IPackage CurrentPackage;
        private IResourceIndexEntry NewResource(IResourceKey rk, MemoryStream ms, DuplicateHandling dups, bool compress)
        {
           
            IResourceIndexEntry rie = this.CurrentPackage.Find(rk.Equals);
            if (rie != null)
            {
                if (dups == DuplicateHandling.Reject)
                {
                    return null;
                }
                if (dups == DuplicateHandling.Replace)
                {
                    this.CurrentPackage.DeleteResource(rie);
                }
            }

            rie = this.CurrentPackage.AddResource(rk, ms, false); //we do NOT want to search again...
            if (rie == null)
            {
                return null;
            }

            rie.Compressed = (ushort)(compress ? 0x5A42 : 0);

            //this.IsPackageDirty = true;

            return rie;
        }

        private void ImportPackagesCommon(string[] packageList,
                                          string title,
                                          DuplicateHandling dups,
                                          bool compress,
                                          bool useNames = false,
                                          bool rename = false,
                                          List<uint> dupsList = null,
                                          AutoSaveState autoSaveState = AutoSaveState.Ask,
                                          IList<IResourceIndexEntry> selection = null
            )
        {
            //bool cpUseNames = this.controlPanel1.UseNames;
            bool cpUseNames = false;
            DateTime now = DateTime.UtcNow;

            bool autoSave = false;
            //if (autoSaveState == AutoSaveState.Ask)
            //{
            //    switch (CopyableMessageBox.Show("Auto-save current package after each package imported?",
            //        title,
            //        CopyableMessageBoxButtons.YesNoCancel,
            //        CopyableMessageBoxIcon.Question))
            //    {
            //        case 0:
            //            autoSave = true;
            //            break;
            //        case 2:
            //            return;
            //    }
            //}
            //else
            //{
            //    autoSave = autoSaveState == AutoSaveState.Always;
            //}

            try
            {
                //this.browserWidget1.Visible = false;
                //this.controlPanel1.UseNames = false;

                bool skipAll = false;
                foreach (string filename in packageList)
                {
                    //if (!string.IsNullOrEmpty(this.Filename)
                    //    && Path.GetFullPath(this.Filename).Equals(Path.GetFullPath(filename)))
                    //{
                    //    CopyableMessageBox.Show("Skipping current package.", this.importPackagesDialog.Title);
                    //    continue;
                    //}

                    //this.lbProgress.Text = "Importing " + Path.GetFileNameWithoutExtension(filename) + "...";
                    //Application.DoEvents();
                    IPackage imppkg=null;
                    try
                    {
                        imppkg = Package.OpenPackage(0, filename);
                    }
                    catch (InvalidDataException ex)
                    {
                        //if (skipAll)
                        //{
                        //    continue;
                        //}
                        //int btn =
                        //    CopyableMessageBox.Show(
                        //        string.Format("Could not open package {0}.\n{1}", Path.GetFileName(filename), ex.Message),
                        //        title,
                        //        CopyableMessageBoxIcon.Error,
                        //        new List<string>(new[] { "Skip this", "Skip all", "Abort" }),
                        //        0,
                        //        0);
                        //if (btn == 0)
                        //{
                        //    continue;
                        //}
                        //if (btn == 1)
                        //{
                        //    skipAll = true;
                        //    continue;
                        //}
                        //break;
                    }
                    try
                    {
                        List<Tuple<MyDataFormat, DuplicateHandling>> limp =
                            new List<Tuple<MyDataFormat, DuplicateHandling>>();
                        List<IResourceIndexEntry> lrie = selection == null
                            ? imppkg.GetResourceList
                            : imppkg.FindAll(rie => selection.Any(tgt => ((AResourceKey)tgt).Equals(rie)));
                        //this.progressBar1.Value = 0;
                        //this.progressBar1.Maximum = lrie.Count;


                        foreach (IResourceIndexEntry rie in lrie)
                        {
                            try
                            {
                                if (rie.ResourceType == 0x0166038C) //NMAP
                                {
                                    //if (useNames)
                                    //{
                                    //    this.browserWidget1.MergeNamemap(
                                    //        WrapperDealer.GetResource(0, imppkg, rie) as IDictionary<ulong, string>,
                                    //        true,
                                    //        rename);
                                    //}
                                }
                                else
                                {
                                    IResource res = WrapperDealer.GetResource(0, imppkg, rie, true);

                                    MyDataFormat impres = new MyDataFormat()
                                    {
                                        tgin = rie as AResourceIndexEntry,
                                        data = res.AsBytes
                                    };

                                    // dups Replace | Reject | Allow
                                    // dupsList null | list of allowable dup types
                                    DuplicateHandling dupThis =
                                        dups == DuplicateHandling.Allow
                                            ? dupsList == null || dupsList.Contains(rie.ResourceType)
                                                ? DuplicateHandling.Allow
                                                : DuplicateHandling.Replace
                                            : dups;

                                    limp.Add(Tuple.Create(impres, dupThis));
                                    //this.progressBar1.Value++;
                                    if (now.AddMilliseconds(100) < DateTime.UtcNow)
                                    {
                                        //Application.DoEvents();
                                        now = DateTime.UtcNow;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                string rk = rie.ToString() ?? "could not be read";

                                string message = string.Format("Could not import all resources. Error occured on package file '{0}', resource key '{1}'. Aborting.\n", filename, rk);
                                //CopyableMessageBox.IssueException(ex, message, title);
                                throw new FileLoadException(message, ex);
                            }
                        }
                        //this.progressBar1.Value = 0;

                        IEnumerable<IResourceIndexEntry> rieList = limp
                            .Select(
                                x =>
                                    this.NewResource((AResourceKey)x.Item1.tgin,
                                        new MemoryStream(x.Item1.data),
                                        x.Item2,
                                        compress))
                            .Where(x => x != null);
                        //this.browserWidget1.AddRange(rieList);
                    }
                    catch (FileLoadException e)
                    {
                        //just the thrown exception, stop looping
                        break;
                    }
                    catch (Exception ex)
                    {
                        //CopyableMessageBox.IssueException(ex, "Could not import all resources - aborting.\n", title);
                        break;
                    }
                    finally
                    {
                        imppkg.Dispose();
                    }
                    //if (autoSave && !this.FileSave())
                    //{
                    //    break;
                    //}
                }
            }
            finally
            {
                //this.lbProgress.Text = "";
                //this.progressBar1.Value = 0;
                //this.progressBar1.Maximum = 0;
                //this.controlPanel1.UseNames = cpUseNames;
                //this.browserWidget1.Visible = true;
                //ForceFocus.Focus(Application.OpenForms[0]);
                //Application.DoEvents();
            }
        }

        private bool ImportFile(string filename,
                              TGIN tgin,
                              bool useName,
                              bool rename,
                              bool compress,
                              DuplicateHandling dups,
                              bool select)
        {
            IResourceKey rk = (TGIBlock)tgin;
            string resName = tgin.ResName;
            bool nmOK = true;
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            BinaryReader r = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
            w.Write(r.ReadBytes((int)r.BaseStream.Length));
            r.Close();
            w.Flush();

            if (useName && !string.IsNullOrEmpty(resName))
            {
                //nmOK = this.browserWidget1.ResourceName(rk.Instance, resName, true, rename);
            }

            IResourceIndexEntry rie = this.NewResource(rk, ms, dups, compress);
            if (rie != null)
            {
                //this.browserWidget1.Add(rie, select);
            }
            CurrentPackage.SavePackage();
            return nmOK;
        }
    }
}
