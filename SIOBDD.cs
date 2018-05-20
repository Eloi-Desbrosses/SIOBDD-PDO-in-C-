using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;

namespace SIOBDDNS
{

    public abstract class SIOBDD
    {
        protected DictionaryNode baseQuery(DataSet dataSet)
        {
            DictionaryNode result = new DictionaryNode();

            try
            {
                foreach (DataColumn column in dataSet.Tables[0].Columns)
                {
                    result.Add(column.ColumnName, new ListNode());
                }

                foreach (DataColumn column in dataSet.Tables[0].Columns)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        result[column.ColumnName].Add(new nodeContent(row[column.ColumnName].ToString()));
                    }
                }
                dataSet.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return result;
        }
    }


    public class SIOBDDTOOL
    {
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if ((strSource.Contains(strStart) && strSource.Contains(strEnd)) ||
                (strSource.Contains(strStart.ToLower()) && strSource.Contains(strEnd.ToLower())))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
    }

    public class SIOBDDMYSQL : SIOBDD
    {
        private OdbcConnection connexion;

        public OdbcConnection Connexion
        {
            get
            {
                return connexion;
            }

            set
            {
                connexion = value;
                Connexion.Open();
            }
        }

        public SIOBDDMYSQL(String chaineConnexion)
        {
            try
            {
                OdbcConnection Connexion = new OdbcConnection(chaineConnexion);
                Connexion.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Concat(e.Message, " ", chaineConnexion));
            }
        }


        public SIOBDDMYSQL(String domaine, String login, String mdp)
        {
            //localhostSource
            Connexion = new OdbcConnection(string.Concat("Dsn=", domaine, ";uid=", login, ";Pwd=", mdp, ";"));
            try
            {
                Connexion.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, " ", Connexion);
            }
        }

        public DictionaryNode query(string query)
        {
            OdbcDataAdapter dataAdapter = new OdbcDataAdapter(query, this.Connexion);
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            DictionaryNode result = base.baseQuery(dataSet);
            dataAdapter.Dispose();
            return result;
        }

        public bool execute(string query)
        {
            try
            {
                OdbcCommand command = new OdbcCommand(query, this.Connexion);
                int test = command.ExecuteNonQuery();
                command.Dispose();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }


    public class SIOBDDORACLE : SIOBDD
    {
        private OleDbConnection connexion;

        public OleDbConnection Connexion
        {
            get
            {
                return connexion;
            }

            set
            {
                connexion = value;
            }
        }

        public SIOBDDORACLE(String chaineConnexion)
        {
            try
            {
                OleDbConnection Connexion = new OleDbConnection(chaineConnexion);
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Concat(e.Message, " ", chaineConnexion));
            }
        }


        public SIOBDDORACLE(String domaine, String login, String mdp)
        {
            //localhostSource
            Connexion = new OleDbConnection(string.Concat("Provider=MSDAORA;Data Source=", domaine, ";User ID=", login, ";Password=", mdp, ";"));
            try
            {
                Connexion.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, " ", Connexion);
            }
        }

        public DictionaryNode query(string query)
        {

            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, this.Connexion);
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            DictionaryNode result = base.baseQuery(dataSet);
            dataAdapter.Dispose();
            return result;
        }

        public bool execute(string query)
        {
            try
            {
                OleDbCommand command = new OleDbCommand(query, this.Connexion);
                int test = command.ExecuteNonQuery();
                command.Dispose();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }

    public class SIOBDDXML
    {
        private string url;
        XmlDocument xmlDoc = new XmlDocument();

        public SIOBDDXML(string url)
        {
            this.url = url;
            try
            {
                xmlDoc.Load(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public string Url { get => url; set => url = value; }

        public void getContent(ref DictionaryNode content)
        {
            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                if (!content.ContainsKey(node.Name))
                {
                    content.Add(node.Name, new ListNode());
                    content[node.Name].Add(this.nodeContent(node));
                }
                else
                {
                    content[node.Name].Add(this.nodeContent(node));
                }
            }
        }

        private DictionaryNode nodeContent(XmlNode specificNode)
        {
            DictionaryNode content = new DictionaryNode();

            foreach (XmlNode nodeInSpecificNode in specificNode.ChildNodes)
            {
                if (nodeInSpecificNode.ChildNodes.Count > 1)
                {
                    content.Add(nodeInSpecificNode.Name, new ListNode());
                    content[nodeInSpecificNode.Name].Add(this.nodeContent(nodeInSpecificNode));
                }
                else
                {
                    content.Add(nodeInSpecificNode.Name, new ListNode());
                    content[nodeInSpecificNode.Name].Add(new nodeContent(nodeInSpecificNode.InnerText));
                }
            }

            return content;
        }
    }

    public class DictionaryNode
    {
        public Dictionary<string, ListNode> Dictionary { get; } = new Dictionary<string, ListNode>();

        public ListNode this[string name]
        {
            get
            {
                return Dictionary[name];
            }
        }

        public void Add(string name, ListNode thingToAdd)
        {
            Dictionary.Add(name, thingToAdd);
        }

        public bool ContainsKey(string name)
        {
            if (Dictionary.ContainsKey(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Count()
        {
            return Dictionary.Count;
        }
    }

    public class ListNode
    {
        public List<DictionaryNode> List { get; } = new List<DictionaryNode>();

        public DictionaryNode this[int index]
        {
            get
            {
                return List[index];
            }
        }

        public void Add(DictionaryNode thingToAdd)
        {
            List.Add(thingToAdd);
        }

        //to make writing [0] optional
        public ListNode this[string name]
        {
            get
            {
                return List[0][name];
            }
        }

        public int Count()
        {
            return List.Count;
        }
    }

    public class nodeContent : DictionaryNode
    {
        private string content;

        public string Content { get => content; set => content = value; }

        public nodeContent(string content)
        {
            this.content = content;
        }

        public override string ToString()
        {
            return content;
        }
    }
}
