/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogWrite.Models
{
    /// <summary>
    /// TODO
    /// </summary>
    class Discovery
    {

        /*
         type TAtomType =(atomFeed,atomPub,atomGData,atomAPI);

         */

    }



    /*
    <?xml version="1.0" encoding="UTF-8"?>
    <rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
      <service>
        <engineName>WordPress</engineName>
        <engineLink>https://wordpress.org/</engineLink>
        <homePageLink>http://1270.0.0.1</homePageLink>
        <apis>
          <api name="WordPress" blogID="1" preferred="true" apiLink="http://1270.0.0.1/xmlrpc.php" />
          <api name="Movable Type" blogID="1" preferred="false" apiLink="http://1270.0.0.1/xmlrpc.php" />
          <api name="MetaWeblog" blogID="1" preferred="false" apiLink="http://1270.0.0.1xmlrpc.php" />
          <api name="Blogger" blogID="1" preferred="false" apiLink="http://1270.0.0.1/xmlrpc.php" />
          <api name="WP-API" blogID="1" preferred="false" apiLink="http://1270.0.0.1/wp-json/" />
        </apis>
      </service>
    </rsd>
    */

    // HTML Head

    /////////////////////////////////////////////////////////////////////////

    /*
    AtomPub
    <link rel="SERVICE" type="application/atomsvc+xml" title="Atom" href="http://1270.0.0.1/app" />
    */

    /*
    RSD XML-RPC or AtomAPI
    <link rel="EditURI" type="application/rsd+xml" title="RSD" href="http://1270.0.0.1/rsd" />
    */

    /////////////////////////////////////////////////////////////////////////

    // Service documents

    // AtomAPI_NS='http://purl.org/atom/ns#';
    // Atom_NS='http://www.w3.org/2005/Atom';
    // Atom_APP_NS='http://www.w3.org/2007/app';

    /////////////////////////////////////////////////////////////////////////

    // AtomPub at wordpress
    // Content-Type: application/atomsvc+xml

    /*
    <?xml version="1.0" encoding="utf-8"?>
    <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
      <workspace>
        <atom:title>WordPress Workspace</atom:title>
        <collection href="http://feeds.jp/wp/wp-app.php/posts">
          <atom:title>WordPress Posts</atom:title>
          <accept>application/atom+xml;type=entry</accept>
          <categories href="http://feeds.jp/wp/wp-app.php/categories" />
        </collection>
        <collection href="http://feeds.jp/wp/wp-app.php/attachments">
          <atom:title>WordPress Media</atom:title>
          <accept>image/*</accept><accept>audio/*</accept><accept>video/*</accept>
        </collection>
      </workspace>
    </service>
    */

    // AtomPub at hatena
    // Content-Type: application/atomsvc+xml

    /*
    <?xml version="1.0" encoding="utf-8"?>
    <service xmlns="http://www.w3.org/2007/app">
      <workspace>
        <atom:title xmlns:atom="http://www.w3.org/2005/Atom">hoge</atom:title>
        <collection href="https://127.0.0.1/atom/entry">
          <atom:title xmlns:atom="http://www.w3.org/2005/Atom">fuga</atom:title>
          <accept>application/atom+xml;type=entry</accept>
        </collection>
      </workspace>
    </service>
     */

    // RSD XML-RPC or AtomAPI
    // Content-Type: application/rsd+xml

    /*
    <?xml version="1.0" encoding="UTF-8"?>
    <rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
      <service>
        <engineName>WordPress</engineName>
        <engineLink>https://wordpress.org/</engineLink>
        <homePageLink>http://1270.0.0.1</homePageLink>
        <apis>
          <api name="WordPress" blogID="1" preferred="true" apiLink="http://1270.0.0.1/xmlrpc.php" />
          <api name="Movable Type" blogID="1" preferred="false" apiLink="http://1270.0.0.1/xmlrpc.php" />
          <api name="MetaWeblog" blogID="1" preferred="false" apiLink="http://1270.0.0.1xmlrpc.php" />
          <api name="Blogger" blogID="1" preferred="false" apiLink="http://1270.0.0.1/xmlrpc.php" />
          <api name="WP-API" blogID="1" preferred="false" apiLink="http://1270.0.0.1/wp-json/" />
        </apis>
      </service>
    </rsd>
    */

    // AtomAPI at vox
    // Content-Type: application/atom+xml

    /*
    <?xml version="1.0" encoding="utf-8"?>
    <feed xmlns="http://purl.org/atom/ns#">
        <link xmlns="http://purl.org/atom/ns#" rel="service.post" href="http://www.vox.com/services/atom/svc=post/collection_id=6a00c2251f52cd549d00c2251f5478604a" title="blog" type="application/x.atom+xml"/>
        <link xmlns="http://purl.org/atom/ns#" rel="alternate" href="http://marumoto.vox.com/" title="blog" type="text/html"/>
        <link xmlns="http://purl.org/atom/ns#" rel="service.feed" href="http://www.vox.com/services/atom/svc=asset/6p00c2251f52cd549d" title="blog" type="application/atom+xml"/>
        <link xmlns="http://purl.org/atom/ns#" rel="service.upload" href="http://www.vox.com/services/atom/svc=asset" title="blog" type="application/atom+xml"/>
        <link xmlns="http://purl.org/atom/ns#" rel="replies" href="http://www.vox.com/services/atom/svc=asset/6p00c2251f52cd549d/type=Comment" title="blog" type="application/atom+xml"/>
    </feed>
    */

    // AtomAPI at livedoor http://cms.blog.livedoor.com/atom
    // Content-Type: application/x.atom+xml

    /*
    <?xml version="1.0" encoding="UTF-8"?>
    <feed xmlns="http://purl.org/atom/ns#">
        <link xmlns="http://purl.org/atom/ns#" type="application/x.atom+xml" rel="service.post" href="http://cms.blog.livedoor.com/atom/blog_id=95864" title="hepcat de　ブログ"/>
        <link xmlns="http://purl.org/atom/ns#" type="application/x.atom+xml" rel="service.feed" href="http://cms.blog.livedoor.com/atom/blog_id=95864" title="hepcat de　ブログ"/>
        <link xmlns="http://purl.org/atom/ns#" type="application/x.atom+xml" rel="service.categories" href="http://cms.blog.livedoor.com/atom/blog_id=95864/svc=categories" title="hepcat de　ブログ"/>
        <link xmlns="http://purl.org/atom/ns#" type="application/x.atom+xml" rel="service.upload" href="http://cms.blog.livedoor.com/atom/blog_id=95864/svc=upload" title="hepcat de　ブログ"/>
    </feed>
    */





}
