using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Diagnostics;



namespace bcHighScores
{
    //HighScores class
    //interfaces with http://bc-programming.com/HighScore/HighScores.php to retrieve eligible scores
    //determine if a score is eligible:
    //access http://bc-programming.com/HighScore/HighScores.php?action=eligible&gameID=<downloadID>&score=<score to test>
    //gives back (as text) rank number that the given score would achieve; (-1 if score doesn't rank above 20)


    //php compatible md5 function...


    /// <summary>
    /// Class containing the static routine for creating PHP compatible MD5 hashes.
    /// </summary>
    public sealed class PHPCompatible
    {
        public static string MD5Hash(string pass)
        {
            MD5 md5 = MD5CryptoServiceProvider.Create();
            byte[] dataMD5 = md5.ComputeHash(Encoding.Default.GetBytes(pass));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dataMD5.Length; i++)
                sb.AppendFormat("{0:x2}", dataMD5[i]);
            return sb.ToString();
        }
    }
    /// <summary>
    /// HighScoreManager: Manages a set of scores.
    /// Note that this is actually used by the LevelSetClass during Serialization/Deserialization; that is, it merely returns the 
    /// existing entry here; this class creates new LocalHighScores when nonexistent names are requested.
    /// </summary>
    [Serializable]
    public class HighScoreManager : ISerializable
    {

        private Dictionary<String, LocalHighScores> ManagedScores = new Dictionary<String, LocalHighScores>();
        public String sFilename;

        public IEnumerable<KeyValuePair<String, LocalHighScores>> GetScores()
        {

            foreach (var loopval in ManagedScores)
            {

                yield return loopval;

            }




        }
        public LocalHighScores getScoreForSetName(String ScoreSetName) //sort of a hack...
        {
            return (from p in ManagedScores where p.Key.IndexOf(ScoreSetName, StringComparison.OrdinalIgnoreCase) > -1 select p).First().Value;


        }

        public LocalHighScores this[String scoresetname]
        {
            get
            {
                if (ManagedScores.ContainsKey(scoresetname))
                    return ManagedScores[scoresetname];
                else
                {
                    LocalHighScores createscores = new LocalHighScores(20);
                    createscores.FillWithProxyData();
                    ManagedScores.Add(scoresetname, createscores);

                    return createscores;
                }



            }

            set
            {
                ManagedScores[scoresetname] = value;



            }




        }
        ~HighScoreManager()
        {
            Debug.Print("HighScoreManager- Saving");
            Save();



        }
        public static HighScoreManager FromFile(String sfilename)
        {
            BinaryFormatter ibin = new BinaryFormatter();
            try
            {
                using (FileStream fstream = new FileStream(sfilename, FileMode.Open))
                {

                    GZipStream gzs = new GZipStream(fstream, CompressionMode.Decompress);
                    HighScoreManager returnthis = (HighScoreManager)ibin.Deserialize(gzs);

                    returnthis.sFilename = sfilename;
                    gzs.Close();
                    return returnthis;
                }
            }
            catch
            {
                Debug.Print("Error during Score deserialization...");
                return null; //null to indicate error...
            }



        }
        public void Save()
        {
            Save(sFilename);

        }
        /*
            public static HighScoreManager Load(String filename)
            {
                //loading function...
                Debug.Print("Loading scores from " + filename);
                BinaryFormatter ibin = new BinaryFormatter();
                try
                {
                    FileStream fstream = new FileStream(filename, FileMode.Open);
                    GZipStream gzs = new GZipStream(fstream, CompressionMode.Decompress);
                    ibin = new BinaryFormatter();
                    HighScoreManager hsm = (HighScoreManager)ibin.Deserialize(gzs);

                }
                catch(Exception ex)
                {
                    return null; //return null to indicate error. We might change this to rethrow...

                }
            }
        */
        //woopsee I duplicated FromFile in Load...
        public void Save(String psfilename)
        {
            if (psfilename == null) return;
            Debug.Print("Saving Scores to " + psfilename);
            Debug.Print("Score Lists:" + ManagedScores.Count.ToString());

            foreach (var loopscore in ManagedScores)
            {
                Debug.Print("key:" + loopscore.Key + " #" + loopscore.Value.GetScores().Count());


            }

            BinaryFormatter ibin = new BinaryFormatter();
            FileStream fstream = new FileStream(psfilename, FileMode.Create);

            GZipStream gzstream = new GZipStream(fstream, CompressionMode.Compress);
            //ibin.Serialize(fstream, this);
            ibin.Serialize(gzstream, this);
            gzstream.Close();

        }
        public HighScoreManager(String pfilename)
        {
            sFilename = pfilename;


        }

        #region ISerializable Members
        public HighScoreManager(SerializationInfo info, StreamingContext context)
        {
            ManagedScores = (Dictionary<String, LocalHighScores>)info.GetValue("ScoreList", typeof(Dictionary<String, LocalHighScores>));


        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ScoreList", ManagedScores);
        }

        #endregion
    }
    [Serializable]
    public class HighScoreEntry : ISerializable, IComparable<HighScoreEntry>, ICloneable
    {

        public String Name;
        public int Score;
        public DateTime DateSet;
        public String ScoreSetName;
        public int ScoreSetHash;
        public HighScoreEntry(String pname, int pscore, DateTime pDateSet)
        {
            Name = pname.Trim();
            Score = pscore;
            DateSet = pDateSet;


        }
        public HighScoreEntry(String pname, int pscore)
            : this(pname, pscore, DateTime.Now)
        {

        }

        public HighScoreEntry(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name").Trim();
            Score = info.GetInt32("Score");
            DateSet = info.GetDateTime("DateSet");
        }


        public override string ToString()
        {
            return Name + "," + Score;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Score", Score);
            info.AddValue("DateSet", DateSet);
        }

        #endregion

        #region IComparable<HighScoreEntry> Members

        public int CompareTo(HighScoreEntry other)
        {
            return this.Score.CompareTo(other.Score);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new HighScoreEntry((String)Name.Clone(), Score, DateSet);
        }

        #endregion
    }
    /// <summary>
    /// defines interfaces that must be supported by HighScore classes. ATM we have "LocalHighScores" and "NetHighScores".
    /// </summary>
    public interface IHighScoreList : ISerializable
    {
        /// <summary>
        /// reloads the scores from whatever medium/net access the implementation has.
        /// </summary>
        void Reload();
        // <summary>
        /// Enumerates all saved scores in this class instance.
        /// </summary>
        /// <returns>All HighScoreEntry's stored in this class, from highest score (1st place) to lowest score.</returns>
        IEnumerable<HighScoreEntry> GetScores();
        /// <summary>
        /// determines wether the given score makes it to a position on the score list.
        /// </summary>
        /// <param name="score"></param>
        /// <returns>the rank the score achieves, or -1 if it doesn't make the cut.
        /// Note that a Name can only appear on a HighScore List a single time: in order
        /// for a name that exists in the scorelist to be eligible again the score must be higher then the existing score for the same name.
        /// in the web-enabled implementation, this is handled automagically by the PHP script.
        /// </returns>
        int Eligible(String Name, int score);
        HighScoreEntry Submit(String Name, int score);
    }

    /// <summary>
    /// class used to store/save/load a set of highscores for the local machine.
    /// </summary>
    /// 
    [Serializable]
    public class LocalHighScores : ISerializable, IHighScoreList, IDeserializationCallback
    {

        public SortedList<HighScoreEntry, HighScoreEntry> Scores = new SortedList<HighScoreEntry, HighScoreEntry>();
        /// <summary>
        /// Maximum number of scores possible to store here.
        /// </summary>
        private int MaxScores = 20;
        public static Random rgen = new Random();
        public static T Choose<T>(IEnumerable<T> ChooseArray)
        {
            if (rgen == null) rgen = new Random();
            SortedList<double, T> sorttest = new SortedList<double, T>();
            foreach (T loopvalue in ChooseArray)
            {
                sorttest.Add(rgen.NextDouble(), loopvalue);

            }

            //return the first item.
            return sorttest.First().Value;
        }
        public void FillWithProxyData()
        {
            String[] RandomNames = new string[]{"Slash","Jam","Nancy","Tommy","Linda","Wendy","Mario","Luigi","Jezebel","Clownman","Jok4r","Scarycat","Dictionary attack","Clojure Lover","M$ Shill","Lintard","RMS Troll","Max",
            "Bearsky","Toad","h4x0r","Jim","Sir Slush","Marle","Terra","random pidgeon","unrandom pidgeon","barfly","Chuck","Jok4r","Israphel","Honeydew","Simon","Lewis","Paul the Musician","Carl the Yellow","FlowerFace","Nibbles.bas","Paddler","RAZAR","BABAR","SNUFFLEOPAGUS","Big Bird","One Fish","Two Fish","Red Fish","Blue Fish","Santa","Oogie","Flashlight","pickleJar","Blanket Jackson","Screwdriver face","PUSSYWHISTLE","Stewart Cheifet","Bears","SAMSINGER"};
            Scores = new SortedList<HighScoreEntry, HighScoreEntry>();
            for (int i = 0; i < MaxScores; i++)
            {


                bool erroroccured = true;
                while (erroroccured && Scores.Count < MaxScores)
                {
                    String nameuse = Choose(RandomNames);
                    int Scoresubmit = rgen.Next(0, 20000);
                    HighScoreEntry addentry = new HighScoreEntry(nameuse, Scoresubmit);
                    try
                    {
                        Scores.Add(addentry, addentry);
                        erroroccured = false;
                    }
                    catch (ArgumentException ae)
                    {
                        erroroccured = true;


                    }


                }
            }


        }

        /// <summary>
        /// Creates an instance of LocalHighScores that can hold "pMaxScores" entries.
        /// </summary>
        /// <param name="pMaxScores">Maximum number of entries this HighScores List will store.</param>
        public LocalHighScores(int pMaxScores)
        {
            MaxScores = pMaxScores;
            FillWithProxyData();
        }
        /// <summary>
        /// Create a LocalHighScores instance from a given netScores instance.
        /// </summary>
        /// <param name="fromnetscores">netscores instance to copy</param>
        /// <param name="maxCount">maxcount of resulting score table</param>
        public LocalHighScores(NetHighScores fromnetscores, int maxCount)
        {
            MaxScores = maxCount;

            foreach (HighScoreEntry loopscore in fromnetscores.GetScores())
            {
                HighScoreEntry clonedval = (HighScoreEntry)loopscore.Clone();
                Scores.Add(clonedval, clonedval);



            }

        }

        /// <summary>
        /// Enumerates all saved scores in this class instance.
        /// </summary>
        /// <returns>All HighScoreEntry's stored in this class, from highest score (1st place) to lowest score.</returns>
        public IEnumerable<HighScoreEntry> GetScores()
        {
            for (int i = Scores.Count - 1; i >= 0; i--)
            {
                yield return Scores.Values[i];
            }
        }
        /// <summary>
        /// Purge: Preconditions:number of items in "Scores" exceeds MaxScores.
        /// Action: removes lowest scoring entries. This is done by iterating backwards through the list and removing entries 
        /// until there are "MaxScores" entries.
        /// </summary>
        private void Purge()
        {
            for (int i = 0; Scores.Count > MaxScores; i++)
            {

                Scores.Remove(Scores.Values[i]);


            }


        }
        public void Reload()
        {
            //stub, since we have no idea if we loaded from something or not.

        }
        public int ScoreEligible(int Score)
        {
            var getbeatenscores = (from sscore in Scores where sscore.Value.Score < Score select sscore).Count();
            return getbeatenscores;

        }
        /// <summary>
        /// Determines if the given score is eligible.
        /// Note that without a name, this could return different results compared to the Eligible(Name,Score) overload.
        /// </summary>
        /// <param name="Score"></param>
        /// <returns></returns>
        public int Eligible(int Score)
        {
            var resultentry = from sname in Scores where sname.Value.Score < Score select sname;
            if (resultentry.Count() > 0)
            {
                return resultentry.Count() + 1;


            }
            else
            {
                return 0;
            }


        }

        public int Eligible(String Name, int Score)
        {
            //first, if there is a existing score with the given Name...
            //if the score for that entry is more then the Score parameter, return -1 (not ranked)
            //if the score for the entry is less then the score parameter, return our "standard" result (the count of all scores greater then the passed score

            /*
            var resultentry = from sname in Scores where sname.Value.Name==Name select sname;


            if (resultentry.Count() > 0)
            {
                var gotitem = resultentry.First();
                if (gotitem.Value.Score > Score)
                    return -1; //not eligible, existing score entry exists with same name but higher score.
                

                //otherwise, it continues on it's merry way.
            }
            */
            //first there cannot be a entry with the same name and score.
            if ((from sc in Scores where sc.Value.Score == Score && sc.Value.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) select sc).Count() > 0)
            {
                return -1;

            }

            //add one: if the first place score is beaten, the result should be 1, not 0 :D
            int resultcount = (from sc in Scores where sc.Value.Score > Score select sc).Count() + 1;
            if (resultcount >= MaxScores) return -1;
            return resultcount;


        }

        public HighScoreEntry Submit(String Name, int score)
        {
            //check if an existing entry exists with the same name...
            //This segment was removed because the result was kind of confusing.
            //it makes sense for the on-line scoring, but locally duplicate names should be allowed.
            /*
            var resultentry = from sname in Scores where sname.Value.Name == Name select sname;
            if (resultentry.Count() > 0)
            {
                var gotitem = resultentry.First();
                //if the found entry has a lower score, update it. otherwise, leave it as is.
                if (gotitem.Value.Score < score)
                {
                    gotitem.Value.Score = score;
                    //remove it... new item will be added later in the procedure.
                    Scores.Remove(gotitem.Value);

                }
                else
                {


                    return gotitem.Value;
                }

                //otherwise, it continues on it's merry way.
            }
            */


            HighScoreEntry newentry = new HighScoreEntry(Name, score);
            Scores.Add(newentry, newentry);
            if (Scores.Count > MaxScores) Purge();
            return newentry;
        }
        List<HighScoreEntry> getScores = null;
        public LocalHighScores(SerializationInfo info, StreamingContext context)
        {
            //load highscores.
            Debug.Print("Deserializing HighScores");
            getScores =
                 (List<HighScoreEntry>)
                 info.GetValue("HighScores", typeof(List<HighScoreEntry>));



        }
        //save highscores.
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Debug.Print("Saving LocalHighScores count=" + Scores.Count);
            if (Scores.Count > 0)
            {
                Debug.Print("Count is non-zero");

            }

            //serialize as List, not a SortedList...
            List<HighScoreEntry> SaveList = new List<HighScoreEntry>(Scores.Values);

            info.AddValue("HighScores", SaveList, typeof(List<HighScoreEntry>));
            //info.AddValue("HighScores", Scores, typeof (SortedList<HighScoreEntry, HighScoreEntry>));

        }

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            Scores = new SortedList<HighScoreEntry, HighScoreEntry>();
            foreach (HighScoreEntry loopentry in from n in getScores where n != null select n)
            {
                Scores.Add(loopentry, loopentry);

            }
        }

        #endregion
    }

    /// <summary>
    /// Class for communicating with a PHP page for retrieving/submitting scores.
    /// </summary>
    public class NetHighScores : IHighScoreList
    {
        public List<HighScoreEntry> scorelist = new List<HighScoreEntry>();

        public static readonly string EligibleFmt =
            "http://bc-programming.com/hiscore/hiscores.php?action=eligible&gameID={0}&name={1}&score={2}&scoresethash={3}";

        public static readonly string SubmitFmt =
            "http://bc-programming.com/hiscore/hiscores.php?action=submit&gameID={0}&name={1}&score={2}&hashvalue={3}&scoresethash={4}&scoresetname={5}";

        public static readonly string simpleviewfmt =
            "http://bc-programming.com/hiscore/hiscores.php?action=simpleview&gameID={0}&scoresethash={1}";


        //$scoresethash = $_GET["scoreset"];
        //$scoresetname = $_GET["scoresetname"];


        private WebClient usecli;
        private int mGameID;
        private int mScoreSetHash;
        private String mScoreSetName;
        private static String GetSubmitString(int gameID, String nickname, int score, int ScoreSetHash, String ScoreSetName)
        {
            String hashval = PHPCompatible.MD5Hash(gameID.ToString() + nickname + score.ToString());
            String useurl = String.Format(SubmitFmt, gameID.ToString(), nickname, score.ToString(), hashval, ScoreSetHash.ToString(), ScoreSetName);
            return useurl;
        }

        public IEnumerable<HighScoreEntry> GetScores()
        {
            foreach (HighScoreEntry loopentry in scorelist)
                yield return loopentry;
        }

        private static String GetEligibleString(int gameID, String Name, int score, int ScoreSetHash)
        {
            return String.Format(EligibleFmt, gameID.ToString(), Name, score.ToString(), ScoreSetHash.ToString());
        }

        private static String GetSimpleViewString(int GameID, int ScoreSetHash)
        {
            return String.Format(simpleviewfmt, GameID.ToString(), ScoreSetHash);
        }

        public HighScoreEntry Submit(String Nickname, int score)
        {
            String urlload = GetSubmitString(mGameID, Nickname, score, mScoreSetHash, mScoreSetName);


            Trace.WriteLine("using URL GET Request:" + urlload);
            String httpresponse = GetURL(urlload);
            Debug.Print("response:" + httpresponse);
            var added = new HighScoreEntry(Nickname, score);
            scorelist.Add(added);
            return added;
            //your highscore has been submitted should be the result.
        }

        private string GetURL(String urlload)
        {
            StreamReader sreader = new StreamReader(usecli.OpenRead(urlload));
            String returnthis = sreader.ReadToEnd();
            sreader.Close();
            return returnthis;
        }

        public int Eligible(String Name, int score)
        {
            String useURL = GetEligibleString(mGameID, Name, score, mScoreSetHash);
            string wholeret = GetURL(useURL);
            int returnint = 0;
            int.TryParse(wholeret, out returnint);
            return returnint;
        }
        public void Reload()
        {
            if (mGameID > 0)
            {
                loadScores(mGameID);


            }


        }

        //loads the scores for the given gameID into the dictionary.
        private void loadScores(int GameID)
        {
            String viewurl = GetSimpleViewString(GameID, mScoreSetHash);
            Debug.Print("Loading Scores from " + viewurl);
            String HighScoredata = GetURL(viewurl);
            Debug.Print("highscoredata=" + HighScoredata);

            //split at &&&
            String[] eachentry = HighScoredata.Split(new String[] { "&&&" }, StringSplitOptions.RemoveEmptyEntries);
            //clear the dictionary...
            scorelist = new List<HighScoreEntry>();
            foreach (String loopstring in eachentry)
            {
                String[] innersplit = loopstring.Split(new String[] { "$$$" }, StringSplitOptions.RemoveEmptyEntries);
                int gotscore = int.Parse(innersplit[1]);
                DateTime gotdateset = DateTime.Parse(innersplit[2]);
                scorelist.Add(new HighScoreEntry(innersplit[0], gotscore, gotdateset));
            }
            //Debug.Print("loaded " + scorelist.Count.ToString() + " scores.");
            Trace.Write("loaded " + scorelist.Count.ToString() + " scores.");
        }
        public NetHighScores(SerializationInfo info, StreamingContext context)
        {
            //TODO: serialization will store the GameID and scoreSet Hash for this netHighScores item.



        }

        public NetHighScores(int GameID, String ScoreSetName, int ScoreSetHash)
        {
            mScoreSetName = ScoreSetName;
            mScoreSetHash = ScoreSetHash;
            mGameID = GameID;
            usecli = new WebClient();

            loadScores(GameID);
        }



        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}