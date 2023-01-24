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
using AngleSharp.Dom;
using static BlogWrite.Models.FeedLink;

namespace BlogWrite.Services;

internal class DataAccessService : IDataAccessService
{
    private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new();

    public SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath)
    {
        var res = new SqliteDataAccessResultWrapper();

        connectionStringBuilder.DataSource = dataBaseFilePath;
        connectionStringBuilder.ForeignKeys = true;

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
                        tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS feeds (" +
                            "feed_id TEXT NOT NULL PRIMARY KEY," +
                            "url TEXT NOT NULL," +
                            "name TEXT NOT NULL," +
                            "title TEXT," +
                            "description TEXT," +
                            "updated TEXT," +
                            "html_url TEXT" +
                            ")";
                        tableCmd.ExecuteNonQuery();

                        tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS entries (" +
                            "entry_id TEXT NOT NULL PRIMARY KEY," +
                            "feed_id TEXT NOT NULL," +
                            "url TEXT NOT NULL," +
                            "title TEXT," +
                            "published TEXT NOT NULL," +
                            "updated TEXT," +
                            "author TEXT," +
                            "summary TEXT," +
                            "content TEXT," +
                            "content_type TEXT," +
                            "image_id TEXT," +
                            "status TEXT," +
                            "archived TEXT," +
                            "CONSTRAINT fk_feeds FOREIGN KEY (feed_id) REFERENCES feeds(feed_id) ON DELETE CASCADE" +
                            ")";
                        tableCmd.ExecuteNonQuery();

                        tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS images (" +
                            "image_id TEXT NOT NULL PRIMARY KEY," +
                            "entry_id TEXT NOT NULL," +
                            "image_url TEXT," +
                            "image_downloaded TEXT," +
                            "image BLOB," +
                            "CONSTRAINT fk_entries FOREIGN KEY (entry_id) REFERENCES entries(entry_id) ON DELETE CASCADE" +
                            ")";
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

                return res;
            }
        }

        return res;
    }

    public SqliteDataAccessResultWrapper InsertFeed(string feedId, Uri feedUri, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri htmlUri)
    {
        var res = new SqliteDataAccessResultWrapper();

        if (string.IsNullOrEmpty(feedId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

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
                        cmd.CommandText = "INSERT OR IGNORE INTO feeds (feed_id, url, name, title, description, updated, html_url) VALUES (@FeedId, @Uri, @Name, @Title, @Description, @Updated, @HtmlUri)";
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@FeedId", feedId);
                        cmd.Parameters.AddWithValue("@Uri", feedUri.AbsoluteUri);
                        cmd.Parameters.AddWithValue("@Name", feedName);
                        cmd.Parameters.AddWithValue("@Title", feedTitle);
                        cmd.Parameters.AddWithValue("@Description", feedDescription);
                        cmd.Parameters.AddWithValue("@Updated", updated.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (htmlUri != null)
                        {
                            cmd.Parameters.AddWithValue("@HtmlUri", htmlUri.AbsoluteUri);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@HtmlUri", "");
                        }

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
                        res.Error.ErrPlaceParent = "DataAccess::InsertFeed";

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
            res.Error.ErrPlaceParent = "DataAccess::InsertFeed";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::InsertFeed");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertFeed";

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
            res.Error.ErrPlaceParent = "DataAccess::InsertFeed";

            return res;
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    public SqliteDataAccessResultWrapper DeleteFeed(string feedId)
    {
        var res = new SqliteDataAccessResultWrapper();
        
        if (string.IsNullOrEmpty(feedId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        try
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = String.Format("DELETE FROM feeds WHERE feed_id = '{0}';", feedId);

                    //cmd.Transaction = connection.BeginTransaction();

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
            res.Error.ErrPlaceParent = "DataAccess::DeleteFeed";

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
            res.Error.ErrPlaceParent = "DataAccess::DeleteFeed";

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
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::DeleteFeed");
                res.Error.ErrDescription = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::DeleteFeed");
                res.Error.ErrDescription = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteFeed";

            return res;
        }

        Debug.WriteLine(string.Format("{0} feed Deleted from DB", res.AffectedCount));

        return res;
    }

    // not really used.
    public SqliteDataAccessResultWrapper UpdateFeed(string feedId, Uri feedUri, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri htmlUri)
    {
        var res = new SqliteDataAccessResultWrapper();

        if (string.IsNullOrEmpty(feedId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

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
                        var sql = "UPDATE feeds SET ";
                        sql += String.Format("name = '{0}', ", escapeSingleQuote(feedName));
                        sql += String.Format("title = '{0}', ", escapeSingleQuote(feedTitle));
                        sql += String.Format("description = '{0}', ", escapeSingleQuote(feedDescription));
                        sql += String.Format("updated = '{0}'", updated.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (htmlUri != null)
                        {
                            sql += String.Format(", html_url = '{0}'", htmlUri.AbsoluteUri);
                        }
                        sql += String.Format(" WHERE feed_id = '{0}'; ", feedId);

                        cmd.CommandText = sql;

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
                        res.Error.ErrPlaceParent = "DataAccess::UpdateFeed";

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
            res.Error.ErrPlaceParent = "DataAccess::UpdateFeed";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::UpdateFeed");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = "InvalidOperationException";
            res.Error.ErrDescription = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateFeed";

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
            res.Error.ErrPlaceParent = "DataAccess::UpdateFeed";

            return res;
        }

        //Debug.WriteLine(string.Format("{0} feed updated", res.AffectedCount.ToString()));

        return res;
    }

    public SqliteDataAccessSelectResultWrapper SelectEntriesByFeedId(string feedId, bool IsUnarchivedOnly = true)
    {
        var res = new SqliteDataAccessSelectResultWrapper();

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
                        cmd.CommandText = String.Format("SELECT * FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' AND archived = '{1}' ORDER BY published DESC LIMIT 1000", feedId, bool.FalseString);
                    }
                    else
                    {
                        cmd.CommandText = String.Format("SELECT * FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' ORDER BY published DESC LIMIT 10000", feedId);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeedEntryItem entry = new FeedEntryItem(Convert.ToString(reader["title"]), feedId, null);

                            //entry.MyNodeFeed = ndf;

                            entry.EntryId = Convert.ToString(reader["entry_id"]);

                            var s = Convert.ToString(reader["url"]);
                            if (!string.IsNullOrEmpty(s))
                                entry.AltHtmlUri = new Uri(s);

                            entry.Published = DateTime.Parse(Convert.ToString(reader["published"]));

                            entry.Summary = Convert.ToString(reader["summary"]);

                            entry.Content = Convert.ToString(reader["content"]);

                            // TODO:
                            entry.ContentType = EntryItem.ContentTypes.textHtml;

                            /*
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["image_url"])))
                                entry.ImageUri = new Uri(Convert.ToString(reader["image_url"]));
                            */
                            /*
                            string bln = Convert.ToString(reader["IsImageDownloaded"]);
                            if (!string.IsNullOrEmpty(bln))
                            {
                                if (bln == bool.TrueString)
                                    entry.IsImageDownloaded = true;
                                else
                                    entry.IsImageDownloaded = false;
                            }
                            */

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
                            entry.ImageId = Convert.ToString(reader["image_id"]);


                            string status = Convert.ToString(reader["status"]);
                            if (!string.IsNullOrEmpty(status))
                                entry.Status = entry.StatusTextToType(status);

                            entry.Author = Convert.ToString(reader["author"]);

                            string blnstr = Convert.ToString(reader["archived"]);
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

                            entry.FeedTitle = Convert.ToString(reader["name"]);

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

        string before = "SELECT * FROM entries INNER JOIN feeds USING (feed_id) WHERE ";

        string middle = "(";

        foreach (var asdf in feedIds)
        {
            if (middle != "(")
                middle += "OR ";

            middle += String.Format("feed_id = '{0}' ", asdf);
        }

        //string after = string.Format(") AND IsArchived = '{0}' ORDER BY Published DESC LIMIT 1000", bool.FalseString);
        string after;
        if (IsUnarchivedOnly)
        {
            after = string.Format(") AND archived = '{0}' ORDER BY published DESC LIMIT 1000", bool.FalseString);
        }
        else
        {
            after = string.Format(") ORDER BY published DESC LIMIT 10000");
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
                            FeedEntryItem entry = new FeedEntryItem(Convert.ToString(reader["title"]), Convert.ToString(reader["feed_id"]), null);

                            //entry.MyNodeFeed = ndf;

                            entry.EntryId = Convert.ToString(reader["entry_id"]);

                            if (!string.IsNullOrEmpty(Convert.ToString(reader["url"])))
                                entry.AltHtmlUri = new Uri(Convert.ToString(reader["url"]));

                            entry.Published = DateTime.Parse(Convert.ToString(reader["published"]));

                            entry.Summary = Convert.ToString(reader["summary"]);

                            entry.Content = Convert.ToString(reader["content"]);

                            // TODO:
                            entry.ContentType = EntryItem.ContentTypes.textHtml;

                            /*
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["image_url"])))
                                entry.ImageUri = new Uri(Convert.ToString(reader["image_url"]));

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
                            /*
                            string bln = Convert.ToString(reader["IsImageDownloaded"]);
                            if (!string.IsNullOrEmpty(bln))
                            {
                                if (bln == bool.TrueString)
                                    entry.IsImageDownloaded = true;
                                else
                                    entry.IsImageDownloaded = false;
                            }
                            */

                            entry.ImageId = Convert.ToString(reader["image_id"]);

                            string status = Convert.ToString(reader["status"]);
                            if (!string.IsNullOrEmpty(status))
                                entry.Status = entry.StatusTextToType(status);

                            entry.Author = Convert.ToString(reader["author"]);

                            string blnstr = Convert.ToString(reader["archived"]);
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

                            entry.FeedTitle = Convert.ToString(reader["name"]);

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

    public SqliteDataAccessInsertResultWrapper InsertEntries(List<EntryItem> entries, string feedId, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri htmlUri)
    {
        var res = new SqliteDataAccessInsertResultWrapper();

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
                        // update feed info.
                        string sql = "UPDATE feeds SET ";
                        sql += String.Format("name = '{0}', ", escapeSingleQuote(feedName));
                        sql += String.Format("title = '{0}', ", escapeSingleQuote(feedTitle));
                        sql += String.Format("description = '{0}', ", escapeSingleQuote(feedDescription));
                        sql += String.Format("updated = '{0}'", updated.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (htmlUri != null)
                        {
                            sql += String.Format(", html_url = '{0}'", htmlUri.AbsoluteUri);
                        }
                        sql += String.Format(" WHERE feed_id = '{0}'; ", feedId);

                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        foreach (var entry in entries)
                        {
                            if (entry is not FeedEntryItem)
                                continue;
                            if ((entry.EntryId == null) || (entry.AltHtmlUri == null))
                                continue;

                            string sqlInsert = "INSERT OR IGNORE INTO entries (entry_id, feed_id, url, title, published, updated, author, summary, content, content_type, image_id, status, archived) VALUES (@EntryId, @FeedId, @AltHtmlUri, @Title, @Published, @Updated, @Author, @Summary, @Content, @ContentType, @ImageId, @Status, @IsArchived)";

                            cmd.CommandText = sqlInsert;

                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Clear();

                            //cmd.Parameters.AddWithValue("@Id", entry.Id);

                            cmd.Parameters.AddWithValue("@FeedId", entry.ServiceId);//feedId

                            //cmd.Parameters.AddWithValue("@EntryId", entry.EntryId);
                            cmd.Parameters.AddWithValue("@EntryId", entry.Id);

                            cmd.Parameters.AddWithValue("@AltHtmlUri", entry.AltHtmlUri.AbsoluteUri);

                            if (entry.Title != null)
                                cmd.Parameters.AddWithValue("@Title", entry.Title);
                            else
                                cmd.Parameters.AddWithValue("@Title", string.Empty);

                            cmd.Parameters.AddWithValue("@Published", entry.Published.ToString("yyyy-MM-dd HH:mm:ss"));

                            cmd.Parameters.AddWithValue("@Updated", entry.Updated.ToString("yyyy-MM-dd HH:mm:ss"));

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
                            /*
                            if (entry.ImageUri != null)
                                cmd.Parameters.AddWithValue("@ImageUri", entry.ImageUri.AbsoluteUri);
                            else
                                cmd.Parameters.AddWithValue("@ImageUri", string.Empty);
                            
                            if (entry.IsImageDownloaded)
                                cmd.Parameters.AddWithValue("@IsImageDownloaded", bool.TrueString);
                            else
                                cmd.Parameters.AddWithValue("@IsImageDownloaded", bool.FalseString);
                            */
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

    public SqliteDataAccessResultWrapper UpdateAllEntriesAsArchived(List<string> feedIds)
    {
        SqliteDataAccessResultWrapper res = new SqliteDataAccessResultWrapper();

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        string before = string.Format("UPDATE entries SET archived = '{0}' WHERE ", bool.TrueString);

        string middle = "(";

        foreach (var asdf in feedIds)
        {
            if (middle != "(")
                middle = middle + "OR ";

            middle = middle + String.Format("feed_id = '{0}' ", asdf);
        }

        string after = string.Format(") AND archived = '{0}'", bool.FalseString);

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

    // not really used.
    public SqliteDataAccessResultWrapper DeleteEntriesByFeedIds(List<string> feedIds)
    {
        var res = new SqliteDataAccessResultWrapper();

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        var sqlDelEntries = string.Empty;

        if (feedIds.Count > 1)
        {
            string before = "DELETE FROM entries WHERE ";
            //var before = "DELETE FROM feeds WHERE ";
            var middle = "(";

            foreach (var asdf in feedIds)
            {
                if (middle != "(")
                    middle = middle + "OR ";

                middle += String.Format("feed_id = '{0}' ", asdf);
            }

            var after = ")";

            sqlDelEntries = before + middle + after;
        }
        else
        {
            sqlDelEntries = String.Format("DELETE FROM entries WHERE feed_id = '{0}';", feedIds[0]);
            //sqlDelEntries = String.Format("DELETE FROM feeds WHERE feed_id = '{0}';", feedIds[0]);
        }

        Debug.WriteLine(sqlDelEntries);

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
    private bool ColumnExists(IDataRecord dr, string columnName)
    {
        for (var i = 0; i < dr.FieldCount; i++)
        {
            if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false; ;
    }

    private string escapeSingleQuote(string s)
    {
        return s is null ? string.Empty : s.Replace("'", "''");
    }
}
