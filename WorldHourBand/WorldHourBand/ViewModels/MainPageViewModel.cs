using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Band;
using System.ComponentModel;
using Microsoft.Band.Tiles.Pages;
using Microsoft.Band.Tiles;

namespace WorldHourBand.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Fields
        /// <summary>
        /// representing the info of the current Band that we are connected to.
        /// </summary>
        private IBandInfo currentBandInfo;

        /// <summary>
        /// representing the band client that we are working with.
        /// </summary>
        private IBandClient currentBandClient;


        private string bandInfo;
        private string bandLiveInfo;
        private string message;

        #endregion

        #region Properties
        /// <summary>
        /// Implement the INotifyPropertyChanged interface for databinding with the main page
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// 
        /// </summary>
        public string BandInfo
        {
            get
            {
                return bandInfo;
            }

            set
            {
                bandInfo = value;
                OnPropertyChanged("BandInfo");
            }
        }


        public string BandLiveInfo
        {
            get
            {
                return bandLiveInfo;
            }

            set
            {
                bandLiveInfo = value;
                OnPropertyChanged("BandLiveInfo");
            }
        }

        public string Message
        {
            get
            {
                return message;
            }

            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }
        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor for the MainPageViewModel.
        /// </summary>
        public MainPageViewModel()
        {

        }
        #endregion

        #region methods

        public async Task TestGetBandInfo()
        {
            //Get a list of paired band
            var pairedBands = await BandClientManager.Instance.GetBandsAsync();

            var tempBand = pairedBands[0];

            BandInfo = tempBand.Name + " "  + tempBand.ConnectionType.ToString();

            return;
        }


        public async Task TestGetBandLiveInfo()
        {

            //check if we already have a connection to a band or not.
            if (currentBandInfo == null)
            {
                //Get a list of paired band
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();


                if (pairedBands.Count() < 1)
                {
                    //something is wrong.
                    BandLiveInfo = "Can't find your band info. Did you pair it?";
                    return;
                }

                //Get the band info 
                currentBandInfo = pairedBands[0];
            }


            try
            {
                //Try to establish connection to the band
                currentBandClient = await BandClientManager.Instance.ConnectAsync(currentBandInfo);

                BandLiveInfo = "Band Firmware Version: " + (await currentBandClient.GetFirmwareVersionAsync()) + "  Hardware Version: "  + (await currentBandClient.GetHardwareVersionAsync());

                return;
            }

            catch (BandException ex)
            {
                //something went wrong.
                BandLiveInfo = "Can't find your band. Please keep that in range";
            }

        }

        public async Task TestPutTileBand()
        {
            //Manage connection to the band.
            bool connected = await InitializeBandConnection();

            if (connected == false)
            {
                Message = "Can't connect to Band!";
                return;
            }


            //if reach here , mean we have something to work with.


            // We'll create a Tile that looks like this:
            // +--------------------+
            // | MY CARD            | 
            // | |||||||||||||||||  | 
            // | 123456789          |
            // +--------------------+

            //Prepare the layout for the Tile page described above
            Microsoft.Band.Tiles.Pages.TextBlock myCardTextBlock = new TextBlock();

            //Initialize the style of the textBlock.

            //Get the Color for the text block.
            myCardTextBlock.Color = Windows.UI.Colors.Cyan.ToBandColor();       //ToBandColor() is a method extension from the Band SDK to conver Windows.UI color to the BandColor.

            //Initialize the ID of the TextBlock element, will be used later to set its text to "My Card"
            myCardTextBlock.ElementId = 1;

            //Rectangle?
            myCardTextBlock.Rect = new PageRect(0, 0, 200, 25);


            //Barcode for the page.
            Barcode barcode = new Barcode(BarcodeType.Code39);

            //set the element id for the barcode.
            barcode.ElementId = 2;

            //Rectangle again?
            barcode.Rect = new PageRect(0 ,0, 250, 50);


            //ANother Textblock for the number line.
            TextBlock digitsTextBlock = new TextBlock();

            //Initialize the style of the textblock again.
            digitsTextBlock.ElementId = 3;
            digitsTextBlock.Rect = new PageRect(0, 0, 200, 25);


            //Create a flow panel.
            //Description from SDK: Arranges its child elements sequentially on the page, either horizontally or vertically
            //The constructors add for a list of items to be added to the flow panel.
            Microsoft.Band.Tiles.Pages.FlowPanel bandTileFlowPanel = new FlowPanel(myCardTextBlock, barcode, digitsTextBlock);

            //Set the orientation of the flow panel to flow vertically.
            bandTileFlowPanel.Orientation = FlowPanelOrientation.Vertical;

            //rect again?
            bandTileFlowPanel.Rect = new PageRect(0, 0, 250, 100);


            //Now we actually create a tile.
            
            //Get a Globally-Unique ID for the tile.
            Guid myTileID = new Guid("D781F673-6D05-4D69-BCFF-EA7E706C3418");

            //Actual Tile.
            Microsoft.Band.Tiles.BandTile myTile = new BandTile(myTileID);

            //Set the properties.
            myTile.Name = "My Own Tile";

            //Set the icon following the sample icon that we have from the SDK.
            myTile.TileIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconLarge.png");
            myTile.SmallIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconSmall.png");


            //Create a new page layout from the FlowPanel that we have all of our stuffs in it. (page 36 SDK)
            Microsoft.Band.Tiles.Pages.PageLayout myTilePageLayout = new PageLayout(bandTileFlowPanel);

            //Set the just-created page layout to the tile. (as our tile has only 1 page)
            myTile.PageLayouts.Add(myTilePageLayout);


            //EXTRA:
            //This will remove the previous tile that was on the phone, incase of debugging to start fresh.
            //Remove by ID will be the safest, since every GuID is unique.
            await currentBandClient.TileManager.RemoveTileAsync(myTile.TileId);


            //Tile is completed. Sync that over to phone.
            await currentBandClient.TileManager.AddTileAsync(myTile);


            //If reached here, means the tile was sent correctly.
            //Now update that manually with data.
            //we update stuffs on the band tile with PageData.
            Microsoft.Band.Tiles.Pages.PageData bandCustomPagedata = new PageData(
                Guid.NewGuid(), // The ID for the page?
                0, // The index of the layout/page to be used. We only have 1 layout/page. (Can add up to 5)
                new TextBlockData(myCardTextBlock.ElementId.Value, "Scotty's Card"),            //from here and now on, is the data for the controls that we have on the layout/page.
                new BarcodeData(barcode.BarcodeType, barcode.ElementId.Value, "11352927"),
                new TextBlockData(digitsTextBlock.ElementId.Value, "11352927"));


            //Done creating the custom data, add it over.
            await currentBandClient.TileManager.SetPagesAsync(myTile.TileId, bandCustomPagedata);

            //Echo over to the message bar.
            Message = "Done";

        }

        /// <summary>
        /// Initialize the connection to the band. Default will be the first one in the paired Band 
        /// </summary>
        /// <returns> True indicate success , False indicate failure</returns>
        public async Task<bool> InitializeBandConnection()
        {

            //check if we already have a connection to a band or not.
            if (currentBandInfo == null)
            {
                //Get a list of paired band
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();

                
                if(pairedBands.Count() < 1)
                {
                    //something is wrong.
                    return false;
                }

                //Get the band info 
                currentBandInfo = pairedBands[0];
            }

            
            try
            {
                //Try to establish connection to the band
                currentBandClient = await BandClientManager.Instance.ConnectAsync(currentBandInfo);

                if(currentBandClient == null)
                {
                    return false;
                }

                return true;
            }

            catch(BandException ex)
            {
                //something went wrong.
                return false;
            }   

        } 

        /// <summary>
        /// Invoke the PropertyChanged event from the INotifyPropertyChanged Interface with a custom message.
        /// </summary>
        /// <param name="message"> Name of the property that got changed</param>
        public void OnPropertyChanged(string message)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(message));
            }
        }


        /// <summary>
        /// Load up an icon (png) file and convert it to an actual BandIcon object to be used with creating a new tile.
        /// </summary>
        /// <param name="url"> Url to the icon</param>
        /// <returns></returns>
        private async Task<BandIcon> LoadIcon(string url)
        {

            //Get the Image file from disk.
            Windows.Storage.StorageFile iconFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(url));

            //Open up the file for random access (but stay with read>)
            using (Windows.Storage.Streams.IRandomAccessStream fileStream = await iconFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                //Create a new bitmap 
                Windows.UI.Xaml.Media.Imaging.WriteableBitmap iconWriteableBitmap = new Windows.UI.Xaml.Media.Imaging.WriteableBitmap(1, 1);

                //Process the picture to useable bitmap.
                await iconWriteableBitmap.SetSourceAsync(fileStream);

                //Extension method is written to convert a bitmap to an BandIcon that we can use with the Band
                return iconWriteableBitmap.ToBandIcon();
            }
        }

        #endregion


    }
}
