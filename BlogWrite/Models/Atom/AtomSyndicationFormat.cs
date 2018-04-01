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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.Atom
{
    class AtomSyndicationFormat
    {
        //TODO
    }

    public enum TextType { ttText, ttHtml, ttXhtml, ttOther }

    abstract class AtomCommonAttributes
    {
        private string _base;
        private string _parentBase;
        private string _xml;
    }

    abstract class AtomUri : AtomCommonAttributes
    {
        Uri Uri;
    }

    abstract class AtomTextConstruct : AtomCommonAttributes
    {
        public TextType TextType;
        public string Text;
    }

    class AtomPerson : AtomCommonAttributes
    {
        public string Name;
        public AtomUri Uri;
        public string Email;

    }

    class AtomPersonList : Collection<AtomPerson>
    {

    }

    class AtomCategory : AtomCommonAttributes
    {
        public string Term;
        public string CategoryLabel;
        public AtomUri Scheme;
    }

    class AtomCategoryList : Collection<AtomCategory>
    {

    }

    class TAtomCategories : AtomCommonAttributes
    {
        public bool Fixed;
        public AtomUri Scheme;
        public AtomUri Href;
        public AtomCategoryList CategoryList;

    }

    class AtomContent : AtomCommonAttributes
    {
        public string ContentType;
        public string Text;
        public AtomUri Src;
    }

    class AtomLink : AtomCommonAttributes
    {
        public string Rel;
        public string ContentType;
        public AtomUri Href;
        public string Length;
        public string Title;
        public string Hreflang;

    }

    class AtomLinkList : Collection<AtomLink>
    {

    }

    class AtomEntry : AtomCommonAttributes
    {
        public string Id;
        public AtomTextConstruct Title;
        public AtomLinkList Links;
        public AtomPersonList Authors;
        public AtomPersonList Contributors;
        public AtomContent Content;
        public AtomTextConstruct Summary;
        public AtomCategoryList Categories;
        public AtomTextConstruct Rights;
        public string Source;
        public DateTime DateTimeUpdated;
        public DateTime DateTimePublished;

        //function getXMLObj:IXMLDOMDocument2;
        //function getEditUri:string;
        //function getEditMediaUri:string;
        //procedure setEditUri(strUri:string);
        //property  EditUri : string read getEditUri write setEditUri;
        //property EditMediaUri : string read getEditMediaUri;
        //property AsXMLDoc : IXMLDOMDocument2 read getXMLObj;
    }

    class AtomEntryList : Collection<AtomEntry>
    {

    }
    

    class AtomFeed : AtomCommonAttributes
    {
        public string Id;
        public AtomTextConstruct Title;
        public AtomTextConstruct SubTitle;
        public AtomLinkList Links;
        public string Generator;
        public AtomUri Icon;
        public AtomUri Logo;
        public AtomEntryList Entries;
        public AtomPersonList Authors;
        public AtomPersonList Contributors;
        public AtomCategoryList Categories;
        public DateTime DateTimeUpdated;
        public AtomTextConstruct Rights;


        //function getFirstUri:string;
        //function getPrevUri:string;
        //function getNextUri:string;
        //function getLastUri:string;
        //function getSelfUri:string;
        //property PrevUri : string read getPrevUri;
        //property NextUri : string read getNextUri;
        //property FirstUri : string read getFirstUri;
        //property LastUri : string read getLastUri;
        //property SelfUri : string read getSelfUri;
    }


}


/*
 type
  TTextType =(ttText,ttHtml,ttXhtml,ttOther);

type
  TAtomCommonAttributes = class(TObject)
  private
    FBase:string;
    FParentBase:string;
    function CalculateUri(baseUri:string; partsUri:string):string;
    function getBase:string;
  public
    Lang:string;
  published
    property parentBase:string read FParentBase write FParentBase;
    property Base : string read getBase write FBase;
  end;

type
  TAtomUri = class(TAtomCommonAttributes)
  private
    FUri:string;
    function getAbsoluteUri:string; overload;
  published
    constructor Create(BaseUri:string);
    property  Uri : string read getAbsoluteUri write FUri;
  end;

type
  TAtomTextConstruct = class(TAtomCommonAttributes)
  public
    TextType:TTextType;
    Text:WideString;
    constructor Create;
  end;

type
  TAtomPerson = class(TAtomCommonAttributes)
  private
    FXML:WideString;
  public
    Name:WideString;
    Uri:TAtomUri;
    Email:WideString;
    function asXMLObj(documentElementName:string):IXMLDOMDocument2;
    constructor Create;
    //TODO:constructor Create(fromXML:WideString; baseUri:string);overload;
    destructor Destroy; override;
  end;

  TAtomPersonList = class(TList)
  private
    function Get(Index: Integer): TAtomPerson;
    procedure Put(Index: Integer; const Value: TAtomPerson);
  public
    destructor Destroy; override;
    property Items[Index: Integer]: TAtomPerson read Get write Put; default;
  end;

type
  TAtomCategory = class(TAtomCommonAttributes)
  private
    FXML:WideString;
    function getXMLObj:IXMLDOMDocument2;
  public
    Term:WideString;
    CategoryLabel:WideString;
    Scheme:TAtomUri;
    property  asXMLObj : IXMLDOMDocument2 read getXMLObj;
    constructor Create;overload;
    constructor Create(fromXML:WideString; baseUri:string);overload;
    destructor Destroy; override;
  end;

  TAtomCategoryList = class(TList)
  private
    function Get(Index: Integer): TAtomCategory;
    procedure Put(Index: Integer; const Value: TAtomCategory);
  public
    destructor Destroy; override;
    property Items[Index: Integer]: TAtomCategory read Get write Put; default;
  end;

Type
  TAtomCategories = class(TAtomCommonAttributes)
  private
    FXML:WideString;
  public
    Fixed:boolean;
    Scheme:TAtomUri;
    Href:TAtomUri;
    CategoryList:TAtomCategoryList;
    constructor Create;
    procedure setCategories(fromXML:WideString;baseUri:string);
    destructor Destroy; override;
    function asXMLObj(recurse:boolean=true):IXMLDOMDocument2;
  end;

type
  TAtomContent = class(TAtomCommonAttributes)
  public
    ContentType:string;
    Text:WideString;
    Src:TAtomUri;
    constructor Create;
    //TODO:constructor Create(fromXML:WideString; baseUri:string);overload;
    destructor Destroy; override;
  end;

type
  TAtomLink = class(TAtomCommonAttributes)
  private
    FXML:WideString;
    function getXMLObj:IXMLDOMDocument2;
  public
    Rel:string; //[edit,edit-media],enclosure,via,self,related,alternate,etc
    ContentType:string;
    Href:TAtomUri;
    Length:WideString;
    Title:WideString;
    Hreflang:string;
    property  asXMLObj : IXMLDOMDocument2 read getXMLObj;
    constructor Create;
    destructor Destroy; override;
  end;

  TAtomLinkList = class(TList)
  private
    function Get(Index: Integer): TAtomLink;
    procedure Put(Index: Integer; const Value: TAtomLink);
  public
    destructor Destroy; override;
    property Items[Index: Integer]: TAtomLink read Get write Put; default;
  end;   
     
     */


/*
 type
 TAtomEntry = class(TAtomCommonAttributes)
  private
    FXML:WideString;
    function getXMLObj:IXMLDOMDocument2;
    function getEditUri:string;
    function getEditMediaUri:string;
    procedure setEditUri(strUri:string);
    //TODO: this should be in different place, eg ..
    function getObjNodeFromXML(XML:WideString):IXMLDOMNode;
  public
    Id:string;
    Title:TAtomTextConstruct;
    Links: TAtomLinkList;
    Authors:TAtomPersonList;
    Contributors:TAtomPersonList;
    Content:TAtomContent;
    Summary:TAtomTextConstruct;
    Categories:TAtomCategoryList;
    DateTimeUpdated:TDateTime;
    DateTimePublished:TDateTime;
    DateTimeEdited:TDateTime;
    Rights:TAtomTextConstruct;
    Source:WideString;
    PubControl:TAtomPubControl;
    constructor Create(baseUri:string = '');overload;
    constructor Create(fromXML:WideString; baseUri:string);overload;
    destructor Destroy; override;
    property  EditUri : string read getEditUri write setEditUri;
    property  EditMediaUri : string read getEditMediaUri;
    property  asXMLObj : IXMLDOMDocument2 read getXMLObj;

 end;    
     */


/*
 type
  TAtomFeed = class(TAtomCommonAttributes)
  private
    FXML:WideString;
    function getFirstUri:string;
    function getPrevUri:string;
    function getNextUri:string;
    function getLastUri:string;
    function getSelfUri:string;
  public
    Id:string;
    Title:TAtomTextConstruct;
    SubTitle:TAtomTextConstruct;
    Links: TAtomLinkList;
    Generator:string;
    Icon:TAtomUri;
    Logo:TAtomUri;
    Entries:TAtomEntryList;
    Authors:TAtomPersonList;
    Contributors:TAtomPersonList;
    Categories:TAtomCategoryList;
    DateTimeUpdated:TDateTime;
    Rights:TAtomTextConstruct;
    function asXMLObj(recurse:boolean=true):IXMLDOMDocument2;
    function mergeEntriesFromFeed(feed:TAtomFeed):TAtomFeed;
    constructor Create(baseUri:string = '');overload;
    constructor Create(fromXML:WideString; baseUri:string);overload;
    destructor Destroy; override;
    property  prevUri : string read getPrevUri;
    property  nextUri : string read getNextUri;
    property  firstUri : string read getFirstUri;
    property  lastUri : string read getLastUri;
    property  selfUri : string read getSelfUri;
  end;   
     */


/*
<?xml version="1.0" encoding="utf-8"?>
<feed xmlns="http://www.w3.org/2005/Atom">
<title type="text">dive into mark</title>
<subtitle type="html">
A &lt;em&gt;lot&lt;/em&gt; of effort
went into making this effortless
</subtitle>
<updated>2005-07-31T12:29:29Z</updated>
<id>tag:example.org,2003:3</id>
<link rel="alternate" type="text/html"
hreflang="en" href="http://example.org/"/>
<link rel="self" type="application/atom+xml"
href="http://example.org/feed.atom"/>
<rights>Copyright (c) 2003, Mark Pilgrim</rights>
<generator uri="http://www.example.com/" version="1.0">
Example Toolkit
</generator>
<entry>
<title>Atom draft-07 snapshot</title>
<link rel="alternate" type="text/html"
href="http://example.org/2005/04/02/atom"/>
<link rel="enclosure" type="audio/mpeg" length="1337"
href="http://example.org/audio/ph34r_my_podcast.mp3"/>
<id>tag:example.org,2003:3.2397</id>
<updated>2005-07-31T12:29:29Z</updated>
<published>2003-12-13T08:29:29-04:00</published>
<author>
<name>Mark Pilgrim</name>
<uri>http://example.org/</uri>
<email>f8dy@example.com</email>
</author>
<contributor>
<name>Sam Ruby</name>
</contributor>
<contributor>
<name>Joe Gregorio</name>
</contributor>
<content type="xhtml" xml:lang="en"
xml:base="http://diveintomark.org/">
<div xmlns="http://www.w3.org/1999/xhtml">
<p><i>[Update: The Atom draft is finished.]</i></p>
</div>
</content>
</entry>
</feed>
*/
