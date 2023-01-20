using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using BlogWrite.Contracts.Services;

namespace BlogWrite.Services;

internal class DataAccessService : IDataAccessService
{
    private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new();

    public DataAccessService()
    {
    
    }

    public SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath)
    {
        var res = new SqliteDataAccessResultWrapper();

        // Create a table if not exists.
        connectionStringBuilder.DataSource = dataBaseFilePath;

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
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
                                "Author TEXT," +
                                "Summary TEXT," +
                                "Content TEXT," +
                                "ContentType TEXT," +
                                "ImageUrl TEXT," +
                                "IsImageDownloaded TEXT," +
                                "Image_ID TEXT," +
                                "Status TEXT," +
                                "IsArchived TEXT)";

                            tableCmd.ExecuteNonQuery();

                            tableCmd.Transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            tableCmd.Transaction.Rollback();

                            res.IsError = true;
                            res.Error.ErrType = ErrorObject.ErrTypes.DB;
                            res.Error.ErrCode = "";
                            res.Error.ErrText = "Exception";
                            res.Error.ErrDescription = e.Message;
                            res.Error.ErrDatetime = DateTime.Now;
                            res.Error.ErrPlace = "Transaction.Commit";
                            res.Error.ErrPlaceParent = "DataAccess::InitializeDatabase";

                            Debug.WriteLine("fuck");
                            return res;
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = "";
                    res.Error.ErrText = "TargetInvocationException";
                    res.Error.ErrDescription = ex.Message;
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "connection.Open";
                    res.Error.ErrPlaceParent = "DataAccess::InitializeDatabase";

                    Debug.WriteLine("fuck");
                    return res;
                }
                catch (System.InvalidOperationException ex)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = "";
                    res.Error.ErrText = "InvalidOperationException"; ;
                    res.Error.ErrDescription = ex.Message;
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "connection.Open";
                    res.Error.ErrPlaceParent = "DataAccess::InitializeDatabase";

                    Debug.WriteLine("fuck");
                    return res;
                }
                catch (Exception e)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = "";

                    if (e.InnerException != null)
                    {
                        res.Error.ErrText = "InnerException";
                        res.Error.ErrDescription = e.InnerException.Message;
                    }
                    else
                    {
                        res.Error.ErrText = "Exception";
                        res.Error.ErrDescription = e.Message;
                    }
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "connection.Open";
                    res.Error.ErrPlaceParent = "DataAccess::InitializeDatabase";

                    Debug.WriteLine("fuck");
                    return res;
                }
            }
        }
        catch
        {
            Debug.WriteLine("fuck");
        }

        return res;
    }

    public SqliteDataAccessSelectResultWrapper SelectEntriesByFeedId(string feedId, bool IsUnarchivedOnly = true)
    {
        SqliteDataAccessSelectResultWrapper res = new SqliteDataAccessSelectResultWrapper();

        if (string.IsNullOrEmpty(feedId))
            return res;

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    if (IsUnarchivedOnly)
                    {
                        cmd.CommandText = String.Format("SELECT * FROM Entry WHERE Feed_ID = '{0}' AND IsArchived = '{1}' ORDER BY Published DESC LIMIT 1000", feedId, bool.FalseString);
                    }
                    else
                    {
                        cmd.CommandText = String.Format("SELECT * FROM Entry WHERE Feed_ID = '{0}' ORDER BY Published DESC LIMIT 10000", feedId);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeedEntryItem entry = new FeedEntryItem(Convert.ToString(reader["Title"]), feedId, null);

                            //entry.MyNodeFeed = ndf;

                            entry.EntryId = Convert.ToString(reader["Entry_ID"]);

                            if (!string.IsNullOrEmpty(Convert.ToString(reader["Url"])))
                                entry.AltHtmlUri = new Uri(Convert.ToString(reader["Url"]));

                            entry.Published = DateTime.Parse(Convert.ToString(reader["Published"]));

                            entry.Summary = Convert.ToString(reader["Summary"]);

                            entry.Content = Convert.ToString(reader["Content"]);

                            entry.ContentType = EntryItem.ContentTypes.textHtml;

                            if (!string.IsNullOrEmpty(Convert.ToString(reader["ImageUrl"])))
                                entry.ImageUri = new Uri(Convert.ToString(reader["ImageUrl"]));

                            string bln = Convert.ToString(reader["IsImageDownloaded"]);
                            if (!string.IsNullOrEmpty(bln))
                            {
                                if (bln == bool.TrueString)
                                    entry.IsImageDownloaded = true;
                                else
                                    entry.IsImageDownloaded = false;
                            }

                            entry.ImageId = Convert.ToString(reader["Image_ID"]);

                            /*
                            if (reader["Image"] != DBNull.Value)
                            {
                                byte[] imageBytes = (byte[])reader["Image"];

                                if (Application.Current != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        entry.Image = BitmapImageFromBytes(imageBytes);
                                    });
                                }

                                entry.IsImageDownloaded = true;
                            }
                            */

                            string status = Convert.ToString(reader["Status"]);
                            if (!string.IsNullOrEmpty(status))
                                entry.Status = entry.StatusTextToType(status);

                            entry.Author = Convert.ToString(reader["Author"]);

                            string blnstr = Convert.ToString(reader["IsArchived"]);
                            if (!string.IsNullOrEmpty(blnstr))
                            {
                                if (blnstr == bool.TrueString)
                                    entry.IsArchived = true;
                                else
                                    entry.IsArchived = false;
                            }

                            //
                            if (entry.IsArchived)
                                if (entry.Status == FeedEntryItem.ReadStatus.rsNew)
                                    entry.Status = FeedEntryItem.ReadStatus.rsNormal;

                            if (!entry.IsArchived)
                            {
                                res.UnreadCount++;
                            }

                            res.AffectedCount++;

                            res.SelectedEntries.Add(entry);
                        }
                    }
                }
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "TargetInvocationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByFeedId";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectEntriesByFeedId");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByFeedId";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = e.ToString();
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectEntriesByFeedId");
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectEntriesByFeedId");
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByFeedId";

            return res;
        }

        return res;
    }

    public SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIds(List<string> feedIds, bool IsUnarchivedOnly = true)
    {
        var res = new SqliteDataAccessSelectResultWrapper();

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        string before = "SELECT * FROM Entry WHERE ";

        string middle = "(";

        foreach (var asdf in feedIds)
        {
            if (middle != "(")
                middle = middle + "OR ";

            middle = middle + String.Format("Feed_ID = '{0}' ", asdf);
        }

        //string after = string.Format(") AND IsArchived = '{0}' ORDER BY Published DESC LIMIT 1000", bool.FalseString);
        string after;
        if (IsUnarchivedOnly)
        {
            after = string.Format(") AND IsArchived = '{0}' ORDER BY Published DESC LIMIT 1000", bool.FalseString);
        }
        else
        {
            after = string.Format(") ORDER BY Published DESC LIMIT 10000");
        }


        //Debug.WriteLine(before + middle + after);

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = before + middle + after;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeedEntryItem entry = new FeedEntryItem(Convert.ToString(reader["Title"]), Convert.ToString(reader["Feed_ID"]), null);

                            //entry.MyNodeFeed = ndf;

                            entry.EntryId = Convert.ToString(reader["Entry_ID"]);

                            if (!string.IsNullOrEmpty(Convert.ToString(reader["Url"])))
                                entry.AltHtmlUri = new Uri(Convert.ToString(reader["Url"]));

                            entry.Published = DateTime.Parse(Convert.ToString(reader["Published"]));

                            entry.Summary = Convert.ToString(reader["Summary"]);

                            entry.Content = Convert.ToString(reader["Content"]);

                            entry.ContentType = EntryItem.ContentTypes.textHtml;

                            if (!string.IsNullOrEmpty(Convert.ToString(reader["ImageUrl"])))
                                entry.ImageUri = new Uri(Convert.ToString(reader["ImageUrl"]));

                            /*
                            if (reader["Image"] != DBNull.Value)
                            {
                                byte[] imageBytes = (byte[])reader["Image"];

                                if (Application.Current != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        entry.Image = BitmapImageFromBytes(imageBytes);
                                    });
                                }

                                entry.IsImageDownloaded = true;
                            }
                            */

                            string bln = Convert.ToString(reader["IsImageDownloaded"]);
                            if (!string.IsNullOrEmpty(bln))
                            {
                                if (bln == bool.TrueString)
                                    entry.IsImageDownloaded = true;
                                else
                                    entry.IsImageDownloaded = false;
                            }

                            entry.ImageId = Convert.ToString(reader["Image_ID"]);

                            string status = Convert.ToString(reader["Status"]);
                            if (!string.IsNullOrEmpty(status))
                                entry.Status = entry.StatusTextToType(status);

                            entry.Author = Convert.ToString(reader["Author"]);

                            string blnstr = Convert.ToString(reader["IsArchived"]);
                            if (!string.IsNullOrEmpty(blnstr))
                            {
                                if (blnstr == bool.TrueString)
                                    entry.IsArchived = true;
                                else
                                    entry.IsArchived = false;
                            }

                            //
                            if (entry.IsArchived)
                                if (entry.Status == FeedEntryItem.ReadStatus.rsNew)
                                    entry.Status = FeedEntryItem.ReadStatus.rsNormal;

                            if (!entry.IsArchived)
                            {
                                res.UnreadCount++;
                            }

                            res.AffectedCount++;

                            res.SelectedEntries.Add(entry);
                        }
                    }
                }
            }
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByMultipleFeedIds";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = e.ToString();
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectEntriesByMultipleFeedIds");
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectEntriesByMultipleFeedIds");
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByMultipleFeedIds";

            return res;
        }

        return res;
    }

    /*
    public SqliteDataAccessSelectImageResultWrapper SelectImageByImageId(string imageId)
    {
        SqliteDataAccessSelectImageResultWrapper res = new SqliteDataAccessSelectImageResultWrapper();

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = String.Format("SELECT * FROM ImageUrl WHERE Image_ID = '{0}'", imageId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //if (reader["Image"] != DBNull.Value)
                            //{
                            byte[] bi = (byte[])reader["Image"];
                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    res.Image = BitmapImageFromBytes(bi);
                                });
                            }
                            //}

                            res.AffectedCount++;
                        }
                    }
                }
            }
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByMultipleFeedIds";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = e.ToString();
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectEntriesByMultipleFeedIds");
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectEntriesByMultipleFeedIds");
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByMultipleFeedIds";

            return res;
        }

        return res;
    }
    */

    public SqliteDataAccessInsertResultWrapper InsertEntries(List<EntryItem> entries)
    {
        SqliteDataAccessInsertResultWrapper res = new SqliteDataAccessInsertResultWrapper();

        if (entries is null)
            return res;

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();
                    try
                    {
                        foreach (var entry in entries)
                        {
                            if (entry is not FeedEntryItem)
                                continue;
                            if ((entry.EntryId == null) || (entry.AltHtmlUri == null))
                                continue;

                            string sqlInsert = "INSERT OR IGNORE INTO Entry (ID, Feed_ID, Entry_ID, Url, Title, Published, Author, Summary, Content, ContentType, ImageUrl, IsImageDownloaded, Image_ID, Status, IsArchived) VALUES (@Id, @feedId, @EntryId, @AltHtmlUri, @Title, @Published, @Author, @Summary, @Content, @ContentType, @ImageUri, @IsImageDownloaded, @ImageId, @Status, @IsArchived)";

                            cmd.CommandText = sqlInsert;

                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@Id", entry.Id);

                            cmd.Parameters.AddWithValue("@feedId", entry.ServiceId);//feedId

                            cmd.Parameters.AddWithValue("@EntryId", entry.EntryId);

                            cmd.Parameters.AddWithValue("@AltHtmlUri", entry.AltHtmlUri.AbsoluteUri);

                            if (entry.Title != null)
                                cmd.Parameters.AddWithValue("@Title", entry.Title);
                            else
                                cmd.Parameters.AddWithValue("@Title", string.Empty);

                            cmd.Parameters.AddWithValue("@Published", entry.Published.ToString("yyyy-MM-dd HH:mm:ss"));

                            if (entry.Author != null)
                                cmd.Parameters.AddWithValue("@Author", entry.Author);
                            else
                                cmd.Parameters.AddWithValue("@Author", string.Empty);

                            if (entry.Summary != null)
                                cmd.Parameters.AddWithValue("@Summary", entry.Summary);
                            else
                                cmd.Parameters.AddWithValue("@Summary", string.Empty);

                            if (entry.Content != null)
                                cmd.Parameters.AddWithValue("@Content", entry.Content);
                            else
                                cmd.Parameters.AddWithValue("@Content", string.Empty);

                            cmd.Parameters.AddWithValue("@ContentType", entry.ContentType.ToString());

                            if (entry.ImageUri != null)
                                cmd.Parameters.AddWithValue("@ImageUri", entry.ImageUri.AbsoluteUri);
                            else
                                cmd.Parameters.AddWithValue("@ImageUri", string.Empty);

                            if (entry.IsImageDownloaded)
                                cmd.Parameters.AddWithValue("@IsImageDownloaded", bool.TrueString);
                            else
                                cmd.Parameters.AddWithValue("@IsImageDownloaded", bool.FalseString);

                            if (entry.ImageId != null)
                                cmd.Parameters.AddWithValue("@ImageId", entry.ImageId);
                            else
                                cmd.Parameters.AddWithValue("@ImageId", string.Empty);

                            if (entry is FeedEntryItem)
                            {
                                cmd.Parameters.AddWithValue("@Status", (entry as FeedEntryItem).Status.ToString());
                                cmd.Parameters.AddWithValue("@IsArchived", bool.FalseString);//(entry as FeedEntryItem).IsArchived.ToString()
                            }

                            var r = cmd.ExecuteNonQuery();

                            if (r > 0)
                            {
                                //c++;
                                res.AffectedCount++;

                                res.InsertedEntries.Add(entry);
                            }
                        }

                        //　コミット
                        cmd.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        cmd.Transaction.Rollback();

                        res.IsError = true;
                        res.Error.ErrType = ErrorObject.ErrTypes.DB;
                        res.Error.ErrCode = "";
                        res.Error.ErrText = "Exception";
                        res.Error.ErrDescription = e.Message;
                        res.Error.ErrDatetime = DateTime.Now;
                        res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                        res.Error.ErrPlaceParent = "DataAccess::InsertEntries";

                        return res;
                    }
                }
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "TargetInvocationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertEntries";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::InsertEntries");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertEntries";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrText = "InnerException";
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                res.Error.ErrText = "Exception";
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::InsertEntries";

            return res;
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    /*
    public SqliteDataAccessInsertResultWrapper InsertImages(List<EntryItem> entries)
    {
        SqliteDataAccessInsertResultWrapper res = new SqliteDataAccessInsertResultWrapper();

        if (entries is null)
            return res;

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();
                    try
                    {
                        foreach (var entry in entries)
                        {
                            if (entry is not FeedEntryItem)
                                continue;
                            if ((entry.EntryId == null) || (entry.AltHtmlUri == null))
                                continue;

                            if ((entry.ImageUri == null) || (entry.ImageByteArray == null) || (entry.ImageByteArray == Array.Empty<byte>()) || entry.IsImageDownloaded)
                                continue;

                            // Insert Image
                            string sqlInsert = "INSERT OR IGNORE INTO Image (Image_ID, ID, Image) VALUES (@ImageId, @Id, @Image)";

                            cmd.CommandText = sqlInsert;

                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@ImageId", entry.Id + ':' + entry.ImageUri.AbsoluteUri);

                            cmd.Parameters.AddWithValue("@Id", entry.Id);
                            if ((entry.ImageByteArray == Array.Empty<byte>()) || entry.ImageByteArray == null)
                            {
                                cmd.Parameters.AddWithValue("@Image", DBNull.Value);
                            }
                            else
                            {
                                SqliteParameter parameter1 = new("@Image", System.Data.DbType.Binary);
                                parameter1.Value = entry.ImageByteArray;
                                cmd.Parameters.Add(parameter1);
                            }

                            var r = cmd.ExecuteNonQuery();

                            if (r > 0)
                            {
                                //c++;
                                res.AffectedCount++;

                                res.InsertedEntries.Add(entry);
                            }

                            // Update Entry IsImageDownloaded
                            string sqlUpdateImageDownloadedFlag = String.Format("UPDATE Entry SET IsImageDownloaded = {1}, Image_ID = {2} WHERE ID = {0}", "@EntryId", "@IsImageDownloaded", "@ImageId");

                            cmd.CommandText = sqlUpdateImageDownloadedFlag;
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@EntryId", entry.Id);

                            (entry as FeedEntryItem).IsImageDownloaded = true;
                            cmd.Parameters.AddWithValue("@IsImageDownloaded", bool.TrueString);

                            cmd.Parameters.AddWithValue("@ImageId", entry.Id + ':' + entry.ImageUri.AbsoluteUri);

                            //res.AffectedCount += cmd.ExecuteNonQuery();
                            cmd.ExecuteNonQuery();
                        }

                        //　コミット
                        cmd.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        cmd.Transaction.Rollback();

                        res.IsError = true;
                        res.Error.ErrType = ErrorObject.ErrTypes.DB;
                        res.Error.ErrCode = "";
                        res.Error.ErrText = "Exception";
                        res.Error.ErrDescription = e.Message;
                        res.Error.ErrDatetime = DateTime.Now;
                        res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                        res.Error.ErrPlaceParent = "DataAccess::InsertImages";

                        return res;
                    }
                }
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "TargetInvocationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertImages";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::InsertImages");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertImages";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrText = "InnerException";
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                res.Error.ErrText = "Exception";
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::InsertImages";

            return res;
        }

        Debug.WriteLine(string.Format("{0} Images Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }
    */

    /*
    public SqliteDataAccessResultWrapper UpdateEntriesAsRead(List<EntryItem> entries)
    {
        SqliteDataAccessResultWrapper res = new SqliteDataAccessResultWrapper();

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();
                    try
                    {
                        foreach (var entry in entries)
                        {
                            if (entry is not FeedEntryItem)
                                continue;

                            string sqlUpdateAsRead = String.Format("UPDATE Entry SET IsArchived = {1} WHERE ID = {0}", "@EntryId", "@IsArchived");

                            cmd.CommandText = sqlUpdateAsRead;
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@EntryId", entry.Id);

                            if (entry is FeedEntryItem)
                            {
                                (entry as FeedEntryItem).IsArchived = true;
                                cmd.Parameters.AddWithValue("@IsArchived", bool.TrueString);
                            }

                            // TODO:
                            res.AffectedCount += cmd.ExecuteNonQuery();
                        }

                        cmd.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        cmd.Transaction.Rollback();

                        res.IsError = true;
                        res.Error.ErrType = ErrorObject.ErrTypes.DB;
                        res.Error.ErrCode = "";
                        res.Error.ErrText = "Exception";
                        res.Error.ErrDescription = e.Message;
                        res.Error.ErrDatetime = DateTime.Now;
                        res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                        res.Error.ErrPlaceParent = "DataAccess::UpdateEntriesAsRead";

                        return res;
                    }
                }
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "TargetInvocationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateEntriesAsRead";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateEntriesAsRead";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrText = "InnerException";
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                res.Error.ErrText = "Exception";
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateEntriesAsRead";

            return res;
        }

        //Debug.WriteLine(string.Format("{0} Entries from {1} Updated as read in the DB", c.ToString(), feedId));

        return res;
    }
    */

    public SqliteDataAccessResultWrapper UpdateAllEntriesAsRead(List<string> feedIds)
    {
        SqliteDataAccessResultWrapper res = new SqliteDataAccessResultWrapper();

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        string before = string.Format("UPDATE Entry SET IsArchived = '{0}' WHERE ", bool.TrueString);

        string middle = "(";

        foreach (var asdf in feedIds)
        {
            if (middle != "(")
                middle = middle + "OR ";

            middle = middle + String.Format("Feed_ID = '{0}' ", asdf);
        }

        string after = string.Format(") AND IsArchived = '{0}'", bool.FalseString);

        //Debug.WriteLine(before + middle + after);

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();

                    try
                    {
                        cmd.CommandText = before + middle + after;

                        cmd.CommandType = CommandType.Text;

                        res.AffectedCount = cmd.ExecuteNonQuery();

                        cmd.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        cmd.Transaction.Rollback();

                        res.IsError = true;
                        res.Error.ErrType = ErrorObject.ErrTypes.DB;
                        res.Error.ErrCode = "";
                        res.Error.ErrText = "Exception";
                        res.Error.ErrDescription = e.Message;
                        res.Error.ErrDatetime = DateTime.Now;
                        res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                        res.Error.ErrPlaceParent = "DataAccess::UpdateAllEntriesAsRead";

                        return res;
                    }
                }
            }
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateAllEntriesAsRead";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrText = "InnerException";
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                res.Error.ErrText = "Exception";
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateAllEntriesAsRead";

            return res;
        }

        //Debug.WriteLine(string.Format("{0} Entries from {1} Updated as read in the DB", c.ToString(), feedId));

        return res;
    }
        
    /*
    public SqliteDataAccessResultWrapper UpdateEntryStatus(EntryItem entry)
    {
        SqliteDataAccessResultWrapper res = new SqliteDataAccessResultWrapper();

        if (entry is not FeedEntryItem)
            return res;

        //int c = 0;

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();
                    try
                    {
                        string sqlUpdateAsRead = String.Format("UPDATE Entry SET Status = {1}, IsArchived = {2} WHERE ID = {0}", "@EntryId", "@Status", "@IsArchived");

                        cmd.CommandText = sqlUpdateAsRead;
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@EntryId", entry.Id);

                        if (entry is FeedEntryItem)
                        {
                            //if ((entry as FeedEntryItem).Status == FeedEntryItem.ReadStatus.rsVisited)
                            //    (entry as FeedEntryItem).IsArchived = true;

                            cmd.Parameters.AddWithValue("@Status", (entry as FeedEntryItem).Status.ToString());
                            cmd.Parameters.AddWithValue("@IsArchived", (entry as FeedEntryItem).IsArchived.ToString());
                        }

                        var r = cmd.ExecuteNonQuery();

                        if (r > 0)
                        {
                            res.AffectedCount++;
                        }

                        cmd.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        cmd.Transaction.Rollback();

                        res.IsError = true;
                        res.Error.ErrType = ErrorObject.ErrTypes.DB;
                        res.Error.ErrCode = "";
                        res.Error.ErrText = "Exception";
                        res.Error.ErrDescription = e.Message;
                        res.Error.ErrDatetime = DateTime.Now;
                        res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                        res.Error.ErrPlaceParent = "DataAccess::UpdateEntryStatus";

                        return res;
                    }
                }
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "TargetInvocationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateEntryStatus";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateEntryStatus";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrText = "InnerException";
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                res.Error.ErrText = "Exception";
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateEntryStatus";

            return res;
        }

        //Debug.WriteLine(string.Format("{0} Entries Status Updated in the DB", c.ToString()));

        return res;
    }
    */

    public SqliteDataAccessResultWrapper DeleteEntriesByFeedIds(List<string> feedIds)
    {
        SqliteDataAccessResultWrapper res = new SqliteDataAccessResultWrapper();

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        //
        string before = "DELETE FROM Entry WHERE ";

        string middle = "(";

        foreach (var asdf in feedIds)
        {
            if (middle != "(")
                middle = middle + "OR ";

            middle = middle + String.Format("Feed_ID = '{0}' ", asdf);
        }

        string after = ")";

        var sqlDelEntries = before + middle + after;

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    //cmd.Transaction = connection.BeginTransaction();

                    cmd.CommandText = sqlDelEntries;

                    res.AffectedCount = cmd.ExecuteNonQuery();

                    //cmd.Transaction.Commit();
                }
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "TargetInvocationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteEntriesByFeedIds";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteEntriesByFeedIds";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = e.ToString();
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::DeleteEntriesByFeedIds");
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::DeleteEntriesByFeedIds");
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteEntriesByFeedIds";

            return res;
        }

        Debug.WriteLine(string.Format("{0} Entries Deleted from DB", res.AffectedCount));

        return res;
    }

    /*
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
    */

    /*
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
    */

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
