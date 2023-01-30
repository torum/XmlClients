using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.Core.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using BlogWrite.Core.Contracts.Services;
using AngleSharp.Dom;
using static BlogWrite.Core.Models.FeedLink;

namespace BlogWrite.Core.Services;

public class DataAccessService : IDataAccessService
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
                            "category TEXT," +
                            "summary TEXT," +
                            "content TEXT," +
                            "content_type TEXT," +
                            "source TEXT," +
                            "source_url TEXT," +
                            "image_id TEXT," +
                            "image_url TEXT," +
                            "status TEXT," +
                            "archived TEXT," +
                            "CONSTRAINT fk_feeds FOREIGN KEY (feed_id) REFERENCES feeds(feed_id) ON DELETE CASCADE" +
                            ")";
                        tableCmd.ExecuteNonQuery();

                        tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS images (" +
                            "image_id TEXT NOT NULL PRIMARY KEY," +
                            "entry_id TEXT NOT NULL," +
                            "image_url TEXT," +
                            //"image_downloaded TEXT," +
                            "image BLOB," +
                            "CONSTRAINT fk_entries FOREIGN KEY (entry_id) REFERENCES entries(entry_id) ON DELETE CASCADE" +
                            ")";
                        tableCmd.ExecuteNonQuery();

                        //tableCmd.CommandText = "ALTER TABLE entries ADD COLUMN category TEXT;";
                        //tableCmd.ExecuteNonQuery();

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

    // Not really used because of updates on InsertEntries.
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
                        cmd.ExecuteNonQuery();

                        foreach (var entry in entries)
                        {
                            if (entry is not FeedEntryItem)
                                continue;
                            if ((entry.EntryId == null) || (entry.AltHtmlUri == null))
                                continue;

                            var sqlInsert = "INSERT OR IGNORE INTO entries (entry_id, feed_id, url, title, published, updated, author, category, summary, content, content_type, image_url, source, source_url, status, archived) VALUES (@EntryId, @FeedId, @AltHtmlUri, @Title, @Published, @Updated, @Author, @Category, @Summary, @Content, @ContentType, @ImageUri, @Source, @SourceUri, @Status, @IsArchived)";

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

                            if (entry.Category != null)
                                cmd.Parameters.AddWithValue("@Category", entry.Category);
                            else
                                cmd.Parameters.AddWithValue("@Category", string.Empty);

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
                            if (entry.IsImageDownloaded)
                                cmd.Parameters.AddWithValue("@IsImageDownloaded", bool.TrueString);
                            else
                                cmd.Parameters.AddWithValue("@IsImageDownloaded", bool.FalseString);
                            */

                            if (entry.ImageUri != null)
                                cmd.Parameters.AddWithValue("@ImageUri", entry.ImageUri.AbsoluteUri);
                            else
                                cmd.Parameters.AddWithValue("@ImageUri", string.Empty);

                            if (entry is FeedEntryItem fei)
                            {
                                if (fei.Source != null)
                                    cmd.Parameters.AddWithValue("@Source", fei.Source);
                                else
                                    cmd.Parameters.AddWithValue("@Source", string.Empty);

                                if (fei.SourceUri != null)
                                    cmd.Parameters.AddWithValue("@SourceUri", fei.SourceUri.AbsoluteUri);
                                else
                                    cmd.Parameters.AddWithValue("@SourceUri", string.Empty);

                                cmd.Parameters.AddWithValue("@Status", fei.Status.ToString());
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
                        //cmd.CommandText = String.Format("SELECT * FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' AND archived = '{1}' ORDER BY published DESC LIMIT 1000", feedId, bool.FalseString);

                        cmd.CommandText = String.Format("SELECT feeds.name as feedName, entries.title as entryTitle, entries.entry_id as entryId, entries.url as entryUrl, entries.published as entryPublished, entries.summary as entrySummary, entries.content as entryContent, entries.content_type as entryContentType, entries.image_url as entryImageUri, entries.source as entrySource, entries.source_url as entrySourceUri, entries.author as entryAuthor, entries.category as entryCategory, entries.archived as entryArchived FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' AND archived = '{1}' ORDER BY published DESC LIMIT 1000", feedId, bool.FalseString);
                    }
                    else
                    {
                        cmd.CommandText = String.Format("SELECT feeds.name as feedName, entries.title as entryTitle, entries.entry_id as entryId, entries.url as entryUrl, entries.published as entryPublished, entries.summary as entrySummary, entries.content as entryContent, entries.content_type as entryContentType, entries.image_url as entryImageUri, entries.source as entrySource, entries.source_url as entrySourceUri, entries.author as entryAuthor, entries.category as entryCategory, entries.archived as entryArchived FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' ORDER BY published DESC LIMIT 10000", feedId);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeedEntryItem entry = new FeedEntryItem(Convert.ToString(reader["entryTitle"]), feedId, null);

                            //entry.MyNodeFeed = ndf;

                            entry.EntryId = Convert.ToString(reader["entryId"]);

                            var s = Convert.ToString(reader["entryUrl"]);
                            if (!string.IsNullOrEmpty(s))
                                entry.AltHtmlUri = new Uri(s);

                            entry.Published = DateTime.Parse(Convert.ToString(reader["entryPublished"]));

                            entry.Summary = Convert.ToString(reader["entrySummary"]);

                            entry.Content = Convert.ToString(reader["entryContent"]);

                            var t = Convert.ToString(reader["entryContentType"]);
                            if (t == "textHtml")
                            {
                                entry.ContentType = EntryItem.ContentTypes.textHtml;
                            }
                            else if (t == "text")
                            {
                                entry.ContentType = EntryItem.ContentTypes.text;
                            }
                            else
                            {
                                // TODO:
                                entry.ContentType = EntryItem.ContentTypes.unknown;
                            }


                            entry.Source= Convert.ToString(reader["entrySource"]);

                            var su = Convert.ToString(reader["entrySourceUri"]);
                            if (!string.IsNullOrEmpty(su))
                            {
                                entry.SourceUri = new Uri(su);
                            }


                            var u = Convert.ToString(reader["entryImageUri"]);
                            if (!string.IsNullOrEmpty(u))
                            {
                                entry.ImageUri = new Uri(u);
                            }

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
                            //entry.ImageId = Convert.ToString(reader["image_id"]);

                            /*
                            var status = Convert.ToString(reader["status"]);
                            if (!string.IsNullOrEmpty(status))
                                entry.Status = entry.StatusTextToType(status);
                            */

                            entry.Author = Convert.ToString(reader["entryAuthor"]);

                            entry.Category = Convert.ToString(reader["entryCategory"]);

                            var blnstr = Convert.ToString(reader["entryArchived"]);
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

                            //entry.Publisher = Convert.ToString(reader["name"]);

                            entry.FeedTitle = Convert.ToString(reader["feedName"]);

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

        var before = "SELECT feeds.name as feedName, feeds.feed_id as feedId, entries.title as entryTitle, entries.entry_id as entryId, entries.url as entryUrl, entries.published as entryPublished, entries.summary as entrySummary, entries.content as entryContent, entries.content_type as entryContentType, entries.image_url as entryImageUri, entries.source as entrySource, entries.source_url as entrySourceUri, entries.author as entryAuthor, entries.category as entryCategory, entries.archived as entryArchived FROM entries INNER JOIN feeds USING (feed_id) WHERE ";

        var middle = "(";

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
                            FeedEntryItem entry = new FeedEntryItem(Convert.ToString(reader["entryTitle"]), Convert.ToString(reader["feedId"]), null);

                            //entry.MyNodeFeed = ndf;

                            entry.EntryId = Convert.ToString(reader["entryId"]);

                            //if (!string.IsNullOrEmpty(Convert.ToString(reader["entryUrl"])))
                            //    entry.AltHtmlUri = new Uri(Convert.ToString(reader["entryUrl"]));

                            var s = Convert.ToString(reader["entryUrl"]);
                            if (!string.IsNullOrEmpty(s))
                            {
                                entry.AltHtmlUri = new Uri(s);
                            }

                            entry.Published = DateTime.Parse(Convert.ToString(reader["entryPublished"]));

                            entry.Summary = Convert.ToString(reader["entrySummary"]);

                            entry.Content = Convert.ToString(reader["entryContent"]);

                            var t = Convert.ToString(reader["entryContentType"]);
                            if (t == "textHtml")
                            {
                                entry.ContentType = EntryItem.ContentTypes.textHtml;
                            }
                            else if (t == "text")
                            {
                                entry.ContentType = EntryItem.ContentTypes.text;
                            }
                            else
                            {
                                // TODO:
                                entry.ContentType = EntryItem.ContentTypes.unknown;
                            }

                            var u = Convert.ToString(reader["entryImageUri"]);
                            if (!string.IsNullOrEmpty(u))
                            {
                                entry.ImageUri = new Uri(u);
                            }

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

                            //entry.ImageId = Convert.ToString(reader["image_id"]);

                            /*
                            string status = Convert.ToString(reader["status"]);
                            if (!string.IsNullOrEmpty(status))
                                entry.Status = entry.StatusTextToType(status);
                            */

                            entry.Source = Convert.ToString(reader["entrySource"]);

                            var su = Convert.ToString(reader["entrySourceUri"]);
                            if (!string.IsNullOrEmpty(su))
                            {
                                entry.SourceUri = new Uri(su);
                            }

                            entry.Author = Convert.ToString(reader["entryAuthor"]);

                            entry.Category = Convert.ToString(reader["entryCategory"]);

                            var blnstr = Convert.ToString(reader["entryArchived"]);
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

                            entry.FeedTitle = Convert.ToString(reader["feedName"]);

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

    public SqliteDataAccessResultWrapper UpdateAllEntriesAsArchived(List<string> feedIds)
    {
        SqliteDataAccessResultWrapper res = new SqliteDataAccessResultWrapper();

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        var before = string.Format("UPDATE entries SET archived = '{0}' WHERE ", bool.TrueString);

        var middle = "(";

        foreach (var asdf in feedIds)
        {
            if (middle != "(")
                middle += "OR ";

            middle += String.Format("feed_id = '{0}' ", asdf);
        }

        var after = string.Format(") AND archived = '{0}'", bool.FalseString);

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

    // Not really used because of "ON DELETE CASCADE".
    public SqliteDataAccessResultWrapper DeleteEntriesByFeedIds(List<string> feedIds)
    {
        var res = new SqliteDataAccessResultWrapper();

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        string sqlDelEntries;// = string.Empty;

        if (feedIds.Count > 1)
        {
            var before = "DELETE FROM entries WHERE ";
            //var before = "DELETE FROM feeds WHERE ";
            var middle = "(";

            foreach (var asdf in feedIds)
            {
                if (middle != "(")
                    middle += "OR ";

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
