using Newtonsoft.Json;

namespace ScrapperWebAPI.Models.Zara.Product
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AbTesting
    {
        public string appsClientKey { get; set; }
        public bool enabled { get; set; }
        public List<object> enabledChannels { get; set; }
        public string webMobileClientKey { get; set; }
        public string webStandardClientKey { get; set; }
    }

    public class AccessibilityAid
    {
        public EnabledChannels enabledChannels { get; set; }
    }

    public class AccountVerification
    {
        public GuestRegister guestRegister { get; set; }
        public PhoneLegalText phoneLegalText { get; set; }
        public RegisteredUser registeredUser { get; set; }
        public List<object> registrationProcessActiveChannels { get; set; }
        public List<object> registrationProcessV2ActiveChannels { get; set; }
        public string registrationProcessV2VerificationMethod { get; set; }
        public List<object> registeredActiveChannels { get; set; }
        public List<object> registrationProcessCaptchaActiveChannels { get; set; }
        public List<object> phoneLegalTextActiveChannels { get; set; }
    }

    public class AddressSearchEngine
    {
        public Daum daum { get; set; }
    }

    public class AdWords
    {
        public string accountId { get; set; }
        public string baseImageUrl { get; set; }
        public string color { get; set; }
        public bool enabled { get; set; }
        public string format { get; set; }
        public string label { get; set; }
        public string scriptUrl { get; set; }
        public string scriptUrlAsync { get; set; }
    }

    public class AlternatesDatum
    {
        public string lang { get; set; }
        public string href { get; set; }
    }

    public class AnalyticsData
    {
        public string appVersion { get; set; }
        public string pageType { get; set; }
        public Page page { get; set; }
        public string trackerUA { get; set; }
        public string anonymizeIp { get; set; }
        public string hostname { get; set; }
        public int catGroupId { get; set; }
        public string catIdentifier { get; set; }
        public string categoryName { get; set; }
        public string colorCode { get; set; }
        public int mainPrice { get; set; }
        public string colorRef { get; set; }
        public int productId { get; set; }
        public string productRef { get; set; }
        public string productName { get; set; }
        public string section { get; set; }
        public string stylingId { get; set; }
        public string family { get; set; }
        public string subfamily { get; set; }
        public int catentryId { get; set; }
        public int brand { get; set; }
        public bool lowOnStockProduct { get; set; }
    }

    public class AnalyticsPlugin
    {
        public Zenit zenit { get; set; }
    }

    public class AppLinks
    {
        public string ios { get; set; }
        public string android { get; set; }
    }

    public class Attribute
    {
        public string type { get; set; }
        public string identifier { get; set; }
        public string name { get; set; }
        public List<string> values { get; set; }
        public Properties properties { get; set; }
    }

    public class AttributeList
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class AZN
    {
        public string currency { get; set; }
        public string symbol { get; set; }
        public string currencyFormat { get; set; }
        public int currencyDecimals { get; set; }
        public string currencyCode { get; set; }
        public string currencySymbol { get; set; }
        public double currencyRateToEuro { get; set; }
        public Formats formats { get; set; }
    }

    public class BillingCountry
    {
        public string countryCode { get; set; }
        public string name { get; set; }
        public List<object> compulsoryDocumentTypesForLegalReasons { get; set; }
    }

    public class BlockSerialReturners
    {
        public List<object> enabledChannels { get; set; }
    }

    public class Bounds
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    public class Brand
    {
        public int brandId { get; set; }
        public int brandGroupId { get; set; }
        public string brandGroupCode { get; set; }
    }

    public class BreadCrumb
    {
        public string text { get; set; }
        public int id { get; set; }
        public string keyword { get; set; }
        public int seoCategoryId { get; set; }
        public string layout { get; set; }
    }

    public class BreadCrumb3
    {
        public string text { get; set; }
        public int id { get; set; }
    }

    public class Cart
    {
        public List<object> enabledSections { get; set; }
        public string provider { get; set; }
        public int timeout { get; set; }
        public List<object> enabledChannels { get; set; }
        public List<object> multitenantEnabledChannels { get; set; }
    }

    public class CartCompositionExtraDetail
    {
        public List<object> enabledChannels { get; set; }
    }

    public class CartRelatedProducts
    {
        public bool isEnabled { get; set; }
        public List<string> supportedChannels { get; set; }
        public int maxItems { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string layout { get; set; }
        public string contentType { get; set; }
        public string gridLayout { get; set; }
        public string sectionName { get; set; }
        public Seo seo { get; set; }
        public List<AttributeList> attributeList { get; set; }
        public List<object> xmedia { get; set; }
        public Topbar topbar { get; set; }
        public List<SeoCloud> seoCloud { get; set; }
        public List<SeoCloudSection> seoCloudSection { get; set; }
        public bool isMediaSwipeable { get; set; }
    }

    public class Category2
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<object> xmedia { get; set; }
        public Seo seo { get; set; }
        public List<object> subcategories { get; set; }
        public List<object> attributes { get; set; }
    }

    public class CategoryGrid
    {
        public WebMobile webMobile { get; set; }
    }

    public class ChannelConfigs
    {
        public Standard standard { get; set; }
        public Mobile mobile { get; set; }
        public Harmony harmony { get; set; }
    }

    public class Chat
    {
        public List<int> integratedChatLangIds { get; set; }
        public string integratedChatUrl { get; set; }
        public bool isChatEnabled { get; set; }
        public bool isMochatEnabled { get; set; }
        public string itxWebChatMainUrl { get; set; }
        public string registeredChatBasePath { get; set; }
        public DynamicsChatConfig dynamicsChatConfig { get; set; }
        public bool isDynamicsChatEnabled { get; set; }
    }

    public class ChatConfig
    {
        public string zaraApiBaseUrl { get; set; }
        public string chatServerUrl { get; set; }
        public string mochatApiBaseUrl { get; set; }
    }

    public class Checkout
    {
        public bool isEnabled { get; set; }
        public string summary { get; set; }
        public bool disabledCartContinue { get; set; }
        public bool forceShippingMethodSelection { get; set; }
        public QuickPurchase quickPurchase { get; set; }
        public BlockSerialReturners blockSerialReturners { get; set; }
        public PostPayment postPayment { get; set; }
        public GenericPunchout genericPunchout { get; set; }
        public DeliveryGroupsEnabled deliveryGroupsEnabled { get; set; }
        public GetPurchaseAttempt getPurchaseAttempt { get; set; }
        public DeliveryGroup deliveryGroup { get; set; }
        public IsApplePayEnabled isApplePayEnabled { get; set; }
        public WalletPayment walletPayment { get; set; }
    }

    public class ClickToCall
    {
        public string clickToCallBaseUrl { get; set; }
        public List<object> clickToCallLangs { get; set; }
        public List<object> clickToCallLangsId { get; set; }
    }

    public class ClientAppConfig
    {
        public string appAssetsBasePath { get; set; }
        public int clientSideNavigationTimeout { get; set; }
        public string env { get; set; }
        public WCSApi WCSApi { get; set; }
        public StoreFrontApi storeFrontApi { get; set; }
        public string wechatAppId { get; set; }
        public AppLinks appLinks { get; set; }
        public string channel { get; set; }
        public CookiesConsent cookiesConsent { get; set; }
        public FormatterConfig formatterConfig { get; set; }
        public FormatterConfigByCur formatterConfigByCur { get; set; }
        public string imageBaseUrl { get; set; }
        public bool isDevEnv { get; set; }
        public bool isOpenProductPageInNewTab { get; set; }
        public bool isSsl { get; set; }
        public string langCode { get; set; }
        public int langId { get; set; }
        public string locale { get; set; }
        public string languageTag { get; set; }
        public string originalUrl { get; set; }
        public bool requestWebpResources { get; set; }
        public bool useXmedia3dAdvancedConsumerUrl { get; set; }
        public Sem sem { get; set; }
        public string storeCode { get; set; }
        public string storeCountryCode { get; set; }
        public int storeId { get; set; }
        public UniversalLinks universalLinks { get; set; }
        public string version { get; set; }
        public string videoBaseUrl { get; set; }
        public List<XmediaFormat> xmediaFormats { get; set; }
        public List<XmediaTransformation> xmediaTransformations { get; set; }
        public bool usingDefaultStore { get; set; }
        public Store store { get; set; }
        public I18nConfig i18nConfig { get; set; }
        public Domains domains { get; set; }
        public ServerPorts serverPorts { get; set; }
        public ZenitEndpoints zenitEndpoints { get; set; }
        public O11y o11y { get; set; }
        public ChatConfig chatConfig { get; set; }
        public GrowthBook growthBook { get; set; }
        public AnalyticsPlugin analyticsPlugin { get; set; }
        public Seo seo { get; set; }
    }

    public class ClientTelemetry
    {
        public List<string> pageViewsEnabledChannels { get; set; }
        public List<string> addToCartEnabledChannels { get; set; }
        public List<string> purchaseConfirmedEnabledChannels { get; set; }
    }

    public class Color
    {
        public string id { get; set; }
        public string hexCode { get; set; }
        public int productId { get; set; }
        public string name { get; set; }
        public int price { get; set; }
        public List<Size> sizes { get; set; }
        public string description { get; set; }
        public string rawDescription { get; set; }
        public ExtraInfo extraInfo { get; set; }
        public List<object> customizations { get; set; }
        public List<object> tagTypes { get; set; }
        public Pricing pricing { get; set; }
        public List<MainImg> mainImgs { get; set; }
    }

    public class Component
    {
        public string material { get; set; }
        public string percentage { get; set; }
    }

    public class Contact
    {
        public List<object> enabledChannels { get; set; }
    }

    public class ContactLinkInHelpMenu
    {
        public List<string> enabledChannels { get; set; }
    }

    public class Content
    {
        public string content { get; set; }
    }

    public class ConversionIntegration
    {
        public AdWords adWords { get; set; }
        public DoubleClick doubleClick { get; set; }
        public Facebook facebook { get; set; }
        public Yahoo yahoo { get; set; }
    }

    public class ConversionRate
    {
        public string from { get; set; }
        public string to { get; set; }
        public double rate { get; set; }
    }

    public class Cookies
    {
        public CookiesConsent cookiesConsent { get; set; }
    }

    public class CookiesConfig
    {
        public CookiesConsent cookiesConsent { get; set; }
    }

    public class CookiesConsent
    {
        public bool isEnabled { get; set; }
        public string oneTrustId { get; set; }
        public List<string> enabledChannels { get; set; }
        public OneTrust oneTrust { get; set; }
    }

    public class CountriesInfo
    {
        public List<BillingCountry> billingCountries { get; set; }
        public List<ShippingCountry> shippingCountries { get; set; }
    }

    public class Currency
    {
        public string code { get; set; }
    }

    public class Currency2
    {
        public string currencyCode { get; set; }
        public string currencySymbol { get; set; }
        public string currencyFormat { get; set; }
        public int currencyDecimals { get; set; }
        public List<ConversionRate> conversionRates { get; set; }
    }

    public class Date
    {
        public string shortDate { get; set; }
        public string longDate { get; set; }
    }

    public class Daum
    {
        public string clientServiceUrl { get; set; }
    }

    public class Default
    {
        public bool isEnabled { get; set; }
    }

    public class DeliveryGroup
    {
        public string defaultVariant { get; set; }
        public PostShipping postShipping { get; set; }
        public ShippingByDelivery shippingByDelivery { get; set; }
    }

    public class DeliveryGroupsEnabled
    {
        public List<string> supportedChannels { get; set; }
    }

    public class Desktop
    {
        public Dynamic dynamic { get; set; }
        public Static @static { get; set; }
        public Ports ports { get; set; }
    }

    public class Detail
    {
        public string reference { get; set; }
        public string displayReference { get; set; }
        public List<Color> colors { get; set; }
        public string colorSelectorLabel { get; set; }
        public string multipleColorLabel { get; set; }
        public DetailedComposition detailedComposition { get; set; }
        public List<object> relatedProducts { get; set; }
    }

    public class DetailedComposition
    {
        public List<Part> parts { get; set; }
        public List<object> exceptions { get; set; }
    }

    public class DeviceFingerprint
    {
        public string alipayJavascriptRiskUrl { get; set; }
        public bool deviceFingerPrintFlashActive { get; set; }
        public string fraudCybersourceBasicMerchantId { get; set; }
        public bool giftcardFraudCheckActive { get; set; }
        public string hostname { get; set; }
        public string merchantId { get; set; }
        public string organizationId { get; set; }
    }

    public class DocInfo
    {
        public DateTime lastModified { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string keywords { get; set; }
        public string pageId { get; set; }
        public string seoAttributes { get; set; }
        public RelData relData { get; set; }
        public HtmlAttributes htmlAttributes { get; set; }
    }

    public class Domains
    {
        public Desktop desktop { get; set; }
    }

    public class Donation
    {
        public bool isTermsLinkEnabled { get; set; }
    }

    public class DoubleClick
    {
        public bool enabled { get; set; }
    }

    public class DropPoints
    {
        public bool showCode { get; set; }
    }

    public class Dynamic
    {
        public string @base { get; set; }
        public string cn { get; set; }
        public string xn { get; set; }
    }

    public class DynamicsChatConfig
    {
        public List<object> enabledChannels { get; set; }
        public string orgId { get; set; }
        public string orgUrl { get; set; }
        public string appId { get; set; }
    }

    public class EdgeImplementationStatus
    {
        public string webMobile { get; set; }
        public string webStandard { get; set; }
    }

    public class EguiData
    {
        public string assignTitleBaseUrl { get; set; }
        public string donationEntitiesListBaseUrl { get; set; }
        public string shoppingGuideInstructionsUrl { get; set; }
        public bool isEnabled { get; set; }
    }

    public class EnabledChannels
    {
    }

    public class EnabledStatus
    {
        public string webMobile { get; set; }
        public string webStandard { get; set; }
        public string iOS { get; set; }
        public string android { get; set; }
    }

    public class Engine
    {
        public string name { get; set; }
        public Urls urls { get; set; }
        public List<Query> query { get; set; }
    }

    public class ESpotCopyright
    {
        public string key { get; set; }
    }

    public class ESpotFooterLinks
    {
        public string type { get; set; }
        public Content content { get; set; }
    }

    public class ESpotLiveStyleSheet
    {
        public string type { get; set; }
        public Content content { get; set; }
        public string key { get; set; }
    }

    public class ESpotProductPageSpecialReturnConditions
    {
        public string type { get; set; }
        public Content content { get; set; }
        public string key { get; set; }
    }

    public class ESpotSocialNetworkFooter
    {
        public string type { get; set; }
        public Content content { get; set; }
        public string key { get; set; }
    }

    public class ESpotWebCommonScripts
    {
        public string type { get; set; }
        public Content content { get; set; }
        public string key { get; set; }
    }

    public class Exelution
    {
        public bool enabled { get; set; }
    }

    public class ExtraInfo
    {
        public string deliveryUrl { get; set; }

    }

    public class Facebook
    {
        public string accountId { get; set; }
        public bool enabled { get; set; }
        public string scriptUrl { get; set; }
    }

    public class Filtering
    {
        public EnabledStatus enabledStatus { get; set; }
        public List<string> allowedFacets { get; set; }
    }

    public class Formats
    {
        public Number number { get; set; }
        public Date date { get; set; }
    }

    public class FormatterConfig
    {
        public string currency { get; set; }
        public string symbol { get; set; }
        public string currencyFormat { get; set; }
        public int currencyDecimals { get; set; }
        public string currencyCode { get; set; }
        public string currencySymbol { get; set; }
        public double currencyRateToEuro { get; set; }
        public Formats formats { get; set; }
    }

    public class FormatterConfigByCur
    {
        public AZN AZN { get; set; }
    }

    public class FraudConfig
    {
        public bool isRiskifiedActive { get; set; }
    }

    public class FreeShippingMethod
    {
        public string textColorHexCodeLight { get; set; }
        public string textColorHexCodeDark { get; set; }
    }

    public class FullPersonalizedGrid
    {
        public List<string> enabledSections { get; set; }
        public List<string> enabledChannels { get; set; }
        public List<object> multitenantEnabledChannels { get; set; }
    }

    public class FuturePrice
    {
        public string backgroundColorHexCode { get; set; }
        public string textColorHexCode { get; set; }
        public string darkModeBackgroundColorHexCode { get; set; }
        public string darkModeTextColorHexCode { get; set; }
    }

    public class GenericPunchout
    {
        public List<object> supportedChannels { get; set; }
    }


    public class GeoInfo
    {
        public Location location { get; set; }
        public Bounds bounds { get; set; }
    }

    public class GetPurchaseAttempt
    {
        public List<object> supportedChannels { get; set; }
    }

    public class GiftCardTerms
    {
        public string url { get; set; }
        public string version { get; set; }
    }

    public class Global
    {
        public List<object> enabledSections { get; set; }
        public string provider { get; set; }
        public int timeout { get; set; }
        public List<object> enabledChannels { get; set; }
        public List<object> multitenantEnabledChannels { get; set; }
    }

    public class Gmaps
    {
        public string user { get; set; }
        public string key { get; set; }
        public string channel { get; set; }
        public bool isAddressAutocompleteActive { get; set; }
        public string autocompleteKey { get; set; }
    }

    public class GoogleServices
    {
        public string key { get; set; }
    }

    public class Grid
    {
        public List<string> enabledSections { get; set; }
        public string provider { get; set; }
        public int timeout { get; set; }
        public List<string> enabledChannels { get; set; }
        public List<object> multitenantEnabledChannels { get; set; }
    }

    public class GrowthBook
    {
        public bool enabled { get; set; }
        public string clientKey { get; set; }
        public string decryptionKey { get; set; }
        public string host { get; set; }
        public bool enableDevMode { get; set; }
        public object payload { get; set; }
    }

    public class Gtm
    {
        public string accountId { get; set; }
        public bool enabled { get; set; }
    }

    public class GuestRegister
    {
        public bool isEnabled { get; set; }
        public bool showCaptcha { get; set; }
        public string verificationMethod { get; set; }
    }

    public class Harmony
    {
        public string projectKey { get; set; }
        public string projectName { get; set; }
        public string apiToken { get; set; }
    }

    public class HelpCenter
    {
        public List<string> enabledChannels { get; set; }
    }

    public class HelpSearch
    {
        public string appId { get; set; }
        public string apiKey { get; set; }
        public string indexName { get; set; }
        public int minCharsToQuery { get; set; }
        public int minMillisToQuery { get; set; }
        public bool showSuggestionWhenNoResults { get; set; }
        public List<object> articleKeyMappings { get; set; }
    }

    public class HighlightPrice
    {
        public string backgroundColorHexCode { get; set; }
        public string textColorHexCode { get; set; }
        public string darkModeBackgroundColorHexCode { get; set; }
        public string darkModeTextColorHexCode { get; set; }
    }

    public class HtmlAttributes
    {
        [JsonProperty("data-disableoverscroll")]
        public string datadisableoverscroll { get; set; }
    }

    public class I18nConfig
    {
        public bool cacheEnabled { get; set; }
        public string defaultMessage { get; set; }
        public string url { get; set; }
        public int version { get; set; }
    }

    public class Ids
    {
        public string web { get; set; }

        [JsonProperty("web-mobile")]
        public string webmobile { get; set; }
        public string ios { get; set; }
        public string android { get; set; }

        [JsonProperty("mini-p")]
        public string minip { get; set; }
    }

    public class InditexLinkedAccounts
    {
        public Default @default { get; set; }
        public Register register { get; set; }
        public Checkout checkout { get; set; }
    }

    public class InstantShipping
    {
        public List<object> enabledChannels { get; set; }
        public bool isEnabled { get; set; }
    }

    public class InteractiveSizeGuide
    {
        public List<string> enabledChannels { get; set; }
        public List<string> enabledSections { get; set; }
    }

    public class IsApplePayEnabled
    {
        public List<object> enabledChannels { get; set; }
    }

    public class ItxRestRelatedProducts
    {
        public List<string> enabledChannels { get; set; }
        public List<string> enabledSections { get; set; }
    }

    public class KeyWordI18n
    {
        public int langId { get; set; }
        public string keyword { get; set; }
        public List<string> seoUris { get; set; }
    }

    public class Legal
    {
        public TERMSANDCONDITIONS TERMS_AND_CONDITIONS { get; set; }
        public PRIVACYPOLICY PRIVACY_POLICY { get; set; }
    }

    public class LegalDocument
    {
        public string kind { get; set; }
        public string label { get; set; }
        public string url { get; set; }
        public string version { get; set; }
        public int showWarningDuringDays { get; set; }
        public List<string> visibleAt { get; set; }
    }

    public class LiveStreaming
    {
        public ScriptUrls scriptUrls { get; set; }
        public List<string> enabledChannels { get; set; }
    }

    public class LiveTracking
    {
        public List<object> enabledChannels { get; set; }
    }

    public class Locale
    {
        public string currencyCode { get; set; }
        public string currencySymbol { get; set; }
        public string currencyFormat { get; set; }
        public int currencyDecimals { get; set; }
        public double currencyRate { get; set; }
        public List<Currency> currencies { get; set; }
        public bool isBankBicMandatory { get; set; }
        public bool isBankInnMandatory { get; set; }
        public bool isBankSwift { get; set; }
        public bool isCompoundName { get; set; }
        public bool isLastNameFirst { get; set; }
    }

    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class MainImg
    {

        public string url { get; set; }
    }

    public class Meta
    {
        public List<Xmedium> xmedia { get; set; }
    }

    public class MkSpots
    {
        public ESpotCopyright ESpot_Copyright { get; set; }
        public ESpotWebCommonScripts ESpot_WebCommonScripts { get; set; }
        public ESpotLiveStyleSheet ESpot_LiveStyleSheet { get; set; }
        public object ESpot_VirtualGiftCard_Preview { get; set; }
        public ESpotProductPageSpecialReturnConditions ESpot_ProductPage_SpecialReturnConditions { get; set; }
        public ESpotSocialNetworkFooter ESpot_SocialNetwork_Footer { get; set; }
        public object ESpot_SocialNetwork_Footer_Sra { get; set; }
        public object ESpot_Footer_Links_Sra { get; set; }
    }

    public class Mobile
    {
        public string projectKey { get; set; }
        public string projectName { get; set; }
        public string apiToken { get; set; }
    }

    public class MobileApp
    {
        public string msg { get; set; }
        public string iOSUri { get; set; }
        public string androidUri { get; set; }
    }

    public class MultiWishlist
    {
        public List<string> enabledChannels { get; set; }
    }

    public class Naizfit
    {
        public string webScriptUrl { get; set; }
        public string appsScriptUrl { get; set; }
        public List<object> enabledChannels { get; set; }
    }

    public class NetworkClientMonitoring
    {
        public Web web { get; set; }
    }

    public class Newsletter
    {
        public bool isAutomaticSubscribeEnabled { get; set; }
        public bool isAutomaticUnsubscribeEnabled { get; set; }
        public bool isDetailedUnsubscribeEnabled { get; set; }
    }

    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class NotificationCenter
    {
        public bool isEnabled { get; set; }
    }

    public class Number
    {
        public string decimalSeparator { get; set; }
        public string thousandsSeparator { get; set; }
    }

    public class O11y
    {
        public ChannelConfigs channelConfigs { get; set; }
        public int bufferSize { get; set; }
        public string domain { get; set; }
        public string environment { get; set; }
        public string collectorUrl { get; set; }
    }

    public class OneTrust
    {
        public Ids ids { get; set; }
    }

    public class OnlineExchange
    {
        public bool isEnabled { get; set; }
        public List<string> enabledChannels { get; set; }
        public int maxExchangeUnitsCount { get; set; }
        public bool isMobileNewProcessForGuestAvailable { get; set; }
        public bool isNewWindowForGuestAvailable { get; set; }
        public bool isShippingEditable { get; set; }
    }

    public class OrderList
    {
        public string apiVersion { get; set; }
    }

    public class OrderProcess
    {
        public EdgeImplementationStatus edgeImplementationStatus { get; set; }
        public bool isFullBillingAddresNeeded { get; set; }
        public string restylingCheckoutStatus { get; set; }
        public string restylingCheckoutUrl { get; set; }
        public string restylingLegacyCheckoutUrl { get; set; }
    }

    public class OwnUniversePersonalizedGrid
    {
        public List<string> enabledSections { get; set; }
        public List<string> enabledChannels { get; set; }
        public List<object> multitenantEnabledChannels { get; set; }
    }

    public class Page
    {
        public string language { get; set; }
        public string shop { get; set; }
        public string currency { get; set; }
    }

    public class Part
    {
        public string description { get; set; }
        public List<object> areas { get; set; }
        public List<Component> components { get; set; }
        public List<object> microcontents { get; set; }
        public List<object> reinforcements { get; set; }
    }

    public class Payment
    {
        public string cardinalDeviceDataCollectionUrl { get; set; }
        public int creditCardExpirationMonthsThresold { get; set; }
        public bool isShowPaymentExchangeWarningEnable { get; set; }
        public string kcpBinaryInstallerUrl { get; set; }
        public string kcpJsUrl { get; set; }
        public string kcpWebPluginUrl { get; set; }
        public string klarnaWidgetUrl { get; set; }
        public int offlineExpirationDelayTime { get; set; }
    }

    public class PaymentMethodsWarning
    {
        public List<object> enabledChannels { get; set; }
        public List<object> methodsAllowed { get; set; }
    }

    public class PdpGrid
    {
        public List<string> enabledSections { get; set; }
        public string provider { get; set; }
        public int timeout { get; set; }
        public List<object> enabledChannels { get; set; }
        public List<object> multitenantEnabledChannels { get; set; }
    }

    public class PdpToast
    {
        public List<string> enabledSections { get; set; }
        public string provider { get; set; }
        public int timeout { get; set; }
        public List<object> enabledChannels { get; set; }
        public List<string> multitenantEnabledChannels { get; set; }
    }

    public class PhoneLegalText
    {
        public bool isEnabled { get; set; }
    }

    public class PhysicalStores
    {
        public bool hasPhysicalStores { get; set; }
        public List<object> storeServices { get; set; }
        public string storeServicesBaseUrl { get; set; }
        public Services services { get; set; }
    }

    public class Ports
    {
        public int plain { get; set; }
        public int ssl { get; set; }
    }

    public class PostPayment
    {
        public List<string> supportedChannels { get; set; }
    }

    public class PostShipping
    {
        public List<string> supportedChannels { get; set; }
    }

    public class Price
    {
        public int value { get; set; }
        public Currency currency { get; set; }
    }

    public class PriceColors
    {
        public SalesPrice salesPrice { get; set; }
        public FuturePrice futurePrice { get; set; }
        public HighlightPrice highlightPrice { get; set; }
    }

    public class Pricing
    {
        public Price price { get; set; }
    }

    public class Privacy
    {
        public string url { get; set; }
        public string version { get; set; }
    }

    public class PRIVACYPOLICY
    {
        public string kind { get; set; }
        public string label { get; set; }
        public string url { get; set; }
        public string version { get; set; }
        public int showWarningDuringDays { get; set; }
        public List<string> visibleAt { get; set; }
    }

    public class Product
    {
        public int id { get; set; }
        public string type { get; set; }
        public string kind { get; set; }
        public string state { get; set; }
        public Brand brand { get; set; }
        public string name { get; set; }
        public Detail detail { get; set; }
        public int section { get; set; }
        public string sectionName { get; set; }
        public int familyId { get; set; }
        public string familyName { get; set; }
        public int subfamilyId { get; set; }
        public string subfamilyName { get; set; }
        public ExtraInfo extraInfo { get; set; }
        public Seo seo { get; set; }
        public DateTime firstVisibleDate { get; set; }
        public List<object> attributes { get; set; }
        public SizeGuide sizeGuide { get; set; }
        public string sizeSystem { get; set; }
        public List<object> xmedia { get; set; }
        public List<object> productTag { get; set; }
    }

    public class ProductDetailPreloadImages
    {
        public bool enabled { get; set; }
        public string scope { get; set; }
    }

    public class ProductMetaDatum
    {
        public string sku { get; set; }
        public string name { get; set; }
        public string brand { get; set; }
        public string description { get; set; }
        public int price { get; set; }
        public string availability { get; set; }
        public string url { get; set; }
        public string sizeName { get; set; }
        public string colorName { get; set; }
    }

    public class ProductRecommendation
    {
        public bool enabled { get; set; }
    }

    public class ProductsCategoryPreloadImages
    {
        public bool enabled { get; set; }
        public int limit { get; set; }
    }

    public class ProductsSearch
    {
        public string provider { get; set; }
        public int maxPrefetchedNextQueries { get; set; }
        public SearchByImage searchByImage { get; set; }
        public Urls urls { get; set; }
        public Filtering filtering { get; set; }
        public List<Query> query { get; set; }
        public string engineName { get; set; }
        public List<Engine> engines { get; set; }
    }

    public class Profile
    {
        public bool isEnabled { get; set; }
    }

    public class Properties
    {
        public string probability { get; set; }
        public string source { get; set; }
        public string type { get; set; }
        public string typeProbability { get; set; }
    }

    public class Prosinecki
    {
        public string webScriptUrl { get; set; }
        public string appsScriptUrl { get; set; }
        public List<string> enabledChannels { get; set; }
        public List<string> enabledBrandGroups { get; set; }
        public List<string> enabledSections { get; set; }
        public List<string> enabledFamilies { get; set; }
    }

    public class Qubit
    {
        public bool isQubitEnabled { get; set; }
        public string qubitScriptUrl { get; set; }
    }

    public class Query
    {
        public string name { get; set; }
        public List<string> value { get; set; }
    }

    public class QuickPurchase
    {
        public bool isEnabled { get; set; }
        public List<string> supportedChannels { get; set; }
    }

    public class RecommendProviderLocation
    {
        public Global global { get; set; }
        public Grid grid { get; set; }
        public PdpGrid pdpGrid { get; set; }
        public PdpToast pdpToast { get; set; }
        public Cart cart { get; set; }
        public SearchHome searchHome { get; set; }
        public FullPersonalizedGrid fullPersonalizedGrid { get; set; }
        public OwnUniversePersonalizedGrid ownUniversePersonalizedGrid { get; set; }
    }

    public class Register
    {
        public bool isEnabled { get; set; }
    }

    public class RegisteredUser
    {
        public bool isEnabled { get; set; }
    }

    public class Relation
    {
        public List<int> ids { get; set; }
        public string type { get; set; }
    }

    public class RelData
    {
        public string canonicalUrl { get; set; }
        public List<AlternatesDatum> alternatesData { get; set; }
    }

    public class ReturnRequestForm
    {
        public List<object> enabledChannels { get; set; }
    }

    public class Returns
    {
        public PaymentMethodsWarning paymentMethodsWarning { get; set; }
        public ReturnRequestForm returnRequestForm { get; set; }
    }

    public class Rgpd
    {
        public bool isEnabled { get; set; }
        public bool showPopup { get; set; }
    }

    public class Root
    {
        public bool noIndex { get; set; }
        public Product product { get; set; }
        //public bool showNativeAppBanner { get; set; }
        //public List<ProductMetaDatum> productMetaData { get; set; }
        //public int parentId { get; set; }
        //public Category category { get; set; }
        //public List<object> categories { get; set; }
        //public List<KeyWordI18n> keyWordI18n { get; set; }
        //public DocInfo docInfo { get; set; }
        //public List<BreadCrumb> breadCrumbs { get; set; }
        //public AnalyticsData analyticsData { get; set; }
        //public ClientAppConfig clientAppConfig { get; set; }
    }

    public class SalesPrice
    {
        public string backgroundColorHexCode { get; set; }
        public string textColorHexCode { get; set; }
        public string darkModeBackgroundColorHexCode { get; set; }
        public string darkModeTextColorHexCode { get; set; }
    }

    public class ScriptUrls
    {
        public string web { get; set; }
        public string apps { get; set; }
    }

    public class SearchByImage
    {
        public bool enabled { get; set; }
        public string host { get; set; }
        public string apiKey { get; set; }
    }

    public class SearchHome
    {
        public List<string> enabledSections { get; set; }
        public string provider { get; set; }
        public int timeout { get; set; }
        public List<object> enabledChannels { get; set; }
        public List<string> multitenantEnabledChannels { get; set; }
    }

    public class Section
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<string> availableFor { get; set; }
        public string engDescription { get; set; }
    }

    public class Sem
    {
        public Exelution exelution { get; set; }
    }

    public class Seo
    {
        public string keyword { get; set; }
        public string description { get; set; }
        public List<BreadCrumb> breadCrumb { get; set; }
        public string seoProductId { get; set; }
        public int discernProductId { get; set; }
        public List<KeyWordI18n> keyWordI18n { get; set; }
        public int discernCategoryId { get; set; }
        public bool irrelevant { get; set; }
        public string metaDescription { get; set; }
        public string secondaryDescription { get; set; }
        public string secondaryHeader { get; set; }
        public int seoCategoryId { get; set; }
        public string title { get; set; }
        public bool isHiddenInMenu { get; set; }
        public ProductsCategoryPreloadImages productsCategoryPreloadImages { get; set; }
        public ProductDetailPreloadImages productDetailPreloadImages { get; set; }
    }

    public class SeoCloud
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<object> xmedia { get; set; }
        public Seo seo { get; set; }
        public List<object> subcategories { get; set; }
        public List<object> attributes { get; set; }
    }

    public class SeoCloudSection
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<object> xmedia { get; set; }
        public Seo seo { get; set; }
        public List<object> subcategories { get; set; }
        public List<object> attributes { get; set; }
    }

    public class ServerPorts
    {
        public int plain { get; set; }
        public int ssl { get; set; }
    }

    public class Services
    {
        public StockInStore stockInStore { get; set; }
    }

    public class Share
    {
        public List<object> enabledChannels { get; set; }
    }

    public class ShippingByDelivery
    {
        public List<string> supportedChannels { get; set; }
    }

    public class ShippingCountry
    {
        public string countryCode { get; set; }
        public string name { get; set; }
    }

    public class ShopcartMedium
    {
        public string datatype { get; set; }
        public int set { get; set; }
        public string type { get; set; }
        public string kind { get; set; }
        public string path { get; set; }
        public string name { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string timestamp { get; set; }
        public List<string> allowedScreens { get; set; }
        public string gravity { get; set; }
        public ExtraInfo extraInfo { get; set; }
        public string url { get; set; }
        public string metaUrl { get; set; }
    }

    public class ShowSimilar
    {
        public List<string> enabledChannels { get; set; }
        public List<object> enabledSections { get; set; }
    }

    public class Size
    {
        public string availability { get; set; }
        public int equivalentSizeId { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int price { get; set; }
    }

    public class SizeGuide
    {
        public bool enabled { get; set; }
    }

    public class SizeRecommender
    {
        public string sizeRecommenderDesktopScript { get; set; }
        public bool isSizeRecommenderEnabled { get; set; }
        public string sizeRecommenderMobileScript { get; set; }
        public string sizeRecommenderPurchaseScript { get; set; }
    }

    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Standard
    {
        public string projectKey { get; set; }
        public string projectName { get; set; }
        public string apiToken { get; set; }
    }

    public class Static
    {
        public string @base { get; set; }
        public string cn { get; set; }
        public string xn { get; set; }
    }

    public class StockInStore
    {
        public string baseUrl { get; set; }
        public List<string> enabledChannels { get; set; }
    }

    public class StockOutSubscription
    {
        public bool shouldConfirmEmail { get; set; }
    }

    public class Store
    {
        public AccountVerification accountVerification { get; set; }
        public CartCompositionExtraDetail cartCompositionExtraDetail { get; set; }
        public CartRelatedProducts cartRelatedProducts { get; set; }
        public int catalogId { get; set; }
        public string colbensonUrl { get; set; }
        public CountriesInfo countriesInfo { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public string discountDisclaimer { get; set; }
        public bool displayDiscountPercentage { get; set; }
        public bool displayFuturePrice { get; set; }
        public bool displayOriginalPrice { get; set; }
        public EguiData eguiData { get; set; }
        public bool generatePermanentSeoUrl { get; set; }
        public int giftCardStepSliderValue { get; set; }
        public int gitftCardMonthsToExpire { get; set; }
        public bool hidePriceNotAvailableMessage { get; set; }
        public int id { get; set; }
        public InditexLinkedAccounts inditexLinkedAccounts { get; set; }

        public Newsletter newsletter { get; set; }
        public bool isNewsletterUnsubscribeButtonEnabled { get; set; }
        public bool isOnlineExchangeAllowed { get; set; }
        public bool isOauthAuthenticationEnabled { get; set; }
        public bool isOpenForSale { get; set; }
        public bool isOrderCancellationRequestFormEnabled { get; set; }
        public bool isPaperlessShipmentEnabled { get; set; }
        public bool isPhonePrefixDisabled { get; set; }
        public bool isProsineckiEnabled { get; set; }
        public bool isRefundBankSearchAvailable { get; set; }
        public bool isRefundHelpEnabled { get; set; }
        public bool isRegionGroupEnabled { get; set; }
        public bool isRegisterEnabled { get; set; }
        public bool isRepentanceEnabled { get; set; }
        public bool isReturnInStoreMessageVisible { get; set; }
        public bool isReturnRequestFormProcessEnabled { get; set; }
        public bool isReturnRequestListAvailable { get; set; }
        public bool isSaleEvent { get; set; }
        public bool isShowPriceTaxMessageRequired { get; set; }
        public bool isShowTaxesRequired { get; set; }
        public bool isSocialDesign { get; set; }
        public bool isStockAccurateEnabled { get; set; }
        public bool isStockInStoresAvailable { get; set; }
        public bool isTaxIncluded { get; set; }
        public bool isTravelModeEnabled { get; set; }
        public bool isUnifiedOrderListEnabled { get; set; }
        public bool isUsePhoneAsLogonId { get; set; }
        public bool isVirtualGiftCardShareAllowed { get; set; }
        public bool isWalletAvailable { get; set; }
        public bool linkToRelatedStores { get; set; }
        public NotificationCenter notificationCenter { get; set; }
        public bool omnibusMessageEnabled { get; set; }
        public string phoneCountryCode { get; set; }
        public PhysicalStores physicalStores { get; set; }
        public Privacy privacy { get; set; }
        public string privacyUrl { get; set; }
        public RecommendProviderLocation recommendProviderLocation { get; set; }
        public List<object> relatedStores { get; set; }
        public string searchLang { get; set; }
        public List<Section> sections { get; set; }
        public List<string> sharingMethods { get; set; }
        public string stockBaseUrl { get; set; }
        public UserPreferredLanguage userPreferredLanguage { get; set; }
        public bool useXmediaRealWidth { get; set; }
        public VirtualGiftCard virtualGiftCard { get; set; }
        public AddressSearchEngine addressSearchEngine { get; set; }
        public DeviceFingerprint deviceFingerprint { get; set; }
        public GeoInfo geoInfo { get; set; }
        public GiftCardTerms giftCardTerms { get; set; }
        public InteractiveSizeGuide interactiveSizeGuide { get; set; }
        public Prosinecki prosinecki { get; set; }
    }

    public class StoreFrontApi
    {
        public string baseUrl { get; set; }
        public int defaultVersion { get; set; }
        public string clientId { get; set; }
    }

    public class Styles
    {
        public Checkout checkout { get; set; }
    }

    public class Support
    {
        public AbTesting abTesting { get; set; }
        public AccessibilityAid accessibilityAid { get; set; }
        public bool adoptLegalChangesInOrderInfo { get; set; }
        public Chat chat { get; set; }
        public ClickToCall clickToCall { get; set; }
        public Contact contact { get; set; }
        public ContactLinkInHelpMenu contactLinkInHelpMenu { get; set; }
        public bool isContactLegalMessageRequired { get; set; }
        public bool isContactPopupEnable { get; set; }
        public ConversionIntegration conversionIntegration { get; set; }
        public Donation donation { get; set; }
        public DropPoints dropPoints { get; set; }
        public List<string> forceHttps { get; set; }
        public FraudConfig fraudConfig { get; set; }
        public bool isGiftCardExpirationDisclaimerRequired { get; set; }
        public GoogleServices googleServices { get; set; }
        public Tracking tracking { get; set; }
        public Gtm gtm { get; set; }
        public OrderList orderList { get; set; }
        public HelpCenter helpCenter { get; set; }
        public List<LegalDocument> legalDocuments { get; set; }
        public List<object> documents { get; set; }
        public string miniContactAvailableContext { get; set; }
        public Naizfit naizfit { get; set; }
        public OnlineExchange onlineExchange { get; set; }
        public OrderProcess orderProcess { get; set; }
        public Payment payment { get; set; }
        public int productsCategoryNamePosition { get; set; }
        public ProductsSearch productsSearch { get; set; }
        public HelpSearch helpSearch { get; set; }
        public Qubit qubit { get; set; }
        public Rgpd rgpd { get; set; }
        public int showAndroidLegacyCartPercent { get; set; }
        public bool showPrivacyLinks { get; set; }
        public StockOutSubscription stockOutSubscription { get; set; }
        public TicketToBill ticketToBill { get; set; }
        public NetworkClientMonitoring networkClientMonitoring { get; set; }
        public WebClientPerformanceMonitoring webClientPerformanceMonitoring { get; set; }
        public WideEyes wideEyes { get; set; }
        public ItxRestRelatedProducts itxRestRelatedProducts { get; set; }
        public Checkout checkout { get; set; }
        public MultiWishlist multiWishlist { get; set; }
        public List<string> wishlistActiveChannels { get; set; }
        public List<object> wishlistOnUserMenuActiveChannels { get; set; }
        public List<string> wishlistSharingActiveChannels { get; set; }
        public List<string> buyLaterActiveChannels { get; set; }
        public CategoryGrid categoryGrid { get; set; }
        public bool isCookieMigrationEnabled { get; set; }
        public bool isNewAddressFormsEnabled { get; set; }
        public ClientTelemetry clientTelemetry { get; set; }
        public bool isSRPLSSubscriptionEnabled { get; set; }
        public Cookies cookies { get; set; }
        public CookiesConfig cookiesConfig { get; set; }
        public Returns returns { get; set; }
        public Tempe3DViewer tempe3DViewer { get; set; }
        public bool isIOSNewGridEnabled { get; set; }
        public ProductRecommendation productRecommendation { get; set; }
        public AccountVerification accountVerification { get; set; }
        public LiveStreaming liveStreaming { get; set; }
        public bool showCrossBorderPrivacyCheck { get; set; }
        public Legal legal { get; set; }
        public bool showSensitivePrivacyCheck { get; set; }
    }

    public class SupportedLanguage
    {
        public int id { get; set; }
        public string code { get; set; }
        public string locale { get; set; }
        public string name { get; set; }
        public string countryName { get; set; }
        public bool isSeoIrrelevant { get; set; }
        public bool isSeoProductIrrelevant { get; set; }
        public string direction { get; set; }
        public Formats formats { get; set; }
        public string languageTag { get; set; }
        public bool isRtl { get; set; }
    }

    public class Tempe3DViewer
    {
        public Urls urls { get; set; }
    }

    public class TERMSANDCONDITIONS
    {
        public string kind { get; set; }
        public string label { get; set; }
        public string url { get; set; }
        public string version { get; set; }
        public int showWarningDuringDays { get; set; }
        public List<string> visibleAt { get; set; }
    }

    public class TicketToBill
    {
        public string captchaUrl { get; set; }
        public string createInvoiceUrl { get; set; }
        public bool isEnabled { get; set; }
        public string ticketImageUrl { get; set; }
    }

    public class Topbar
    {
        public string header { get; set; }
        public List<Category> categories { get; set; }
    }

    public class Tracking
    {
        public List<string> finalMilestones { get; set; }
        public LiveTracking liveTracking { get; set; }
        public List<string> milestonesOrder { get; set; }
    }
     

    public class UniversalLinks
    {
        public string ios { get; set; }
        public string android { get; set; }
    }

    public class Urls
    {
        public string autocomplete { get; set; }
        public string empathize { get; set; }
        public string ping { get; set; }
        public string search { get; set; }
        public string nextQueries { get; set; }
        public string react { get; set; }
        public string standalone { get; set; }
    }

    public class UserPreferredLanguage
    {
        public Register register { get; set; }
        public Profile profile { get; set; }
    }

    public class VirtualGiftCard
    {
        public Share share { get; set; }
        public InstantShipping instantShipping { get; set; }
    }

    public class Wallet
    {
        public bool isEnabled { get; set; }
        public bool isGiftCardAvailable { get; set; }
        public bool isCreditCardAvailable { get; set; }
        public bool isAffinityCardAvailable { get; set; }
        public bool isCashPaymentAvailable { get; set; }
    }

    public class WalletPayment
    {
        public List<string> enabledChannels { get; set; }
        public List<string> supportedFlows { get; set; }
    }

    public class WCSApi
    {
        public string baseUrl { get; set; }
        public string defaultVersion { get; set; }
    }

    public class WearItWith
    {
        public List<string> enabledChannels { get; set; }
        public List<int> enabledSections { get; set; }
    }

    public class Web
    {
        public bool enabled { get; set; }
        public int interval { get; set; }
    }

    public class WebClientPerformanceMonitoring
    {
        public bool enabled { get; set; }
        public string webMobileKey { get; set; }
        public string webStandardKey { get; set; }
    }

    public class WebMobile
    {
        public int clientRows { get; set; }
        public int numPreloadMedia { get; set; }
    }

    public class WideEyes
    {
        public string apiKey { get; set; }
        public string host { get; set; }
        public ShowSimilar showSimilar { get; set; }
        public WearItWith wearItWith { get; set; }
    }

    public class Wishlist
    {
        public bool isEnabled { get; set; }
    }

    public class XmediaFormat
    {
        public string datatype { get; set; }
        public int id { get; set; }
        public int set { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string codecs { get; set; }
        public string extension { get; set; }
        public int width { get; set; }
        public bool isSeoRelevant { get; set; }
    }

    public class XmediaTransformation
    {
        public Meta meta { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

    public class Xmedium
    {
        public string url { get; set; }
    }

    public class Yahoo
    {
        public string accountId { get; set; }
        public string baseImageUrl { get; set; }
        public string conversionId { get; set; }
        public bool enabled { get; set; }
        public string label { get; set; }
        public string scriptUrl { get; set; }
    }

    public class ZaraIdQR
    {
        public bool isEnabled { get; set; }
        public List<Color> colors { get; set; }
    }

    public class Zenit
    {
        public string environment { get; set; }
    }

    public class ZenitEndpoints
    {
        public string addToCart { get; set; }
        public string click { get; set; }
        public string hit { get; set; }
        public string impressions { get; set; }
        public string log { get; set; }
        public string purchaseConfirmed { get; set; }
        public string content { get; set; }
        public string searchAssistant { get; set; }
        public string experiments { get; set; }
    }


}
