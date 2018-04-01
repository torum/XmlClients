/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// 

/// Atom Syndication Format:
///  https://tools.ietf.org/html/rfc4287
/// Atom Publishing protocol:
///  https://tools.ietf.org/html/rfc5023
///  
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XML.Atom
{
    class AtomPublishingProtocol
    {
        //TODO
    }

    class AtomPubCntrol
    {
        public bool Draft;
    }

    class AtomPubEntry : AtomEntry
    {
        public DateTime DateTimeEdited;

        public AtomPubCntrol PubControl;

    }

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

//Service Documents are identified with the "application/atomsvc+xml" media type

/*
<?xml version="1.0" encoding='utf-8'?>
<service xmlns="http://www.w3.org/2007/app"
        xmlns:atom="http://www.w3.org/2005/Atom">
 <workspace>
   <atom:title>Main Site</atom:title>
   <collection
       href="http://example.org/blog/main" >
     <atom:title>My Blog Entries</atom:title>
     <categories
        href="http://example.com/cats/forMain.cats" />
   </collection>
   <collection
       href="http://example.org/blog/pic" >
     <atom:title>Pictures</atom:title>
     <accept>image/png</accept>
     <accept>image/jpeg</accept>
     <accept>image/gif</accept>
   </collection>
 </workspace>
 <workspace>
   <atom:title>Sidebar Blog</atom:title>
   <collection
       href="http://example.org/sidebar/list" >
     <atom:title>Remaindered Links</atom:title>
     <accept>application/atom+xml;type=entry</accept>
     <categories fixed="yes">
       <atom:category
         scheme="http://example.org/extra-cats/"
         term="joke" />
       <atom:category
         scheme="http://example.org/extra-cats/"
         term="serious" />
     </categories>
   </collection>
 </workspace>
</service>
 */


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




//Category Documents are identified with the "application/atomcat+xml" media type

/*
 <?xml version="1.0" ?>
       <app:categories
           xmlns:app="http://www.w3.org/2007/app"
           xmlns:atom="http://www.w3.org/2005/Atom"
           fixed="yes" scheme="http://example.com/cats/big3">
         <atom:category term="animal" />
         <atom:category term="vegetable" />
         <atom:category term="mineral" />
       </app:categories>
     */


///////////////////////////////////////////////////////////////////////////




/*
 type
  TAtomCollection = class(TAtomCommonAttributes)
  private
    FXML:WideString;
  public
    Title:string;
    Href:TAtomUri;
    AcceptMediaRangeList:TStringList; //AcceptMediaRangeList is empty that means nothing allowed to post.
    Categories:TAtomCategories;  //"Create" auto.
    //property  asXMLObj : IXMLDOMDocument2 read getXMLObj;
    function asXMLObj(recurse:boolean=true):IXMLDOMDocument2;
    constructor Create;overload;
    constructor Create(fromXML:WideString; baseUri:string; recurse:boolean=true);overload;
    destructor Destroy; override;
  end;

  TAtomCollectionList = class(TList)
  private
    function Get(Index: Integer): TAtomCollection;
    procedure Put(Index: Integer; const Value: TAtomCollection);
  public
    destructor Destroy; override;
    property Items[Index: Integer]: TAtomCollection read Get write Put; default;
  end;

type
  TAtomWorkspace = class(TAtomCommonAttributes)
  private
    FXML:WideString;
  public
    Title:string;
    Collections:TAtomCollectionList;
    function asXMLObj(recurse:boolean=true):IXMLDOMDocument2;
    constructor Create;overload;
    constructor Create(fromXML:WideString; baseUri:string; recurse:boolean=true);overload;
    destructor Destroy; override;
  end;

  TAtomWorkspaceList = class(TList)
  private
    function Get(Index: Integer): TAtomWorkspace;
    procedure Put(Index: Integer; const Value: TAtomWorkspace);
  public
    destructor Destroy; override;
    property Items[Index: Integer]: TAtomWorkspace read Get write Put; default;
  end;

type
  TAtomService = class(TAtomCommonAttributes)
  private
    FXML:WideString;
  public
    Workspaces:TAtomWorkspaceList;
    function asXMLObj(recurse:boolean=true):IXMLDOMDocument2;
    constructor Create();overload;
    constructor Create(fromXML:WideString; baseUri:string; recurse:boolean=true);overload;
    destructor Destroy; override;
  end;     
     */
