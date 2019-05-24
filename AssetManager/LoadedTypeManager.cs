using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
namespace System.Reflection.BASeCamp
{
    public static class StringExtensions
{
    /// <summary>
    /// this string is the regular expression, sregex is the string to test.
    /// </summary>
    /// <param name="test">Regular Expression</param>
    /// <param name="sregex">String to match</param>
    /// <returns></returns>
    public static bool TestRegex(this string test, String sregex)
    {
        // System.Text.RegularExpressions.Regex re = new Regex(test,RegexOptions.IgnoreCase);
        return System.Text.RegularExpressions.Regex.Match(test, sregex, RegexOptions.IgnoreCase).Success;
    }
}

/// defines some basic routines that can be hooked to display, for example, progress, what files are being loaded, etc.
/// used by the splash screen.
/// </summary>
public interface iManagerCallback
{
    void ShowMessage(String message);
    void UpdateProgress(float ProgressPercentage);
    void FlagError(String ErrorDescription, Exception AttachedException);
}

internal class Nullcallback : iManagerCallback
{
    #region iManagerCallback Members

    public void ShowMessage(string message)
    {
        //don't care...
    }

    public void UpdateProgress(float ProgressPercentage)
    {
    }

    public void FlagError(String ErrorDescription, Exception AttachedException)
    {
    }

    #endregion
}


#if !iManagerCallback

#endif


    /// <summary>
    /// VersionInfo structure. Designed to help ease parsing and interpretation of version numbers.
    /// 
    /// </summary>
    [Serializable]
    public struct VersionInfo : IComparable<VersionInfo>, IComparable<System.Version>, ISerializable
    {
        public readonly int BuildNumber;
        public readonly int Major;
        public readonly int Minor;
        public readonly int Revision;

        /// <summary>
        /// Initialize this VersionInfo with a given string source.
        /// </summary>
        /// <param name="pSource"></param>
        public VersionInfo(String pSource)
            : this(pSource, new char[] {'.'})
        {
        }

        /// <summary>
        /// Initialize this VersionInfo structure with the given fields.
        /// </summary>
        /// <param name="pMajor">Major Version Number</param>
        /// <param name="pMinor">Minor Version Number</param>
        /// <param name="pRevision">Revision</param>
        /// <param name="pBuildNumber">Build Number</param>
        public VersionInfo(int pMajor, int pMinor, int pRevision, int pBuildNumber)
        {
            Major = pMajor;
            Minor = pMinor;
            Revision = pRevision;
            BuildNumber = pBuildNumber;
        }

        /// <summary>
        /// Read This VersionInfo from a Serialization Stream.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="Context"></param>
        public VersionInfo(SerializationInfo info, StreamingContext Context)
        {
            Major = info.GetInt32("Major");
            Minor = info.GetInt32("Minor");
            Revision = info.GetInt32("Revision");
            BuildNumber = info.GetInt32("BuildNumber");
        }

        public VersionInfo(String pSource, char[] splitchars)
        {
            String[] splitresult = pSource.Split(splitchars, 4);
            int[] Values = new int[4];
            for (int i = 0; i < splitresult.Length; i++)
            {
                int gotresult = 0;
                if (!int.TryParse(splitresult[i], out gotresult))
                {
                    gotresult = AsciiValue(splitresult[i]);
                }

                Values[i] = gotresult;
            }

            this.Major = Values[0];
            this.Minor = Values[1];
            this.Revision = Values[2];
            this.BuildNumber = Values[3];
        }

        public VersionInfo(Version v)
        {
            Major = v.Major;
            Minor = v.Minor;
            Revision = v.Revision;
            BuildNumber = v.Build;
        }


        public int CompareTo(Version other)
        {
            return CompareTo(new VersionInfo(other));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Major", Major);
            info.AddValue("Minor", Minor);
            info.AddValue("Revision", Revision);
            info.AddValue("BuildNumber", BuildNumber);
        }

        public static implicit operator VersionInfo(Version sourceobj)
        {
            return new VersionInfo(sourceobj);
        }

        private static int AsciiValue(String ofString)
        {
            int gotresult = 0;
            foreach (var iterate in ofString)
            {
                gotresult <<= iterate;
            }

            return gotresult;
        }

        public static VersionInfo GetApplicationVersion()
        {
            Version ourappver = Assembly.GetCallingAssembly().GetName().Version;
            int mMajor = ourappver.Major;
            int mMinor = ourappver.Minor;
            int mRevision = ourappver.Revision;
            int mBuildNumber = ourappver.Build;
            return new VersionInfo(mMajor, mMinor, mRevision, mBuildNumber);
        }

        public override string ToString()
        {
            return Major + "." + Minor + "." + Revision + "." + BuildNumber;
        }


        public override bool Equals(object obj)
        {
            if (obj is VersionInfo)
            {
                return CompareTo((VersionInfo) obj) == 0;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Major >> Minor >> Revision >> BuildNumber;
        }

        #region IComparable<VersionInfo> Members

        public int CompareTo(VersionInfo other)
        {
            int majorcompare = this.Major.CompareTo(other.Major);
            if (majorcompare != 0) return majorcompare;
            int minorcompare = this.Minor.CompareTo(other.Minor);
            if (minorcompare != 0) return minorcompare;
            int revisioncompare = this.Revision.CompareTo(other.Revision);
            if (revisioncompare != 0) return revisioncompare;
            int Buildcompare = this.BuildNumber.CompareTo(other.BuildNumber);
            return Buildcompare;
        }

        public static bool operator ==(VersionInfo v1, VersionInfo v2)
        {
            return v1.CompareTo(v2) == 0;
        }

        public static bool operator !=(VersionInfo v1, VersionInfo v2)
        {
            return v1.CompareTo(v2) != 0;
        }

        public static bool operator >(VersionInfo v1, VersionInfo v2)
        {
            return v1.CompareTo(v2) > 0;
        }

        public static bool operator <(VersionInfo v1, VersionInfo v2)
        {
            return v1.CompareTo(v2) < 0;
        }

        public static bool operator >=(VersionInfo v1, VersionInfo v2)
        {
            return v1.CompareTo(v2) >= 0;
        }

        public static bool operator <=(VersionInfo v1, VersionInfo v2)
        {
            return v1.CompareTo(v2) <= 0;
        }

        #endregion
    }

    public class RequiredVersionAttribute : Attribute
    {
        private String _VersionOf;
        private VersionInfo vi;

        /// <summary>
        /// Constructor accepting a single VersionInfo structure.
        /// </summary>
        /// <param name="pvi">Version of main assembly required.</param>
        public RequiredVersionAttribute(VersionInfo pvi)
            : this(pvi, Assembly.GetExecutingAssembly().GetName().FullName)
        {
        }

        /// <summary>
        /// Constructor accepting a VersionInfo structure and a string identifier of the assembly whose
        /// version must be greater than or equal to that specified.
        /// </summary>
        /// <param name="pvi"></param>
        /// <param name="pVersionof"></param>
        public RequiredVersionAttribute(VersionInfo pvi, String pVersionof)
        {
            _VersionOf = pVersionof;
            vi = pvi;
        }

        //called by appropriate implementation code to test RequiredVersionAttribute.
        //makes sure that the given component is the required version or later.
        public bool SupportsVersion()
        {
            //first, get the Assembly.
            Assembly testassembly = GetAssemblyFromName(_VersionOf);
            if (testassembly == null)
            {
                throw new ArgumentNullException("Failed to resolve assembly named " + _VersionOf);
            }

            VersionInfo actualver = testassembly.GetName().Version;
            if (actualver >= vi)
                return true;
            else
                return false;
        }

        //task: loop through all custom attributes on the type.
        //for the RequiredVersionAttribute's, call SupportsVersion, plopping exceptions to Trace.
        //if all tests come back true, return true. if all tests come back false, return false.
        //if a test fails, return false.
        public static bool TestComponent(Type typecheck)
        {
            foreach (var iterate in typecheck.GetCustomAttributes(typeof(RequiredVersionAttribute), true))
            {
                RequiredVersionAttribute rva = iterate as RequiredVersionAttribute;
                try
                {
                    if (!rva.SupportsVersion())
                        return false;
                }
                catch (ArgumentNullException exx)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// retrieves the Assembly for the given name, by iterating all Assemblies in the given Domain.
        /// </summary>
        /// <param name="pName">Name to search for.</param>
        /// <param name="domain">domain to look in</param>
        /// <returns></returns>
        private static Assembly GetAssemblyFromName(String pName, AppDomain pDomain)
        {
            return pDomain.GetAssemblies().FirstOrDefault
            (
                iterate => iterate.GetName().Name.Equals(pName, StringComparison.OrdinalIgnoreCase));
        }

        private static Assembly GetAssemblyFromName(String pName)
        {
            return GetAssemblyFromName(pName, AppDomain.CurrentDomain);
        }
    }


    /// <summary>
    /// Indicates that a component requires a given version or later of a given component.
    /// </summary>
    //I've noticed something. Something bad. When BaseBlock starts up, it  takes way to damned long for it to start. Why? because the LoadedTypeManagers()
    //are currently being instantiated one by one. The bad thing is that they all end up opening the same assemblies and inspecting the same files, so, Instead of doing it that way
    //I am going to change it so that the LoadedTypeManagers are instead dependent on this class.
    //these two classes really would make good material for a blog post or article or something...
    /// <summary>
    /// MultiTypeManager: used to load a group of LoadedTypeManager's simultaneously (that is, MultiTypeManager does the assembly looping and
    /// then calls into each LoadedTypeManager() to see if it is "allowed" to add it...
    /// </summary>
    public class MultiTypeManager
    {
        public delegate bool AssemblyLoadTestFunction(Assembly testassembly);

        public delegate void TypeManagerLoadProgressCallback(float pprogress);

        private LoadedTypeManager.TypeManagerInspectTypeCallback inspectioncallback = null;
        public Dictionary<Type, LoadedTypeManager> loadeddata;
        private Assembly[] useassemblies;

        public MultiTypeManager(String[] lookfolders, Type[] typesload,
            iManagerCallback pcallback, AssemblyLoadTestFunction TestAssembly,
            TypeManagerLoadProgressCallback pp, Assembly[] preloadedassemblies)
            : this(AssembliesFromStrings(lookfolders), typesload, pcallback, TestAssembly, pp, preloadedassemblies)
        {
        }

        public MultiTypeManager(IEnumerable<Assembly> lookassemblies, IEnumerable<Type> typesload,
            iManagerCallback pcallback, AssemblyLoadTestFunction TestAssembly,
            TypeManagerLoadProgressCallback pprog, IEnumerable<Assembly> preloadedassemblies)
            : this(lookassemblies, typesload, pcallback, TestAssembly, pprog, null, preloadedassemblies)
        {
        }

        public MultiTypeManager(IEnumerable<Assembly> lookassemblies, IEnumerable<Type> typesload,
            iManagerCallback pcallback, AssemblyLoadTestFunction TestAssembly,
            TypeManagerLoadProgressCallback pprog,
            LoadedTypeManager.TypeManagerInspectTypeCallback pinspectioncallback,
            IEnumerable<Assembly> preloadedassemblies)
        {
            lookassemblies = LoadedTypeManager.RemoveDuplicates(lookassemblies, pcallback).AsParallel();
            //each Type corresponds to a new LoadedTypeManager to load for that type.
            //LoadedTypeManager[] ltm = new LoadedTypeManager[typesload.Length];
            if (TestAssembly == null) TestAssembly = ass => true;
            //create the Dictionary first...
            inspectioncallback = pinspectioncallback;
            List<Assembly> buildcheck = new List<Assembly>();
            if (preloadedassemblies != null)
            {
                foreach (var addassembly in preloadedassemblies)
                {
                    if (!buildcheck.Contains(addassembly))
                        buildcheck.Add(addassembly);
                }
            }

            if (lookassemblies != null)
            {
                foreach (var addassembly in lookassemblies)
                {
                    if (!buildcheck.Contains(addassembly))
                        buildcheck.Add(addassembly);
                }
            }


            Assembly[] checkassemblies = buildcheck.ToArray();
            useassemblies = checkassemblies;
            loadeddata = new Dictionary<Type, LoadedTypeManager>();
            foreach (Type looptype in typesload)
            {
                // LoadedTypeManager addtm = new LoadedTypeManager(checkassemblies,looptype, IgnoreAssemblies,pcallback);
                LoadedTypeManager addtm = new LoadedTypeManager(looptype);
                addtm.inspectioncallback = inspectioncallback;
                loadeddata.Add(looptype, addtm);
            }


            //now that we have the LoadedTypeManager objects, uninitialized- we can iterate through each assembly and the types
            //in each assembly for matches.
            //assemblyprogressincrement: the amount of progress we will go through after checking each assembly.
            float assemblyprogressincrement = 1f / (float) checkassemblies.Count();
            float currprogress = 0;

            foreach (Assembly loopassembly in checkassemblies)
            {
                if (loopassembly.FullName.Contains("Dragon"))
                {
                    Debug.Print("dragon");
                }

                if (loopassembly == Assembly.GetExecutingAssembly())
                {
                    Debug.Print("test");
                }

                //
                if (TestAssembly(loopassembly))
                {
                    pcallback.ShowMessage("Inspecting Assembly:" + loopassembly.GetName().Name);
                    //iterate through each type...
                    currprogress += assemblyprogressincrement;
                    Type[] typesiterate;
                    try
                    {
                        typesiterate = loopassembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException rtle)
                    {
                        pcallback.ShowMessage
                        ("ReflectionTypeLoadException occured;" + rtle.StackTrace +
                         " InnerExceptions:");
                        foreach (Exception loopexception in rtle.LoaderExceptions)
                        {
                            pcallback.ShowMessage
                            ("RTLE Loader Exception:" + loopexception.Message + " Source:" +
                             loopexception.Source + "stack Trace:" + loopexception.StackTrace);
                        }

                        pcallback.ShowMessage("End of RTLE Loader Exceptions");
                        currprogress += assemblyprogressincrement;
                        pprog(currprogress);
                        typesiterate = null;
                    }

                    if (typesiterate != null)
                    {
                        //iterate through each type...

                        //get appropriate percent per type...
                        // float percentpertype = assemblyprogressincrement / typesiterate.Length;

                        foreach (Type looptype in typesiterate)
                        {
                            //currprogress+=percentpertype;

                            //if this is a class type...


                            // pcallback.ShowMessage("Checking type:" + looptype.FullName);
                            //And... for each type, iterate through all the LoadedTypeManagers in our dictionary...
                            foreach (var checkmanager in loadeddata)
                            {
                                if (LoadedTypeManager.CheckType(looptype, checkmanager.Key, pcallback))
                                {
                                    pcallback.ShowMessage
                                    ("Type:" + looptype.FullName + " is a, or implements, " +
                                     checkmanager.Key.Name);

                                    //add it to that manager...
                                    if (inspectioncallback != null) inspectioncallback(looptype);
                                    if (looptype != null) checkmanager.Value.ManagedTypes.Add(looptype);
                                }
                            }
                        }
                    }
                }
                else
                {
                    currprogress += assemblyprogressincrement;
                    pprog(currprogress);
                    pcallback.ShowMessage("Skipped Assembly " + loopassembly.FullName);
                }
            }

            //at the conclusion of the loop, show a summary.
            if (!(pcallback is Nullcallback))
            {
                pcallback.ShowMessage
                ("Assembly enumeration complete.(" + checkassemblies.Count().ToString() +
                 " Assemblies ");
                //^save time by not doing this for a Nullcallback...
                foreach (var loopltm in loadeddata)
                {
                    pcallback.ShowMessage
                    (" found " + loopltm.Value.ManagedTypes.Count + " instances of type " +
                     loopltm.Key.Name);
                }
            }
        }

        public LoadedTypeManager this[Type index]
        {
            get
            {
                if (!loadeddata.ContainsKey(index))
                {
                    //add it...
                    loadeddata.Add(index, new LoadedTypeManager(useassemblies, index, null));
                    //throw new ApplicationException("Type " + index.FullName + " not enumerated...");
                }

                return loadeddata[index];
            }
        }


        public static IEnumerable<Assembly> AssembliesFromStrings(String[] foldernames)
        {
            return AssembliesFromStrings(foldernames, new Nullcallback());
        }

        public static IEnumerable<Assembly> AssembliesFromStrings(IEnumerable<String> foldernames, iManagerCallback datahook)
        {
            yield return Assembly.GetExecutingAssembly();
            foreach (String loopfolder in foldernames)
            {
                if (Directory.Exists(loopfolder))
                {
                    foreach (
                        FileInfo loopfile in
                        new DirectoryInfo(loopfolder).GetFiles("*.dll", SearchOption.TopDirectoryOnly))
                    {
                        Assembly testassembly = null;
                        try
                        {
                            if (loopfile.FullName.EndsWith("DragonAdapter.dll", StringComparison.OrdinalIgnoreCase))
                            {
                                Debug.Print("Dragon");
                            }

                            testassembly = Assembly.LoadFile(loopfile.FullName);
                        }
                        catch (Exception err)
                        {
                            //Debug.Print("failed to load assembly:" + loopfile.FullName + " " + err.Message);
                            datahook.ShowMessage("failed to load assembly:" + loopfile.FullName + " " + err.Message);
                        }

                        if (testassembly != null)
                            yield return testassembly;
                    }
                }
            }
        }
    }

    /// <summary>
    /// extension method for levelbuilder classes.
    /// </summary>
    ///
    /// <summary>
    /// Manages a collection of iLevelSetBuilder implementing types, which are acquired by loading all the assemblies in a given folder and
    /// finding all Types in that assembly that implement iLevelSetBuilder.
    /// </summary>
    public class LoadedTypeManager
    {
        public delegate void TypeManagerInspectTypeCallback(Type inspectingType);

        /// <summary>
        /// the Type that we are looking in assemblies for.
        /// </summary>
        private readonly Type TypeManage;

        private List<Type> _ManagedTypes = new List<Type>();

        public TypeManagerInspectTypeCallback inspectioncallback = null;

        //called for each type.
        private iManagerCallback mcallback = new Nullcallback();

        public LoadedTypeManager(String lookfolder, Type lookfor, iManagerCallback pcallback)
            : this(new string[] {lookfolder}, lookfor, pcallback)
        {
        }

        /// <summary>
        /// Internal constructor used to create a LoadedTypeManager that has no actual state. This is used by the MultiTypeManager class
        /// to create "empty" TypeManager instances that it can then add to.
        /// </summary>
        /// <param name="ptypemanage"></param>
        /// <param name="pcallback"></param>
        internal LoadedTypeManager(Type ptypemanage, iManagerCallback pcallback)
        {
            TypeManage = ptypemanage;
            mcallback = pcallback;
        }

        internal LoadedTypeManager(Type ptypemanage)
            : this(ptypemanage, new Nullcallback())
        {
        }

        /*
            public Dictionary<Type, IHighScoreList> GetScoreDictionary()
            {
                return directScores;




            }
        */
        /*
        private void LoadTypeScores(Type[] typedata,iManagerCallback mcallback)
        {
            String appfolder=  BCBlockGameState.AppDataFolder;
            String hiscorefile = Path.Combine(appfolder,"Hiscores.dat");
            //open the file.
            
            if(File.Exists(hiscorefile))
            {
                mcallback.ShowMessage("Attempting to load scores from file, " + hiscorefile);
                //TODO: handle IOException error when file can't be opened for some reason.
                FileStream fread = new FileStream(hiscorefile, FileMode.Open);
                BinaryFormatter xformatter = new BinaryFormatter();
                ScoreData = (Dictionary<String, IHighScoreList>) xformatter.Deserialize(fread);
                fread.Close();
                //create our "typed" Dictionary.
                mcallback.ShowMessage("loaded " + directScores.Count + " Lists.");
                directScores = new Dictionary<Type, IHighScoreList>();
                foreach (String keyloop in ScoreData.Keys)
                {
                    Type madetype = Type.GetType(keyloop);
                    if (madetype != null)
                    {
                        directScores.Add(madetype, ScoreData[keyloop]);


                    }




                }


            }
            else
            {
                mcallback.ShowMessage("Hiscore file, " + hiscorefile + " not found. Creating empty score list.");
                //create a new one.
                ScoreData = new Dictionary<string, IHighScoreList>();
                directScores = new Dictionary<Type, IHighScoreList>();
                foreach (Type looptype in typedata)
                {
                    if(looptype.GetInterfaces().Contains(typeof(iLevelSetBuilder)))
                    {
                        mcallback.ShowMessage("Creating scorelist for type, " + looptype.FullName);
                        IHighScoreList createdscore = new LocalHighScores(20);
                        ScoreData.Add(looptype.FullName, createdscore);
                        directScores.Add(looptype, createdscore);
                    }

                }




            }


        }
        */

        public LoadedTypeManager(Assembly[] lookassemblies, Type lookfor, iManagerCallback pcallback)
            : this(lookassemblies, lookfor, null, pcallback)
        {
        }

        internal LoadedTypeManager(Type pTypeManage, Type[] pmanagedtypes)
            : this(pTypeManage, pmanagedtypes, new Nullcallback())
        {
        }

        /// <summary>
        /// Directly initializes a LoadedTypeManager instance with a specified list of managed types and the Type being managed.
        /// </summary>
        /// <param name="pTypeManage"></param>
        /// <param name="pmanagedtypes"></param>
        /// <param name="pcallback"></param>
        internal LoadedTypeManager(Type pTypeManage, IEnumerable<Type> pmanagedtypes, iManagerCallback pcallback)
        {
            TypeManage = pTypeManage;
            ManagedTypes = pmanagedtypes.ToList();
            mcallback = pcallback;
        }

        public LoadedTypeManager(Assembly[] lookassemblies, Type lookfor, IEnumerable<string> ignoreassemblynames,
            iManagerCallback pcallback)
            : this(lookassemblies, lookfor, ignoreassemblynames, pcallback, null)
        {
        }

        public LoadedTypeManager(Assembly[] lookassemblies, Type lookfor, IEnumerable<string> ignoreassemblynames,
            iManagerCallback pcallback, TypeManagerInspectTypeCallback pinspectioncallback)
        {
            TypeManage = lookfor;
            this.inspectioncallback = pinspectioncallback;
            LoadTypes(lookassemblies, ignoreassemblynames);
            //LoadTypeScores(ManagedTypes.ToArray(), mcallback);
        }

        public LoadedTypeManager(string[] slookfolders, Type lookfor, iManagerCallback pcallback)
            : this(slookfolders, lookfor, new String[] { }, pcallback)
        {
        }

        public LoadedTypeManager(string[] slookfolders, Type lookfor, IEnumerable<string> ignoreassemblynames,
            iManagerCallback pcallback)
        {
            //alright, iterate through all the folders..
            mcallback = pcallback;
            TypeManage = lookfor;


            DirectoryInfo[] lookfolders = new DirectoryInfo[slookfolders.Length];
            for (int i = 0; i < slookfolders.Length; i++)
            {
                lookfolders[i] = new DirectoryInfo(slookfolders[i]);
            }

            List<Assembly> AssemblyList = new List<Assembly>();

            foreach (DirectoryInfo loopfolder in lookfolders)
            {
                //now iterate through all the DLL files in that folder.
                if (loopfolder.Exists)
                {
                    foreach (FileInfo dllfile in loopfolder.GetFiles("*.dll", SearchOption.TopDirectoryOnly))
                    {
                        Assembly LoadAssembly = null;
                        //attempt to load this as an assembly...
                        try
                        {
                            LoadAssembly = Assembly.LoadFile(dllfile.FullName);
                            mcallback.ShowMessage("Loaded Assembly:" + dllfile.Name);
                        }
                        catch (Exception error)
                        {
                            mcallback.ShowMessage("Load of " + dllfile.Name + " failed- " + error.Message);
                        }

                        if (LoadAssembly != null)
                        {
                            if (
                                !ignoreassemblynames.Any<String>
                                (
                                    (y) => y.Equals(LoadAssembly.GetName().Name, StringComparison.OrdinalIgnoreCase)))
                                AssemblyList.Add(LoadAssembly);
                        }
                    }
                }
            }

            mcallback.ShowMessage("Found " + AssemblyList.Count.ToString() + " Assemblies.");
            AssemblyList.Add(Assembly.GetExecutingAssembly()); //add this assembly, so it can find the default builder.
            LoadTypes(AssemblyList.ToArray());
            //LoadTypeScores(ManagedTypes.ToArray(), mcallback);
        }

        public List<Type> ManagedTypes
        {
            get { return RemoveNulls(_ManagedTypes); }
            internal set { _ManagedTypes = value; }
        }

        public static List<Type> GetTypesFromAssembly(Assembly lookassembly, Type derivedFrom)
        {
            LoadedTypeManager ltm = new LoadedTypeManager(new Assembly[] {lookassembly}, derivedFrom, new Nullcallback());
            ltm.LoadTypes(new Assembly[] {lookassembly});
            return ltm.ManagedTypes;
        }


        public List<Type> RemoveNulls(List<Type> removefrom)
        {
            removefrom.RemoveAll((a) => a == null);
            return removefrom;
        }

        ~LoadedTypeManager()
        {
            //save the highscores set
            /*
            Debug.Print("LoadedTypeManager destructor saving highscores....");
            String appfolder = BCBlockGameState.AppDataFolder;
            String Hiscorefile = Path.Combine(appfolder,"Hiscores.dat");
            Debug.Print("Saving to " + Hiscorefile);
  
                //Open the file...
                FileStream fwrite = new FileStream(Hiscorefile, FileMode.Create);
                BinaryFormatter xformatter = new BinaryFormatter();
                xformatter.Serialize(fwrite, ScoreData);
                //close
                fwrite.Close();
                fwrite.Dispose();
            */
        }


        internal static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> removefrom, iManagerCallback mcallback)
        {
            Dictionary<T, T> Dictcheck = new Dictionary<T, T>();


            foreach (T item in removefrom)
            {
                if (item != null)
                {
                    if (!Dictcheck.ContainsKey(item))
                    {
                        Dictcheck.Add(item, item);
                        yield return item;
                    }
                    else
                    {
                        mcallback.ShowMessage("removing duplicate entry for " + item.ToString());
                    }
                }
            }
        }

        private void LoadTypes(Assembly[] useassemblies)
        {
            LoadTypes(useassemblies, null);
        }

        private void LoadTypes(IEnumerable<Assembly> useassemblies, IEnumerable<string> ignoreassemblies)
        {
            //strip duplicates first.
            //Notice: this code is mostly dead! consult MultiTypeManager...
            ManagedTypes = new List<Type>();
            if (ignoreassemblies == null) ignoreassemblies = new String[] {""};
            useassemblies = RemoveDuplicates<Assembly>(useassemblies, mcallback);
            foreach (var LoadAssembly in useassemblies.AsParallel())
            {
                int CountInFile = 0;
                Assembly assembly = LoadAssembly; //prevent access to local closure
                if (assembly.GetName().Name.Equals("script_testscript", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Print("test");
                }

                if ((!assembly.GetName().Name.StartsWith("script_", StringComparison.OrdinalIgnoreCase)) &&
                    ignoreassemblies.Any((y) => (assembly.GetName().Name.TestRegex(y) && !String.IsNullOrEmpty(y))))
                {
                    mcallback.ShowMessage("Skipping Assembly:" + LoadAssembly.GetName().Name);
                }
                else
                {
                    mcallback.ShowMessage("Inspecting Assembly:" + LoadAssembly.GetName().Name);
                    try
                    {
                        foreach (Type looptype in LoadAssembly.GetTypes())
                        {
                            CountInFile += CheckType(looptype) ? 1 : 0;
                        }
                    }
                    catch (ReflectionTypeLoadException rtle)
                    {
                        mcallback.ShowMessage
                        ("ReflectionTypeLoadException occured;" + rtle.StackTrace +
                         " InnerExceptions:");
                        foreach (Exception loopexception in rtle.LoaderExceptions)
                        {
                            mcallback.ShowMessage
                            ("RTLE Loader Exception:" + loopexception.Message + " Source:" +
                             loopexception.Source + "stack Trace:" + loopexception.StackTrace);
                        }

                        mcallback.ShowMessage("End of RTLE Loader Exceptions");

                        //continue enumerating.. we could see some of them...
                        foreach (Type looptype in rtle.Types)
                        {
                            CountInFile += CheckType(looptype) ? 1 : 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        mcallback.ShowMessage(ex.Message + " stack:" + ex.StackTrace);
                    }
                }

                mcallback.ShowMessage
                ("Found " + CountInFile.ToString() + " " + TypeManage.Name + " Implementations in " +
                 LoadAssembly.GetName());
                foreach (Type looptype in ManagedTypes)
                {
                    mcallback.ShowMessage(TypeManage.Name + " Implemented by:" + looptype.Name);
                }
            }

            mcallback.ShowMessage("Assembly enumeration complete. Removing duplicates...");
            _ManagedTypes = RemoveDuplicates(_ManagedTypes, mcallback).ToList();
        }

        public bool CheckType(Type looptype)
        {
            if (looptype == null) return false;
            if (RequiredVersionAttribute.TestComponent(looptype) == false)
            {
                Trace.WriteLine("RequiredVersionAttribute returned false for type " + looptype.FullName);
                return false;
            }

            if (inspectioncallback != null) inspectioncallback(looptype);
            if (CheckType(looptype, TypeManage, mcallback))
            {
                if (looptype != null && !ManagedTypes.Contains(looptype))
                    ManagedTypes.Add(looptype);
                return true;
            }

            return false;
        }

        public static bool CheckType(Type looptype, Type TypeManage, iManagerCallback mcallback)
        {
            if (looptype == null || TypeManage == null) return false;
            int CountInFile = 0;


            if (!looptype.IsAbstract)
            {
                //if this is the type we want already, then we add it.
                //Seems a class cannot be a "Subclass" of itself. Realistically if the Type we are looking for is itself
                //instantiable we want to include it.
                if (looptype == TypeManage)
                    return true;
                //added more recently: check wether it is a class derived from the given type.
                if (looptype.IsSubclassOf(TypeManage))
                {
                    mcallback.ShowMessage
                    ("Found " + TypeManage.Name + " Derivation:" +
                     looptype.Name);

                    //ManagedTypes.Add(looptype);
                    CountInFile++;
                    //break out of the immediate loop.
                }
                else
                {
                    foreach (var loopinterface in looptype.GetInterfaces())
                    {
                        try
                        {
                            if (loopinterface.Equals(TypeManage))
                            {
                                mcallback.ShowMessage
                                ("Found " + TypeManage.Name + " Implementor:" +
                                 looptype.Name);

                                //ManagedTypes.Add(looptype);
                                CountInFile++;
                                //break out of the immediate loop.
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Print("Exception:" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                // Debug.Print("Skipped Abstract class:" + looptype.FullName);
            }

            return CountInFile > 0;
        }
    }
}