using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;

namespace BlogWrite.Models
{
    public class DataAccess
    {
        private string _dataBaseFilePath;
        public string DataBaseFilePath
        {
            get { return _dataBaseFilePath; }
        }

        private SqliteConnectionStringBuilder connectionStringBuilder;

        public ResultWrapper InitializeDatabase(string dataBaseFilePath)
        {
            ResultWrapper res = new ResultWrapper();

            _dataBaseFilePath = dataBaseFilePath;

            // Create a table if not exists.
            connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dataBaseFilePath
            };

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();

                    using (var tableCmd = connection.CreateCommand())
                    {
                        tableCmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Entry (" +
                                "ID TEXT NOT NULL PRIMARY KEY," +
                                "Feed_ID TEXT NOT NULL," +
                                "Entry_ID TEXT NOT NULL," +
                                "Url TEXT NOT NULL," +
                                "Title TEXT," +
                                "Published TEXT NOT NULL," +
                                "Summary TEXT," +
                                "SummaryPlainText TEXT," +
                                "Content TEXT," +
                                "ContentType TEXT," +
                                "ImageUrl TEXT," +
                                "Image BLOB," +
                                "Status TEXT," +
                                "Flag INTEGER)";
                            tableCmd.ExecuteNonQuery();

                            tableCmd.Transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            tableCmd.Transaction.Rollback();

                            res.IsError = true;
                            res.Error.ErrType = ErrorObject.ErrTypes.DB;
                            res.Error.ErrCode = 0;
                            res.Error.ErrText = "「" + e.Message + "」";
                            res.Error.ErrDescription = "";
                            res.Error.ErrDatetime = DateTime.Now;
                            res.Error.ErrPlace = "@DataAccess::InitializeDatabase::Transaction.Commit";

                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = 0;
                    res.Error.ErrText = "「" + ex.Message + "」";
                    res.Error.ErrDescription = "";
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "TargetInvocationException@DataAccess::InitializeDatabase::connection.Open";

                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = 0;
                    res.Error.ErrText = "「" + ex.Message + "」";
                    res.Error.ErrDescription = "";
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "InvalidOperationException@DataAccess::InitializeDatabase::connection.Open";

                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = 0;

                    if (e.InnerException != null)
                    {
                        res.Error.ErrText = "「" + e.InnerException.Message + "」";
                    }
                    else
                    {
                        res.Error.ErrText = "「" + e.Message + "」";
                    }
                    res.Error.ErrDescription = "";
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "Exception@DataAccess::InitializeDatabase::connection.Open";
                }
            }

            return res;
        }

        public ResultWrapper SelectEntriesByFeedId(ObservableCollection<EntryItem> entries, string feedId)
        {
            ResultWrapper res = new ResultWrapper();

            entries.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT * FROM Entry WHERE Feed_ID = '{0}'", feedId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EntryItem entry = new EntryItem(Convert.ToString(reader["Title"]), feedId, null);

                                entry.EntryId = Convert.ToString(reader["Entry_ID"]);

                                if (!string.IsNullOrEmpty(Convert.ToString(reader["Url"])))
                                    entry.AltHtmlUri = new Uri(Convert.ToString(reader["Url"]));
                                
                                entry.Published = DateTime.Parse(Convert.ToString(reader["Published"]));
                                
                                entry.Summary = Convert.ToString(reader["Summary"]);
                                
                                entry.SummaryPlainText = Convert.ToString(reader["SummaryPlainText"]);
                                
                                entry.Content = Convert.ToString(reader["Content"]);

                                //string ct = Convert.ToString(reader["ContentType"]);
                                entry.ContentType = EntryItem.ContentTypes.textHtml;

                                if (!string.IsNullOrEmpty(Convert.ToString(reader["ImageUrl"])))
                                    entry.ImageUri = new Uri(Convert.ToString(reader["ImageUrl"]));

                                if (reader["Image"] != DBNull.Value)
                                {
                                    byte[] imageBytes = (byte[])reader["Image"];
                                    entry.Image = BitmapImageFromBytes(imageBytes);
                                }

                                //entry.Status = Convert.ToString(reader["Status"]);
                                
                                //Convert.ToInt32(reader["Flag"]);

                                entries.Add(entry);

                            }
                        }
                    }
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.WriteLine("Opps. TargetInvocationException@DataAccess::SelectEntriesByFeedId");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                res.Error.ErrText = "「" + ex.Message + "」";
                res.Error.ErrDescription = "";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "TargetInvocationException@DataAccess::SelectEntriesByFeedId";

                throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectEntriesByFeedId");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                res.Error.ErrText = "「" + ex.Message + "」";
                res.Error.ErrDescription = "";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "InvalidOperationException@DataAccess::SelectEntriesByFeedId";

                throw ex.InnerException;
            }
            catch (Exception e)
            {
                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;

                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectEntriesByFeedId");
                    res.Error.ErrText = "「" + e.InnerException.Message + "」";
                }
                else
                {
                    Debug.WriteLine(e.Message + " @DataAccess::SelectEntriesByFeedId");
                    res.Error.ErrText = "「" + e.Message + "」";
                }
                res.Error.ErrDescription = "";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "Exception@DataAccess::SelectEntriesByFeedId";

            }

            return res;
        }

        public ResultWrapper InsertEntries(ObservableCollection<EntryItem> entries, string feedId)
        {
            ResultWrapper res = new ResultWrapper();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            foreach (var entry in entries)
                            {
                                /*
                                EntryItem entry = new("asdf", null);
                                entry.AltHtmlUri = new("http://localhost/");
                                entry.EntryId = Guid.NewGuid().ToString();
                                entry.Published = DateTime.Now;
                                entry.Summary = "Hoge";
                                entry.SummaryPlainText = "Ho";
                                entry.Content = "asdfasdfasdfasdfasdfasdf";
                                entry.ContentType = EntryItem.ContentTypes.textHtml;
                                entry.ImageUri = new("http://localhost/");
                                entry.Image = null;
                                entry.Status = EntryItem.EntryStatus.esNew;
                                */
                                string sqlInsert = "INSERT OR IGNORE INTO Entry (ID, Feed_ID, Entry_ID, Url, Title, Published, Summary, SummaryPlainText, Content, ContentType, ImageUrl, Image, Status, Flag) VALUES (@Id, @feedId, @EntryId, @AltHtmlUri, @Title, @Published, @Summary, @SummaryPlainText, @Content, @ContentType, @ImageUri, @Image, @Status, @Flag)";
                                    
                                cmd.CommandText = sqlInsert;
                                cmd.CommandType = CommandType.Text;

                                cmd.Parameters.Clear();

                                cmd.Parameters.AddWithValue("@Id", entry.Id);
                                cmd.Parameters.AddWithValue("@feedId", feedId);
                                if (entry.EntryId == null)
                                    Debug.WriteLine("entry.EntryId is null");
                                cmd.Parameters.AddWithValue("@EntryId", entry.EntryId);
                                if (entry.AltHtmlUri == null)
                                    Debug.WriteLine("entry.AltHtmlUri is null");
                                cmd.Parameters.AddWithValue("@AltHtmlUri", entry.AltHtmlUri.AbsoluteUri);
                                if (entry.Title != null)
                                    cmd.Parameters.AddWithValue("@Title", entry.Title);
                                else
                                    cmd.Parameters.AddWithValue("@Title", string.Empty);

                                cmd.Parameters.AddWithValue("@Published", entry.Published.ToString("yyyy-MM-dd HH:mm:ss"));
                                
                                if (entry.Summary != null)
                                    cmd.Parameters.AddWithValue("@Summary", entry.Summary);
                                else
                                    cmd.Parameters.AddWithValue("@Summary", string.Empty);

                                if (entry.SummaryPlainText != null)
                                    cmd.Parameters.AddWithValue("@SummaryPlainText", entry.SummaryPlainText);
                                else
                                    cmd.Parameters.AddWithValue("@SummaryPlainText", string.Empty);

                                if (entry.Content != null)
                                    cmd.Parameters.AddWithValue("@Content", entry.Content);
                                else
                                    cmd.Parameters.AddWithValue("@Content", string.Empty);

                                cmd.Parameters.AddWithValue("@ContentType", entry.ContentType.ToString());

                                if (entry.ImageUri != null)
                                    cmd.Parameters.AddWithValue("@ImageUri", entry.ImageUri.AbsoluteUri);
                                else
                                    cmd.Parameters.AddWithValue("@ImageUri", string.Empty);

                                if ((entry.ImageByteArray == Array.Empty<byte>()) || entry.ImageByteArray == null)
                                {
                                    cmd.Parameters.AddWithValue("@Image", DBNull.Value);
                                }
                                else
                                {
                                    SqliteParameter parameter1 = new SqliteParameter("@Image", System.Data.DbType.Binary);
                                    parameter1.Value = entry.ImageByteArray;
                                    cmd.Parameters.Add(parameter1);
                                }
                                //cmd.Parameters.AddWithValue("@Image", DBNull.Value);
                                /*
                                if (entry.Image != null)
                                {
                                    byte[] data = BitmapImageToByteArray(entry.Image);
                                    if (data != Array.Empty<byte>())
                                    {
                                        SqliteParameter parameter1 = new SqliteParameter("@Image", System.Data.DbType.Binary);
                                        parameter1.Value = BitmapImageToByteArray(entry.Image);
                                        cmd.Parameters.Add(parameter1);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@Image", DBNull.Value);
                                    }
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@Image", DBNull.Value);
                                }
                                */

                                cmd.Parameters.AddWithValue("@Status", entry.Status.ToString());
                                cmd.Parameters.AddWithValue("@Flag", 0);

                                var r = cmd.ExecuteNonQuery();
                                if (r > 0)
                                {
                                    Debug.WriteLine("Inserted: " + r.ToString() + " for " + entry.EntryId);
                                }
                                else
                                {
                                    Debug.WriteLine("Inserted: " + r.ToString() + " for " + entry.EntryId);
                                }
                            }

                            //　コミット
                            cmd.Transaction.Commit();

                            res.IsError = false;
                        }
                        catch (Exception e)
                        {
                            res.IsError = true;

                            cmd.Transaction.Rollback();

                            res.Error.ErrType = ErrorObject.ErrTypes.DB;
                            res.Error.ErrCode = 0;
                            res.Error.ErrText = "「" + e.Message + "」";
                            res.Error.ErrDescription = "";
                            res.Error.ErrDatetime = DateTime.Now;
                            res.Error.ErrPlace = "@DataAccess::InsertEntries::Transaction.Commit";

                            Debug.WriteLine(e.Message + " @DataAccess::InsertEntries::Transaction.Commit");
                        }
                    }
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.WriteLine("Opps. TargetInvocationException@DataAccess::InsertEntries");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                res.Error.ErrText = "「" + ex.Message + "」";
                res.Error.ErrDescription = "";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "TargetInvocationException@DataAccess::InsertEntries::connection.Open";

                throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine("Opps. InvalidOperationException@DataAccess::InsertEntries");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                res.Error.ErrText = "「" + ex.Message + "」";
                res.Error.ErrDescription = "";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "InvalidOperationException@DataAccess::InsertEntries::connection.Open";

                throw ex.InnerException;
            }
            catch (Exception e)
            {
                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;

                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message + " @DataAccess::InsertEntries");
                    res.Error.ErrText = "「" + e.InnerException.Message + "」";
                }
                else
                {
                    Debug.WriteLine(e.Message + " @DataAccess::InsertEntries");
                    res.Error.ErrText = "「" + e.Message + "」";
                }
                res.Error.ErrDescription = "";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "Exception@DataAccess::InsertEntries::connection.Open";
            }

            return res;
        }
       
        public static Byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
        {
            try
            {
                byte[] data = Array.Empty<byte>();

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + " @DataAccess::BitmapImageToByteArray");

                return Array.Empty<byte>();
            }
        }

        public static BitmapImage BitmapImageFromBytes(Byte[] bytes)
        {
            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    BitmapImage bmimage = new BitmapImage();
                    bmimage.BeginInit();
                    bmimage.CacheOption = BitmapCacheOption.OnLoad;
                    bmimage.StreamSource = stream;
                    bmimage.EndInit();

                    return bmimage;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + " @DataAccess::BitmapImageFromBytes");

                return null;
            }
        }

        // ColumnExists check
        public bool ColumnExists(IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false; ;
        }

    }
}
