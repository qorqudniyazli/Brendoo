namespace ScrapperWebAPI.Models.MassimoModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Assembly
    {
        public string name { get; set; }
        public List<object> country { get; set; }
    }

    public class Attribute
    {
        public string id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string type { get; set; }
    }

    public class BundleColor
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class BundleProductSummary
    {
        public Detail detail { get; set; }
        public string productUrl { get; set; }
        public List<ProductUrlTranslation> productUrlTranslations { get; set; }
    }

    public class Care
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int type { get; set; }
    }

    public class CertifiedMaterials
    {
        public bool show { get; set; }
        public List<object> materials { get; set; }
    }

    public class Color
    {
    //{
    //    public string id { get; set; }
    //    public int catentryId { get; set; }
    //    public string reference { get; set; }
        public string name { get; set; }
        //public string modelHeigh { get; set; }
        //public string modelName { get; set; }
        //public string modelSize { get; set; }
        //public Image image { get; set; }
        public List<Size> sizes { get; set; }
        public bool isContinuity { get; set; }
        public List<Composition> composition { get; set; }
        public List<object> compositionByZone { get; set; }
        public CompositionDetail compositionDetail { get; set; }
        public List<object> colFilter { get; set; }
        public Sustainability sustainability { get; set; }
        public Traceability traceability { get; set; }
        public CertifiedMaterials certifiedMaterials { get; set; }
        public List<Relation> relations { get; set; }
    }

    public class Component
    {
        public int id { get; set; }
        public string material { get; set; }
        public string percentage { get; set; }
    }

    public class Composition
    {
        public string part { get; set; }
        public List<Composition> composition { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string percentage { get; set; }
    }

    public class CompositionDetail
    {
        public List<Part> parts { get; set; }
        public List<string> exceptions { get; set; }
    }

    public class Confection
    {
        public string name { get; set; }
        public List<object> country { get; set; }
    }

    public class CustomizableData
    {
        public Hotpoint hotpoint { get; set; }
        public int areaId { get; set; }
    }

    public class Detail
    {
        public string description { get; set; }
        public List<Color> colors { get; set; }
        public List<Xmedium> xmedia { get; set; }
    }

    public class DyeingPrinting
    {
        public string name { get; set; }
        public List<object> country { get; set; }
    }

    public class ExtraInfo
    {
        public string originalName { get; set; }
        public string assetId { get; set; }
        public string deliveryUrl { get; set; }
        public string deliveryPath { get; set; }
        public string url { get; set; }
        public bool? isLargeAndSmallDifferentResourcesValidForBothViews { get; set; }
        public CustomizableData customizableData { get; set; }
        public List<Hash> hash { get; set; }
    }


    public class Finish
    {
        public string name { get; set; }
        public List<object> country { get; set; }
    }

    public class Hash
    {
        public string md5Hash { get; set; }
        public string size { get; set; }
    }

    public class Hotpoint
    {
        public decimal top { get; set; }        // int əvəzinə decimal
        public decimal left { get; set; }       // int əvəzinə decimal
        public string viewport { get; set; }
        public List<object> freePositionAlign { get; set; }
        public decimal width { get; set; }      // int əvəzinə decimal
        public string attribute { get; set; }
        public RawData rawData { get; set; }
        public decimal height { get; set; }     // int əvəzinə decimal
    }

    public class Location
    {
        public List<string> mediaLocations { get; set; }
        public int location { get; set; }
    }

    public class Media
    { 
        public ExtraInfo extraInfo { get; set; }  
    }

    public class Part
    {
        public string id { get; set; }
        public string description { get; set; }
        public List<object> areas { get; set; }
        public List<Component> components { get; set; }
        public List<object> microcontents { get; set; }
        public List<object> reinforcements { get; set; }
    }

    public class Pricking
    {
        public string name { get; set; }
        public List<object> country { get; set; }
    }

    public class Product
    { 
        public string name { get; set; } 
       public List<BundleProductSummary> bundleProductSummaries { get; set; }
        
    }

    public class ProductUrlTranslation
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    
    public class Purchaser
    {
        public string level { get; set; }
        public string value { get; set; }
    }

    public class RawData
    {
        public string textOrientation { get; set; }
        public decimal referenceWidth { get; set; }   // int → decimal
        public decimal referenceHeight { get; set; }  // int → decimal
        public decimal top { get; set; }              // int → decimal
        public decimal left { get; set; }             // int → decimal
        public decimal rotation { get; set; }         // int → decimal
        public decimal width { get; set; }            // int → decimal
        public string align { get; set; }
        public decimal height { get; set; }           // int → decimal
        public string writingMode { get; set; }
    }


    public class Relation
    {
        public List<int> ids { get; set; }
        public string type { get; set; }
    }

    public class Root
    {
        public List<Product> products { get; set; }
    }

    public class Size
    { 
        public string name { get; set; }
        
        public string price { get; set; } 
        public bool isBuyable { get; set; } 
    }


    public class Sustainability
    {
        public bool show { get; set; }
        public SyntheticFiberPercentage syntheticFiberPercentage { get; set; }
    }

    public class SyntheticFiberPercentage
    {
        public string name { get; set; }
    }

    public class Traceability
    {
        public bool show { get; set; }
        public string paragraph { get; set; }
        public Weaving weaving { get; set; }
        public DyeingPrinting dyeingPrinting { get; set; }
        public Confection confection { get; set; }
        public Assembly assembly { get; set; }
        public Pricking pricking { get; set; }
        public Finish finish { get; set; }
    }

    public class Weaving
    {
        public string name { get; set; }
        public List<object> country { get; set; }
    }

    public class XmediaItem
    {
        public List<Media> medias { get; set; } 
    }

    public class XmediaLocation
    {
        public List<Location> locations { get; set; }
        public int set { get; set; }
    }

    public class Xmedium
    { 
        public List<XmediaItem> xmediaItems { get; set; }
        public string colorCode { get; set; }
        public List<XmediaLocation> xmediaLocations { get; set; }
    }
}
